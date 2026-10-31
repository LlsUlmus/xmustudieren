"""Acceptor 持久化：追加写 WAL + 组提交（group commit）。

Paxos 的安全性要求 Acceptor 在回复 Promise/Accepted 之前，先把
promised_id / accepted_id / accepted_value 落盘，否则崩溃重启后可能违背自己做过的承诺。

记录格式：  type(B) slot(Q) ballot(Q) len(I) | value | crc32(I)
重放时遇到长度不足或 CRC 不符的尾部记录（写一半就崩溃）即停止，并截断。
"""
import asyncio
import os
import struct
import time
import zlib

REC_PROMISE_GLOBAL = 1   # 范围 Prepare 的承诺：promised_id 对所有实例生效
REC_PROMISE_SLOT = 2     # 单实例承诺：promised_id[slot]
REC_ACCEPT = 3           # accepted_id[slot], accepted_value[slot]（同时隐含 promised_id[slot]）

_HDR = struct.Struct("!BQQI")
_CRC = struct.Struct("!I")


def _encode(rtype, slot, ballot, value):
    head = _HDR.pack(rtype, slot, ballot, len(value))
    return head + value + _CRC.pack(zlib.crc32(head + value))


class WAL:
    def __init__(self, path, fsync=True, loop=None):
        self.path = path
        self.fsync = fsync
        self.loop = loop
        self.f = None
        self._pending = []        # [(bytes, callback)]
        self._flushing = False
        self._scheduled = False
        self.fsync_count = 0
        self.fsync_time = 0.0
        self.records = 0
        self.bytes = 0

    def replay(self):
        """读取已有 WAL，返回记录列表；随后以追加方式打开。"""
        recs = []
        good = 0
        if os.path.exists(self.path):
            with open(self.path, "rb") as f:
                data = f.read()
            pos = 0
            n = len(data)
            while n - pos >= _HDR.size:
                rtype, slot, ballot, ln = _HDR.unpack_from(data, pos)
                end = pos + _HDR.size + ln + _CRC.size
                if end > n:
                    break
                body = data[pos:pos + _HDR.size + ln]
                (crc,) = _CRC.unpack_from(data, pos + _HDR.size + ln)
                if zlib.crc32(body) != crc:
                    break
                recs.append((rtype, slot, ballot, body[_HDR.size:]))
                pos = end
            good = pos
            if good != n:
                with open(self.path, "r+b") as f:
                    f.truncate(good)
        else:
            os.makedirs(os.path.dirname(self.path) or ".", exist_ok=True)
        self.f = open(self.path, "ab", buffering=0)
        return recs

    def append(self, rtype, slot, ballot, value, callback):
        """排队一条记录；落盘（按配置 fsync）完成后调用 callback。"""
        self._pending.append((_encode(rtype, slot, ballot, value), callback))
        if not self._flushing and not self._scheduled:
            self._scheduled = True
            self.loop.call_soon(self._start_flush)

    def barrier(self, callback):
        """等到此前排队的记录全部落盘后调用 callback（用于重传报文的幂等回复）。"""
        if not self._pending and not self._flushing:
            callback()
        else:
            self._pending.append((b"", callback))

    def _start_flush(self):
        self._scheduled = False
        if self._flushing or not self._pending:
            return
        batch = self._pending
        self._pending = []
        data = b"".join(b for b, _ in batch)
        self.records += len(batch)
        self.bytes += len(data)
        self.f.write(data)
        if not self.fsync:
            # fsync 关：数据只到操作系统页缓存，进程崩溃不丢、整机掉电会丢
            for _, cb in batch:
                cb()
            return
        self._flushing = True
        fut = self.loop.run_in_executor(None, self._do_fsync)
        fut.add_done_callback(lambda _f, batch=batch: self._flushed(batch))

    def _do_fsync(self):
        t0 = time.perf_counter()
        os.fsync(self.f.fileno())
        self.fsync_time += time.perf_counter() - t0
        self.fsync_count += 1

    def _flushed(self, batch):
        self._flushing = False
        for _, cb in batch:
            cb()
        if self._pending:
            self._start_flush()

    def close(self):
        if self.f:
            try:
                self.f.close()
            except OSError:
                pass
            self.f = None
