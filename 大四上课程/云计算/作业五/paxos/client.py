"""客户端库 + 负载发生器 + 命令行。

  python -m paxos.client --peers 1=127.0.0.1:7001,... put k v
  python -m paxos.client --peers ... get k --node 2
  python -m paxos.client --peers ... status
  python -m paxos.client --peers ... bench --clients 8 --duration 5
"""
import argparse
import asyncio
import json
import random
import sys
import time

from . import wire
from .net import tune_event_loop

now = time.perf_counter


class _Proto(asyncio.Protocol):
    def __init__(self, conn):
        self.conn = conn
        self.parser = wire.FrameParser()

    def connection_made(self, transport):
        self.conn.tr = transport

    def data_received(self, data):
        for m in self.parser.feed(data):
            fut = self.conn.waiting.pop(m.aux, None)
            if fut is not None and not fut.done():
                fut.set_result(m)

    def connection_lost(self, exc):
        self.conn.tr = None
        for fut in self.conn.waiting.values():
            if not fut.done():
                fut.set_exception(ConnectionError("lost"))
        self.conn.waiting.clear()


class Conn:
    """到某个节点的一条客户端连接，支持流水线（按序号匹配响应）。"""

    def __init__(self, addr, loop):
        self.addr = addr
        self.loop = loop
        self.tr = None
        self.waiting = {}
        self.seq = random.getrandbits(40) << 20

    async def connect(self):
        await self.loop.create_connection(lambda: _Proto(self), *self.addr)
        self.tr.write(wire.pack(wire.HELLO, 0, aux=1))

    async def call(self, mtype, payload, timeout):
        if self.tr is None:
            await self.connect()
        self.seq += 1
        seq = self.seq
        fut = self.waiting[seq] = self.loop.create_future()
        self.tr.write(wire.pack(mtype, 0, aux=seq, payload=payload))
        try:
            return await asyncio.wait_for(fut, timeout)
        finally:
            self.waiting.pop(seq, None)

    async def request(self, data, timeout=5.0):
        m = await self.call(wire.CLIENT_REQ, data, timeout)
        if m.flags & wire.F_ERR:
            raise RuntimeError("rejected")
        return m.slot

    async def ctrl(self, req, timeout=10.0):
        m = await self.call(wire.CTRL, json.dumps(req).encode(), timeout)
        return json.loads(m.payload.decode())

    def close(self):
        if self.tr is not None:
            self.tr.abort()
            self.tr = None


async def ctrl(addr, req, timeout=10.0):
    c = Conn(addr, asyncio.get_event_loop())
    try:
        return await c.ctrl(req, timeout)
    finally:
        c.close()


def percentile(sorted_vals, p):
    if not sorted_vals:
        return float("nan")
    k = (len(sorted_vals) - 1) * p / 100.0
    lo = int(k)
    hi = min(lo + 1, len(sorted_vals) - 1)
    return sorted_vals[lo] + (sorted_vals[hi] - sorted_vals[lo]) * (k - lo)


class Bench:
    """闭环（clients 个并发客户端各自串行发请求）或开环（按泊松过程以固定速率到达）。

    记录每个请求的 (发起时刻, 延迟ms, 是否成功, 目标节点)。alive 是可被外部修改的可用节点集合，
    节点被 kill 后其客户端会自动切到其他节点。
    """

    def __init__(self, addrs, targets, payload=b"x" * 16, timeout=5.0):
        self.addrs = addrs
        self.targets = list(targets)
        self.alive = set(targets)
        self.payload = payload
        self.timeout = timeout
        self.records = []
        self.t0 = None
        self.stop = False

    def _pick(self, prefer):
        if prefer in self.alive:
            return prefer
        cands = sorted(self.alive)
        return random.choice(cands) if cands else prefer

    async def _closed_client(self, idx):
        loop = asyncio.get_event_loop()
        prefer = self.targets[idx % len(self.targets)]
        target = prefer
        conn = Conn(self.addrs[target], loop)
        k = 0
        while not self.stop:
            want = self._pick(prefer)
            if want != target:
                conn.close()
                target = want
                conn = Conn(self.addrs[target], loop)
            k += 1
            data = b"SET k%d-%d " % (idx, k % 64) + self.payload
            t = now()
            ok = True
            try:
                await conn.request(data, self.timeout)
            except Exception:
                ok = False
                conn.close()
                conn = Conn(self.addrs[target], loop)
                await asyncio.sleep(0.02)
            self.records.append((t - self.t0, (now() - t) * 1000.0, ok, target))
        conn.close()

    async def _one(self, conn, target):
        t = now()
        ok = True
        try:
            await conn.request(b"SET o%d " % random.getrandbits(20) + self.payload, self.timeout)
        except Exception:
            ok = False
        self.records.append((t - self.t0, (now() - t) * 1000.0, ok, target))

    async def _open_source(self, target, rate, duration):
        loop = asyncio.get_event_loop()
        conn = Conn(self.addrs[target], loop)
        await conn.connect()
        t_next = now()
        end = self.t0 + duration
        tasks = []
        while True:
            t_next += random.expovariate(rate)
            if t_next >= end:
                break
            while True:
                dt = t_next - now()
                if dt <= 0:
                    break
                await asyncio.sleep(dt - 0.002 if dt > 0.003 else 0)
            tasks.append(loop.create_task(self._one(conn, target)))
        if tasks:
            await asyncio.wait(tasks)
        conn.close()

    async def run_closed(self, clients, duration, hooks=()):
        self.t0 = now()
        tasks = [asyncio.ensure_future(self._closed_client(i)) for i in range(clients)]
        hook_tasks = [asyncio.ensure_future(h(self)) for h in hooks]
        await asyncio.sleep(duration)
        self.stop = True
        await asyncio.wait(tasks, timeout=self.timeout + 1)
        for t in tasks + hook_tasks:
            t.cancel()
        return self.records

    async def run_open(self, rate_per_target, duration):
        self.t0 = now()
        await asyncio.gather(*[self._open_source(t, rate_per_target, duration) for t in self.targets])
        return self.records


