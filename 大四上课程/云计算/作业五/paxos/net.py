"""TCP 传输层 + 应用层故障注入（单向延迟、抖动、丢包、分区）。

每对节点之间维护一条"我 -> 对端"的出站长连接（断线自动重连），回复也走各自的出站连接。
在 Docker/虚拟机上可改用 tc netem 在内核层注入；这里的注入器用于单机多进程实验，
并且可以在运行中通过 CTRL 报文动态调整。
"""
import asyncio
import collections
import random
import socket
import sys
import time

from . import wire

_WIN = sys.platform == "win32"
# Windows 的定时器粒度较粗，临近到期时改为让出事件循环自旋，保证亚毫秒级的延迟注入精度
_SPIN = 0.003 if _WIN else 0.0


def tune_event_loop(loop):
    """Windows 下把系统定时器精度调到 1ms，并让事件循环使用高精度时钟。"""
    if _WIN:
        try:
            import ctypes
            ctypes.windll.winmm.timeBeginPeriod(1)
        except Exception:
            pass
        loop.time = time.perf_counter


def _nodelay(transport):
    sock = transport.get_extra_info("socket")
    if sock is not None:
        try:
            sock.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, 1)
        except OSError:
            pass


class _OutProto(asyncio.Protocol):
    def __init__(self, link):
        self.link = link

    def connection_made(self, transport):
        _nodelay(transport)
        self.link._connected(transport)

    def data_received(self, data):
        pass

    def connection_lost(self, exc):
        self.link._lost()


class Link:
    """到某个对端的出站链路。"""

    def __init__(self, tp, peer, addr):
        self.tp = tp
        self.peer = peer
        self.addr = addr
        self.tr = None
        self.connected = False
        self.q = collections.deque()
        self.wake = asyncio.Event()
        self.sent = 0
        self.dropped = 0
        self._tasks = [tp.loop.create_task(self._connect_loop()),
                       tp.loop.create_task(self._pump())]

    def _connected(self, transport):
        self.tr = transport
        self.connected = True
        transport.write(wire.pack(wire.HELLO, self.tp.node_id))

    def _lost(self):
        self.tr = None
        self.connected = False
        self.q.clear()

    async def _connect_loop(self):
        loop = self.tp.loop
        while True:
            if not self.connected:
                try:
                    await loop.create_connection(lambda: _OutProto(self), *self.addr)
                except OSError:
                    pass
            await asyncio.sleep(0.05 if not self.connected else 0.2)

    def send(self, data):
        tp = self.tp
        self.sent += 1
        if not self.connected or self.peer in tp.blocked:
            self.dropped += 1
            return
        if tp.loss > 0.0 and random.random() < tp.loss:
            self.dropped += 1
            return
        delay = tp.delay
        if delay <= 0.0 and not self.q:
            self.tr.write(data)
            return
        if tp.jitter > 0.0:
            delay += random.uniform(0.0, tp.jitter)
        self.q.append((time.perf_counter() + delay, data))
        self.wake.set()

    async def _pump(self):
        q = self.q
        while True:
            if not q:
                self.wake.clear()
                await self.wake.wait()
                continue
            dt = q[0][0] - time.perf_counter()
            if dt > 0:
                if dt > _SPIN:
                    await asyncio.sleep(dt - _SPIN * 0.7)
                else:
                    await asyncio.sleep(0)
                continue
            now = time.perf_counter()
            tr = self.tr
            while q and q[0][0] <= now:
                _, data = q.popleft()
                if tr is not None:
                    tr.write(data)

    def close(self):
        for t in self._tasks:
            t.cancel()
        if self.tr is not None:
            self.tr.abort()


class InConn(asyncio.Protocol):
    """入站连接：对端节点或客户端。客户端的响应沿原连接返回。"""

    def __init__(self, tp):
        self.tp = tp
        self.parser = wire.FrameParser()
        self.tr = None

    def connection_made(self, transport):
        _nodelay(transport)
        self.tr = transport

    def data_received(self, data):
        try:
            msgs = self.parser.feed(data)
        except wire.FrameError:
            self.tr.abort()
            return
        tp = self.tp
        for m in msgs:
            if m.type == wire.HELLO:
                continue
            if m.src in tp.blocked:
                continue
            tp.on_msg(m, self)

    def send(self, data):
        if self.tr is not None and not self.tr.is_closing():
            self.tr.write(data)

    def connection_lost(self, exc):
        self.tr = None


class Transport:
    def __init__(self, node_id, addrs, on_msg, loop):
        self.node_id = node_id
        self.addrs = addrs            # {id: (host, port)}
        self.on_msg = on_msg
        self.loop = loop
        self.delay = 0.0              # 单向延迟（秒）= RTT/2
        self.jitter = 0.0
        self.loss = 0.0               # 报文丢弃概率
        self.blocked = set()          # 被分区隔开的对端
        self.links = {}
        self.server = None

    async def start(self, bind_host=None):
        host, port = self.addrs[self.node_id]
        self.server = await self.loop.create_server(
            lambda: InConn(self), bind_host or host, port, reuse_address=not _WIN)
        for peer, addr in self.addrs.items():
            if peer != self.node_id:
                self.links[peer] = Link(self, peer, addr)

    def send(self, peer, data):
        self.links[peer].send(data)

    def set_faults(self, delay_ms=None, jitter_ms=None, loss=None, block=None):
        if delay_ms is not None:
            self.delay = delay_ms / 1000.0
        if jitter_ms is not None:
            self.jitter = jitter_ms / 1000.0
        if loss is not None:
            self.loss = float(loss)
        if block is not None:
            self.blocked = set(block)

    def alive(self, peers):
        return sum(1 for p in peers if p == self.node_id or self.links[p].connected)
