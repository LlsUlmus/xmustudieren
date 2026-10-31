"""实验套件。每组实验输出 data/<组名>.csv（每次运行一行）与 data/raw/<组名>/<运行名>.csv（逐请求原始数据）。

  python experiments/run.py --exp baseline          # 跑一组
  python experiments/run.py --exp all               # 顺序跑全部
  python experiments/run.py --list
"""
import argparse
import asyncio
import csv
import json
import os
import random
import sys
import time

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from harness import Cluster, ROOT, aggregate, new_loop  # noqa: E402
from paxos import client as pc  # noqa: E402

DATA = os.path.join(ROOT, "data")
RUN = os.path.join(ROOT, "run")
MODES = ("basic", "multi", "fast")
PORT_BASE = [21000]
_seq = [0]

FIELDS = ["exp", "run", "mode", "n", "rtt_ms", "rtt_measured_ms", "loss", "clients", "rate_per_node",
          "fsync", "batch", "backoff_ms", "election_ms", "contend", "duration_s",
          "ok", "failed", "throughput", "mean_ms", "p50_ms", "p95_ms", "p99_ms", "max_ms",
          "msgs", "msgs_per_op", "bytes_per_op", "conflict_rate", "rounds_per_op",
          "basic_rounds", "basic_conflicts", "fast_proposals", "fast_collisions", "fast_slowpath",
          "fast_learned", "slot_lost", "timeouts", "retransmits", "nack_rx", "elections",
          "step_downs", "dup_commits", "decided", "dropped", "fsync_count", "fsync_avg_ms",
          "slots_compared", "diverged", "safety_violations", "recovery_ms", "note"]


def port():
    _seq[0] += 1
    return PORT_BASE[0] + (_seq[0] % 60) * 12


class Out:
    def __init__(self, exp):
        self.exp = exp
        os.makedirs(os.path.join(DATA, "raw", exp), exist_ok=True)
        self.path = os.path.join(DATA, exp + ".csv")
        self.f = open(self.path, "w", newline="", encoding="utf-8")
        self.w = csv.DictWriter(self.f, fieldnames=FIELDS, extrasaction="ignore")
        self.w.writeheader()

    def row(self, row):
        row["exp"] = self.exp
        for k, v in list(row.items()):
            if isinstance(v, float):
                row[k] = round(v, 4)
        self.w.writerow(row)
        self.f.flush()
        print("[%s] %s" % (self.exp, " ".join("%s=%s" % (k, row[k]) for k in (
            "run", "ok", "failed", "throughput", "p50_ms", "p99_ms", "msgs_per_op", "conflict_rate",
            "diverged", "recovery_ms") if row.get(k) not in (None, ""))), flush=True)

    def raw(self, name, recs, limit=2000):
        if limit and len(recs) > limit:
            recs = sorted(random.sample(recs, limit))
        with open(os.path.join(DATA, "raw", self.exp, name + ".csv"), "w", encoding="utf-8") as f:
            f.write("t_start_s,latency_ms,ok,node\n")
            for r in recs:
                f.write("%.5f,%.4f,%d,%d\n" % (r[0], r[1], r[2], r[3]))

    def close(self):
        self.f.close()


def delta(a, b):
    return {k: b.get(k, 0) - a.get(k, 0) for k in b}


def metrics(mode, d, nok):
    m = {k: d.get(k, 0) for k in ("basic_rounds", "basic_conflicts", "fast_proposals", "fast_collisions",
                                  "fast_slowpath", "fast_learned", "slot_lost", "timeouts", "retransmits",
                                  "nack_rx", "elections", "step_downs", "dup_commits", "decided",
                                  "dropped", "fsync_count")}
    m["msgs"] = d.get("tx", 0)
    m["msgs_per_op"] = d.get("tx", 0) / nok if nok else float("nan")
    m["bytes_per_op"] = d.get("tx_bytes", 0) / nok if nok else float("nan")
    m["fsync_avg_ms"] = d.get("fsync_time", 0) * 1000.0 / d["fsync_count"] if d.get("fsync_count") else 0.0
    if mode == "basic":
        # 冲突率：因编号被抢而失败的轮次占全部轮次的比例
        m["conflict_rate"] = d.get("basic_conflicts", 0) / d["basic_rounds"] if d.get("basic_rounds") else 0.0
        m["rounds_per_op"] = d.get("basic_rounds", 0) / nok if nok else float("nan")
    elif mode == "fast":
        # 冲突率：直投后没能拿下该实例（需换实例重投）的提案占比
        m["conflict_rate"] = d.get("slot_lost", 0) / d["fast_proposals"] if d.get("fast_proposals") else 0.0
        m["rounds_per_op"] = d.get("fast_proposals", 0) / nok if nok else float("nan")
    else:
        m["conflict_rate"] = 0.0
        m["rounds_per_op"] = 1.0
    return m


