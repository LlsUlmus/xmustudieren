"""冒烟测试：三种模式各起一个集群，打一小段负载，核对 Learner 一致性。"""
import asyncio
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from harness import Cluster, ROOT, aggregate, new_loop  # noqa: E402
from paxos import client as pc  # noqa: E402


async def one(mode, n, rtt, loss=0.0, clients=6, listeners=1, dur=3.0, **kw):
    c = Cluster(n, mode, os.path.join(ROOT, "run", "smoke-%s-%d" % (mode, n)), listeners=listeners,
                rtt_ms=rtt, loss=loss, **kw)
    c.start()
    try:
        leader = await c.wait_ready()
        mrtt = await c.measure_rtt()
        b = pc.Bench(c.addrs, c.voters)
        recs = await b.run_closed(clients, dur)
        s = pc.summarize(recs, 0.3, dur)
        await asyncio.sleep(max(0.5, c.retry_ms / 1000 * 3))
        st = await c.stats()
        agg = aggregate(st)
        slots, div = await c.check_safety()
        pref = {x["prefix"] for x in st.values()}
        print("%-5s n=%d rtt=%s(measured %.2f) loss=%s leader=%s | ok=%d fail=%d tput=%.0f/s p50=%.2f p95=%.2f p99=%.2f"
              % (mode, n, rtt, mrtt, loss, leader, s["ok"], s["failed"], s["throughput"], s["p50_ms"],
                 s["p95_ms"], s["p99_ms"]))
        print("       msgs/op=%.1f | learners=%d slots_compared=%d diverged=%d violations=%d same_prefix=%s"
              % (agg.get("tx", 0) / max(1, len(recs)), len(st), slots, div,
                 agg.get("safety_violations", 0), len(pref) == 1))
        keys = ["basic_rounds", "basic_conflicts", "slot_lost", "fast_collisions", "fast_slowpath",
                "fast_learned", "timeouts", "retransmits", "elections", "dup_commits", "nack_rx"]
        print("      ", {k: agg[k] for k in keys if agg.get(k)})
    finally:
        c.stop()


async def main():
    args = sys.argv[1:]
    rtt = float(args[0]) if args else 0.0
    loss = float(args[1]) if len(args) > 1 else 0.0
    for mode in ("multi", "basic", "fast"):
        for n in (3, 4):
            await one(mode, n, rtt, loss)


if __name__ == "__main__":
    new_loop().run_until_complete(main())
