"""单机多进程集群编排：每个 Paxos 节点是一个独立的操作系统进程，节点间走回环 TCP。

提供启动 / kill / 重启 / 网络故障注入 / 统计采集 / Learner 一致性校验。
"""
import asyncio
import os
import shutil
import subprocess
import sys
import time

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
sys.path.insert(0, ROOT)

from paxos import client as pc  # noqa: E402

_port_seq = [0]


def next_base_port():
    _port_seq[0] += 1
    return 21000 + (os.getpid() % 50) * 400 + (_port_seq[0] % 39) * 10


class Cluster:
    def __init__(self, n, mode, workdir, listeners=0, rtt_ms=0.0, loss=0.0, fsync=False,
                 hb_ms=50, election_ms=300, retry_ms=None, backoff_ms=5, fast_wait_ms=None,
                 batch=1, linger_ms=0, window=512, contend=0, base_port=None):
        self.n = n
        self.mode = mode
        self.workdir = workdir
        self.rtt_ms = rtt_ms
        self.loss = loss
        self.base = base_port or next_base_port()
        self.voters = list(range(1, n + 1))
        self.listener_ids = [90 + i for i in range(listeners)]
        self.addrs = {i: ("127.0.0.1", self.base + i) for i in self.voters}
        for k, i in enumerate(self.listener_ids):
            self.addrs[i] = ("127.0.0.1", self.base + n + 1 + k)
        self.retry_ms = retry_ms if retry_ms is not None else max(60.0, 3 * rtt_ms + 40)
        self.fast_wait_ms = fast_wait_ms if fast_wait_ms is not None else max(10.0, rtt_ms / 2 + 5)
        self.opts = dict(fsync=int(fsync), hb_ms=hb_ms, election_ms=election_ms,
                         retry_ms=self.retry_ms, backoff_ms=backoff_ms,
                         fast_wait_ms=self.fast_wait_ms, batch=batch, linger_ms=linger_ms,
                         window=window, contend=contend)
        self.procs = {}
        self.logs = {}
        if os.path.exists(workdir):
            shutil.rmtree(workdir, ignore_errors=True)
        os.makedirs(workdir, exist_ok=True)

    def _fmt(self, ids):
        return ",".join("%d=%s:%d" % (i, self.addrs[i][0], self.addrs[i][1]) for i in ids)

    def start(self, ids=None):
        for i in (ids or self.voters + self.listener_ids):
            self.start_node(i)

    def start_node(self, i):
        cmd = [sys.executable, "-m", "paxos.node", "--id", str(i),
               "--peers", self._fmt(self.voters), "--mode", self.mode,
               "--data-dir", os.path.join(self.workdir, "data"),
               "--delay-ms", str(self.rtt_ms / 2.0), "--loss", str(self.loss)]
        if self.listener_ids:
            cmd += ["--listeners", self._fmt(self.listener_ids)]
        for k, v in self.opts.items():
            cmd += ["--" + k.replace("_", "-"), str(v)]
        logf = open(os.path.join(self.workdir, "node-%d.log" % i), "ab")
        self.logs[i] = logf
        self.procs[i] = subprocess.Popen(cmd, cwd=ROOT, stdout=logf, stderr=logf)

    def kill(self, i):
        """硬杀进程（Windows: TerminateProcess；Linux: SIGKILL），不给任何清理机会。"""
        p = self.procs.pop(i, None)
        if p is not None:
            p.kill()
            p.wait()
        f = self.logs.pop(i, None)
        if f:
            f.close()

    def stop(self):
        for i in list(self.procs):
            self.kill(i)

    def alive_ids(self):
        return sorted(self.procs)

    async def ctrl(self, i, req, timeout=10.0):
        return await pc.ctrl(self.addrs[i], req, timeout)

    async def ctrl_all(self, req, ids=None):
        ids = ids if ids is not None else self.alive_ids()
        res = await asyncio.gather(*[self.ctrl(i, req) for i in ids], return_exceptions=True)
        return {i: r for i, r in zip(ids, res) if not isinstance(r, Exception)}

    async def set_net(self, rtt_ms=None, loss=None):
        req = {"op": "net"}
        if rtt_ms is not None:
            req["delay_ms"] = rtt_ms / 2.0
        if loss is not None:
            req["loss"] = loss
        await self.ctrl_all(req)

    async def partition(self, groups):
        """groups: [[1],[2,3]] 表示两组之间互不可达；传 None 恢复。"""
        ids = self.alive_ids()
        if not groups:
            await self.ctrl_all({"op": "net", "block": []})
            return
        side = {}
        for g in groups:
            for i in g:
                side[i] = set(g)
        await asyncio.gather(*[
            self.ctrl(i, {"op": "net", "block": [j for j in self.addrs if j not in side.get(i, {i})]})
            for i in ids])

    async def stats(self):
        return await self.ctrl_all({"op": "stats"})

    async def leader(self):
        st = await self.stats()
        for i, s in st.items():
            if s.get("is_leader"):
                return i
        return None

    async def wait_ready(self, timeout=40.0):
        """等所有节点起来；multi/fast 还要等选出 Leader 且所有投票节点都知道。"""
        t0 = time.perf_counter()
        while time.perf_counter() - t0 < timeout:
            st = await self.stats()
            if len(st) == len(self.procs):
                if self.mode == "basic":
                    await asyncio.sleep(0.3)
                    return None
                leaders = [i for i, s in st.items() if s.get("is_leader")]
                lb = st[leaders[0]]["lb_num"] if len(leaders) == 1 else 0
                if len(leaders) == 1 and all(s.get("leader") == leaders[0] for s in st.values())                         and all(st[i]["pg_num"] <= lb for i in self.voters if i in st):
                    await asyncio.sleep(0.2)
                    return leaders[0]
            await asyncio.sleep(0.1)
        raise RuntimeError("cluster not ready: %s" % self.workdir)

    async def measure_rtt(self, a=None, b=None, count=30):
        a = a or self.voters[0]
        b = b or self.voters[1]
        r = await self.ctrl(a, {"op": "ping", "peer": b, "count": count}, 30.0)
        v = sorted(r["rtt_ms"])
        return v[len(v) // 2] if v else float("nan")

    async def check_safety(self):
        """比对所有存活节点（含 listener）学到的每个实例的值，返回 (被比较的实例数, 不一致的实例数)。"""
        dumps = await self.ctrl_all({"op": "dump"})
        seen = {}
        diverged = set()
        for _i, d in dumps.items():
            for s, crc in d["slots"]:
                if s in seen and seen[s] != crc:
                    diverged.add(s)
                seen.setdefault(s, crc)
        return len(seen), len(diverged)


def aggregate(stats):
    """把各节点的计数器求和。"""
    tot = {}
    for s in stats.values():
        for k, v in s.items():
            if isinstance(v, (int, float)) and not isinstance(v, bool):
                tot[k] = tot.get(k, 0) + v
    return tot


def new_loop():
    if sys.platform == "win32":
        asyncio.set_event_loop_policy(asyncio.WindowsSelectorEventLoopPolicy())
    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    from paxos.net import tune_event_loop
    tune_event_loop(loop)
    return loop