async def run_one(out, name, mode, n, rtt=0.0, loss=0.0, clients=8, rate=0.0, dur=4.0, warm=0.5,
                  targets=None, raw_limit=2000, hooks=None, note="", timeout=5.0, extra=None,
                  before_check=None, **ck):
    """起一个集群跑一次负载并记录一行结果。hooks(cluster, bench) 返回协程列表用于故障注入。"""
    c = Cluster(n, mode, os.path.join(RUN, out.exp, name), listeners=1, rtt_ms=rtt, loss=0.0,
                base_port=port(), **ck)
    c.start()
    row = dict(run=name, mode=mode, n=n, rtt_ms=rtt, loss=loss, clients=clients if not rate else 0,
               rate_per_node=rate, fsync=int(ck.get("fsync", False)), batch=ck.get("batch", 1),
               backoff_ms=ck.get("backoff_ms", 5), election_ms=ck.get("election_ms", 300),
               contend=ck.get("contend", 0), duration_s=dur, note=note)
    try:
        await c.wait_ready()
        row["rtt_measured_ms"] = await c.measure_rtt()
        if loss:
            await c.set_net(loss=loss)          # 集群就绪后再打开丢包
            await asyncio.sleep(0.2)
        s0 = aggregate(await c.stats())
        b = pc.Bench(c.addrs, targets or c.voters, timeout=timeout)
        t_begin = time.perf_counter()
        if rate:
            recs = await b.run_open(rate, dur)
        else:
            hk = hooks(c, b) if hooks else ()
            recs = await b.run_closed(clients, dur, hk)
        elapsed = time.perf_counter() - t_begin
        s1 = aggregate(await c.stats())
        summ = pc.summarize(recs, warm, dur)
        row.update(summ)
        nok_all = sum(1 for r in recs if r[2])
        row.update(metrics(mode, delta(s0, s1), nok_all))
        if loss:
            await c.set_net(loss=0.0)
        if before_check:
            await before_check(c, b)
        await c.partition(None)
        await asyncio.sleep(max(0.6, c.retry_ms / 1000.0 * 4))
        row["slots_compared"], row["diverged"] = await c.check_safety()
        row["safety_violations"] = aggregate(await c.stats()).get("safety_violations", 0)
        if extra:
            row.update(extra(c, b, recs))
        out.raw(name, recs, raw_limit)
    except Exception as e:  # noqa: BLE001
        row["note"] = (note + " ERROR " + repr(e)).strip()
    finally:
        c.stop()
    out.row(row)
    return row


# ============================================================ 各组实验

async def exp_baseline(out):
    """轻载（开环，总到达率 60 req/s 均分到各节点）下三种机制 × 3/4/5 节点的延迟与消息开销。"""
    for n in (3, 4, 5):
        for mode in MODES:
            await run_one(out, "%s-n%d" % (mode, n), mode, n, rtt=10, rate=60.0 / n, dur=8)


async def exp_concurrency(out):
    """并发提案数扫描（闭环客户端均匀连到所有节点）。"""
    for rtt in (10, 0):
        for n in (3, 4):
            if rtt == 0 and n == 4:
                continue
            for mode in MODES:
                for cl in (1, 2, 4, 8, 16, 32, 64):
                    await run_one(out, "%s-n%d-rtt%d-c%d" % (mode, n, rtt, cl), mode, n, rtt=rtt,
                                  clients=cl, dur=4)


async def exp_rtt(out):
    """网络 RTT 扫描。"""
    for n in (3, 4):
        for mode in MODES:
            for rtt in (0, 1, 10, 50, 100):
                await run_one(out, "%s-n%d-rtt%d" % (mode, n, rtt), mode, n, rtt=rtt, clients=8,
                              dur=4 if rtt <= 10 else 8)