def summarize(records, t_from=0.0, t_to=None):
    recs = [r for r in records if r[0] >= t_from and (t_to is None or r[0] < t_to)]
    lat = sorted(r[1] for r in recs if r[2])
    span = (t_to if t_to is not None else (max((r[0] + r[1] / 1000.0 for r in recs), default=t_from))) - t_from
    n = len(lat)
    return {
        "ok": n,
        "failed": len(recs) - n,
        "throughput": n / span if span > 0 else 0.0,
        "mean_ms": sum(lat) / n if n else float("nan"),
        "p50_ms": percentile(lat, 50),
        "p95_ms": percentile(lat, 95),
        "p99_ms": percentile(lat, 99),
        "max_ms": lat[-1] if lat else float("nan"),
    }


def _parse_addrs(s):
    out = {}
    for part in filter(None, s.split(",")):
        nid, addr = part.split("=")
        host, port = addr.rsplit(":", 1)
        out[int(nid)] = (host, int(port))
    return out


async def _main(args):
    addrs = _parse_addrs(args.peers)
    ids = sorted(addrs)
    node = args.node or ids[0]
    if args.cmd == "put":
        c = Conn(addrs[node], asyncio.get_event_loop())
        t = now()
        try:
            slot = await c.request(("SET %s %s" % (args.key, args.value)).encode(), args.timeout)
            print("OK slot=%d %.2fms (via node %d)" % (slot, (now() - t) * 1000, node))
        except Exception as e:  # noqa: BLE001
            print("FAILED after %.0fms via node %d: %s (no quorum / node down?)"
                  % ((now() - t) * 1000, node, type(e).__name__))
    elif args.cmd == "get":
        print(json.dumps(await ctrl(addrs[node], {"op": "get", "key": args.key}), ensure_ascii=False))
    elif args.cmd == "status":
        for i in ids:
            try:
                s = await ctrl(addrs[i], {"op": "stats"}, 2.0)
                print("node %d: leader=%s is_leader=%s ballot=%s prefix=%d decided=%d tx=%d violations=%d"
                      % (i, s["leader"], s["is_leader"], s["ballot"], s["prefix"], s["ndecided"],
                         s.get("tx", 0), s.get("safety_violations", 0)))
            except Exception as e:
                print("node %d: unreachable (%s)" % (i, type(e).__name__))
    elif args.cmd == "net":
        for i in ids:
            req = {"op": "net", "delay_ms": args.rtt_ms / 2.0, "loss": args.loss}
            print(i, await ctrl(addrs[i], req))
    elif args.cmd == "bench":
        targets = [int(x) for x in args.targets.split(",")] if args.targets else ids
        b = Bench(addrs, targets, timeout=args.timeout)
        if args.rate > 0:
            recs = await b.run_open(args.rate, args.duration)
        else:
            recs = await b.run_closed(args.clients, args.duration)
        s = summarize(recs, 0.0, args.duration)
        print(json.dumps(s, indent=1))
        if args.out:
            with open(args.out, "w") as f:
                f.write("t_start_s,latency_ms,ok,node\n")
                for r in recs:
                    f.write("%.6f,%.4f,%d,%d\n" % (r[0], r[1], r[2], r[3]))


def main(argv=None):
    ap = argparse.ArgumentParser()
    ap.add_argument("--peers", required=True)
    ap.add_argument("--node", type=int, default=0)
    sub = ap.add_subparsers(dest="cmd", required=True)
    p = sub.add_parser("put"); p.add_argument("key"); p.add_argument("value")
    p.add_argument("--timeout", type=float, default=5)
    p = sub.add_parser("get"); p.add_argument("key")
    sub.add_parser("status")
    p = sub.add_parser("net", help="运行时设置应用层故障注入")
    p.add_argument("--rtt-ms", type=float, default=0); p.add_argument("--loss", type=float, default=0)
    p = sub.add_parser("bench")
    p.add_argument("--clients", type=int, default=8)
    p.add_argument("--rate", type=float, default=0, help=">0 时为开环：每个目标节点每秒到达数")
    p.add_argument("--duration", type=float, default=5)
    p.add_argument("--targets", default="")
    p.add_argument("--timeout", type=float, default=5)
    p.add_argument("--out", default="")
    args = ap.parse_args(argv)
    if sys.platform == "win32":
        asyncio.set_event_loop_policy(asyncio.WindowsSelectorEventLoopPolicy())
    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    tune_event_loop(loop)
    loop.run_until_complete(_main(args))


if __name__ == "__main__":
    main()