async def exp_loss(out):
    """RTT × 丢包率网格，寻找吞吐崩溃点。"""
    for mode in MODES:
        for rtt in (1, 10, 50, 100):
            for loss in (0.0, 0.01, 0.05, 0.10, 0.20, 0.30, 0.40):
                await run_one(out, "%s-n3-rtt%d-loss%d" % (mode, rtt, round(loss * 100)), mode, 3,
                              rtt=rtt, loss=loss, clients=8, dur=5 if rtt <= 10 else 8)
    for mode in MODES:
        for loss in (0.0, 0.01, 0.05, 0.10, 0.20, 0.30):
            await run_one(out, "%s-n4-rtt10-loss%d" % (mode, round(loss * 100)), mode, 4,
                          rtt=10, loss=loss, clients=8, dur=5)


async def exp_fsync(out):
    """fsync 开/关，以及批处理对 fsync 成本的摊薄。"""
    for rtt in (0, 10):
        for mode in MODES:
            for fs in (0, 1):
                await run_one(out, "%s-rtt%d-fsync%d" % (mode, rtt, fs), mode, 3, rtt=rtt, clients=8,
                              dur=4, fsync=bool(fs))
    for fs in (0, 1):
        for batch in (1, 4, 16, 64):
            await run_one(out, "multi-batch%d-fsync%d" % (batch, fs), "multi", 3, rtt=0, clients=64,
                          dur=4, fsync=bool(fs), batch=batch, linger_ms=1.0 if batch > 1 else 0,
                          note="batching")


async def exp_batch(out):
    """批处理优化：RTT=10ms 下不同批大小的吞吐（Leader 流水线窗口固定为 4）。"""
    for batch in (1, 2, 4, 8, 16, 32, 64):
        await run_one(out, "multi-rtt10-batch%d" % batch, "multi", 3, rtt=10, clients=128, dur=4,
                      batch=batch, linger_ms=2.0 if batch > 1 else 0, window=4, note="window=4")


async def exp_livelock(out):
    """Basic Paxos 活锁：所有 Proposer 抢同一个最低空闲实例（contend=1），改变竞争者数量与退避窗口。"""
    for rtt in (10,):
        for backoff in (0, 1, 5, 20, 80):
            for k, per in ((1, 1), (2, 1), (3, 1), (3, 2), (3, 4), (3, 8)):
                await run_one(out, "basic-rtt%d-bo%d-p%dx%d" % (rtt, backoff, k, per), "basic", 3,
                              rtt=rtt, clients=k * per, targets=list(range(1, k + 1)), dur=6,
                              backoff_ms=backoff, contend=1, timeout=4.0,
                              note="proposers=%d clients_per_proposer=%d" % (k, per))
    for backoff in (0, 5):
        for k in (1, 2, 3, 4, 5):
            await run_one(out, "basic-n5-bo%d-p%dx1" % (backoff, k), "basic", 5, rtt=10, clients=k,
                          targets=list(range(1, k + 1)), dur=6, backoff_ms=backoff, contend=1,
                          timeout=4.0, note="proposers=%d clients_per_proposer=1" % k)


async def exp_fastconflict(out):
    """Fast Paxos 冲突率 vs 到达率（开环，每个节点都是 Proposer），与 Multi-Paxos 同负载对比。"""
    for rtt, rates in ((10, (1, 2, 5, 10, 20, 40, 80, 160)), (50, (0.5, 1, 2, 5, 10, 20, 40))):
        for n in (3, 4, 5):
            if rtt == 50 and n == 5:
                continue
            for mode in ("fast", "multi"):
                for rate in rates:
                    dur = min(40.0, max(8.0, 400.0 / (rate * n)))
                    await run_one(out, "%s-n%d-rtt%d-rate%s" % (mode, n, rtt, rate), mode, n, rtt=rtt,
                                  rate=rate, dur=dur, warm=1.0)


def _gap(recs, t_from, t_to):
    """[t_from, t_to] 内相邻两次成功完成之间的最大间隔 = 不可用窗口。"""
    done = sorted(r[0] + r[1] / 1000.0 for r in recs if r[2])
    pts = [t_from] + [t for t in done if t_from <= t <= t_to] + [t_to]
    return max(b - a for a, b in zip(pts, pts[1:])) * 1000.0


async def exp_faults(out):
    """kill Acceptor / kill Leader / 网络分区 / 超过容错上限，记录逐请求时间线。"""
    T1, T2, DUR = 4.0, 9.0, 14.0

    def scenario(kind, kill_n=1):
        def hooks(c, b):
            async def h(bench):
                await asyncio.sleep(T1)
                leader = await c.leader()
                followers = [i for i in c.voters if i != leader]
                if kind == "kill_follower":
                    victims = followers[:kill_n]
                elif kind == "kill_leader":
                    victims = [leader if leader else c.voters[0]]
                elif kind == "kill_many":
                    victims = followers[:kill_n]
                else:
                    victims = []
                bench.fault = dict(leader=leader, victims=victims)
                if kind == "partition_leader":
                    iso = leader if leader else c.voters[0]
                    bench.fault["victims"] = [iso]
                    await c.partition([[iso], [i for i in c.addrs if i != iso]])
                elif kind == "partition_minority":
                    iso = followers[0]
                    bench.fault["victims"] = [iso]
                    await c.partition([[iso], [i for i in c.addrs if i != iso]])
                else:
                    for v in victims:
                        c.kill(v)
                        bench.alive.discard(v)
                bench.t_fault = time.perf_counter() - bench.t0
                await asyncio.sleep(T2 - T1)
                bench.t_heal = time.perf_counter() - bench.t0
                if kind.startswith("partition"):
                    await c.partition(None)
                else:
                    for v in victims:
                        c.start_node(v)
                    await asyncio.sleep(0.5)
                    for v in victims:
                        bench.alive.add(v)
            return [h]
        return hooks

    def extra(c, b, recs):
        tf = getattr(b, "t_fault", T1)
        th = getattr(b, "t_heal", T2)
        during = pc.summarize(recs, tf + 1.5, th)      # 故障稳定后的吞吐（跳过切换瞬间）
        before = pc.summarize(recs, 0.5, tf)
        return {"recovery_ms": _gap(recs, tf - 0.05, th),
                "note": "fault=%s t_fault=%.2f t_heal=%.2f tput_before=%.0f tput_during=%.0f heal_gap_ms=%.0f"
                        % (json.dumps(getattr(b, "fault", {})), tf, th, before["throughput"],
                           during["throughput"], _gap(recs, th, DUR - 0.2))}

    plan = [
        ("multi", 3, "kill_follower", 1), ("multi", 3, "kill_leader", 1),
        ("multi", 3, "partition_leader", 1), ("multi", 3, "partition_minority", 1),
        ("multi", 3, "kill_many", 2),
        ("multi", 4, "kill_follower", 1), ("multi", 4, "kill_leader", 1), ("multi", 4, "kill_many", 2),
        ("multi", 5, "kill_many", 2), ("multi", 5, "kill_many", 3),
        ("basic", 3, "kill_follower", 1), ("basic", 3, "kill_many", 2),
        ("fast", 3, "kill_follower", 1), ("fast", 4, "kill_follower", 1),
        ("fast", 3, "kill_leader", 1), ("fast", 4, "kill_leader", 1), ("fast", 4, "kill_many", 2),
    ]
    for mode, n, kind, k in plan:
        name = "%s-n%d-%s%s" % (mode, n, kind, k if kind == "kill_many" else "")
        await run_one(out, name, mode, n, rtt=10, clients=4 if mode != "multi" else 8, dur=DUR,
                      hooks=scenario(kind, k), extra=extra, raw_limit=0, timeout=3.0)


async def exp_recovery(out):
    """Leader 故障恢复时间 vs 选举超时：每次运行连续杀 6 次 Leader，记录每次的不可用窗口。"""
    for mode in ("multi", "fast"):
        for rtt in (10, 50):
            for el in (150, 300, 600, 1200):
                if mode == "fast" and rtt == 50:
                    continue
                hb = min(50, el / 4)
                kills = 6
                period = 3.0 + el / 1000.0 * 2
                gaps = []

                def hooks(c, b, gaps=gaps, period=period, kills=kills):
                    async def h(bench):
                        await asyncio.sleep(2.0)
                        for _ in range(kills):
                            leader = await c.leader()
                            if leader is None:
                                await asyncio.sleep(period)
                                continue
                            c.kill(leader)
                            bench.alive.discard(leader)
                            tk = time.perf_counter() - bench.t0
                            await asyncio.sleep(period * 0.6)
                            gaps.append((tk, tk + period * 0.6))
                            c.start_node(leader)
                            await asyncio.sleep(period * 0.4)
                            bench.alive.add(leader)
                    return [h]

                def extra(c, b, recs, gaps=gaps):
                    g = [_gap(recs, a - 0.02, z) for a, z in gaps]
                    return {"recovery_ms": sum(g) / len(g) if g else float("nan"),
                            "note": "recovery_each_ms=" + "/".join("%.0f" % x for x in g)}
                await run_one(out, "%s-rtt%d-el%d" % (mode, rtt, el), mode, 3, rtt=rtt, clients=6,
                              dur=2.0 + kills * period + 1.0, election_ms=el, hb_ms=hb, hooks=hooks,
                              extra=extra, raw_limit=0, timeout=6.0)


async def exp_chaos(out):
    """安全性压力测试：随机 kill/重启、分区、丢包、延迟抖动交替进行，最后核对所有 Learner。"""
    for mode in MODES:
        for n in (3, 4):
            def hooks(c, b):
                async def h(bench):
                    rnd = random.Random(1234 + n)
                    down = []
                    while True:
                        await asyncio.sleep(rnd.uniform(0.6, 1.4))
                        act = rnd.choice(["kill", "restart", "partition", "heal", "loss", "delay"])
                        if act == "kill" and len(down) < (n - 1) // 2 + 1:
                            v = rnd.choice([i for i in c.voters if i not in down])
                            c.kill(v)
                            bench.alive.discard(v)
                            down.append(v)
                        elif act == "restart" and down:
                            v = down.pop(0)
                            c.start_node(v)
                            await asyncio.sleep(0.4)
                            bench.alive.add(v)
                        elif act == "partition":
                            ids = list(c.addrs)
                            rnd.shuffle(ids)
                            k = rnd.randint(1, len(ids) - 1)
                            await c.partition([ids[:k], ids[k:]])
                        elif act == "heal":
                            await c.partition(None)
                        elif act == "loss":
                            await c.set_net(loss=rnd.choice([0.0, 0.05, 0.2]))
                        elif act == "delay":
                            await c.set_net(rtt_ms=rnd.choice([2, 10, 40]))
                        bench.nfaults = getattr(bench, "nfaults", 0) + 1
                return [h]

            async def before_check(c, b):
                # 把被杀的节点全部拉起、网络恢复正常，等追赶完成后再核对所有 Learner
                for i in c.voters:
                    if i not in c.procs:
                        c.start_node(i)
                await asyncio.sleep(1.0)
                await c.partition(None)
                await c.set_net(rtt_ms=5, loss=0.0)
                await asyncio.sleep(3.0)

            def extra(c, b, recs):
                st = getattr(b, "final_stats", {})
                return {"note": "fault_actions=%d" % getattr(b, "nfaults", 0)}

            await run_one(out, "%s-n%d" % (mode, n), mode, n, rtt=5, clients=6, dur=40,
                          hooks=hooks, extra=extra, before_check=before_check, timeout=3.0,
                          raw_limit=4000)


EXPS = {
    "baseline": exp_baseline, "concurrency": exp_concurrency, "rtt": exp_rtt, "loss": exp_loss,
    "fsync": exp_fsync, "batch": exp_batch, "livelock": exp_livelock,
    "fastconflict": exp_fastconflict, "faults": exp_faults, "recovery": exp_recovery,
    "chaos": exp_chaos,
}


async def main(args):
    names = list(EXPS) if args.exp == "all" else args.exp.split(",")
    for name in names:
        out = Out(name)
        t0 = time.time()
        try:
            await EXPS[name](out)
        finally:
            out.close()
        print("== %s done in %.0fs -> %s" % (name, time.time() - t0, out.path), flush=True)


if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("--exp", default="all")
    ap.add_argument("--port-base", type=int, default=21000)
    ap.add_argument("--list", action="store_true")
    a = ap.parse_args()
    if a.list:
        for k, f in EXPS.items():
            print("%-13s %s" % (k, (f.__doc__ or "").strip()))
        sys.exit(0)
    PORT_BASE[0] = a.port_base
    new_loop().run_until_complete(main(a))
