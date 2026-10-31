"""由 data/*.csv 生成报告用图（report/figures/*.png）。  python experiments/plots.py"""
import os

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt  # noqa: E402
import numpy as np  # noqa: E402
import pandas as pd  # noqa: E402

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DATA = os.path.join(ROOT, "data")
FIG = os.path.join(ROOT, "report", "figures")
os.makedirs(FIG, exist_ok=True)

plt.rcParams["font.sans-serif"] = ["Microsoft YaHei", "SimHei", "Noto Sans CJK SC", "DejaVu Sans"]
plt.rcParams["axes.unicode_minus"] = False
plt.rcParams["figure.dpi"] = 130
plt.rcParams["axes.grid"] = True
plt.rcParams["grid.alpha"] = 0.3

MODES = ["basic", "multi", "fast"]
NAME = {"basic": "Basic Paxos", "multi": "Multi-Paxos", "fast": "Fast Paxos"}
COLOR = {"basic": "#d1495b", "multi": "#00798c", "fast": "#edae49"}
MARK = {"basic": "s", "multi": "o", "fast": "^"}


def load(name):
    p = os.path.join(DATA, name + ".csv")
    return pd.read_csv(p) if os.path.exists(p) else None


def save(fig, name):
    fig.tight_layout()
    fig.savefig(os.path.join(FIG, name + ".png"), bbox_inches="tight")
    plt.close(fig)
    print("saved", name)


def fig_baseline():
    d = load("baseline")
    if d is None:
        return
    fig, ax = plt.subplots(1, 3, figsize=(13, 3.8))
    ns = sorted(d.n.unique())
    w = 0.26
    for k, m in enumerate(MODES):
        s = d[d["mode"] == m].set_index("n").loc[ns]
        x = np.arange(len(ns)) + (k - 1) * w
        ax[0].bar(x, s.p50_ms, w, color=COLOR[m], label=NAME[m])
        ax[0].errorbar(x, s.p50_ms, yerr=[np.zeros(len(ns)), s.p99_ms - s.p50_ms], fmt="none",
                       ecolor="k", capsize=3, lw=1)
        ax[1].bar(x, s.mean_ms, w, color=COLOR[m])
        ax[2].bar(x, s.msgs_per_op, w, color=COLOR[m])
    for a, t, y in zip(ax, ["P50 延迟（误差线到 P99）", "平均延迟", "每条命令的网络报文数"],
                       ["ms", "ms", "报文/命令"]):
        a.set_xticks(np.arange(len(ns)))
        a.set_xticklabels(["%d 节点" % n for n in ns])
        a.set_title(t)
        a.set_ylabel(y)
    ax[0].legend()
    fig.suptitle("轻载基线：RTT=10ms，开环总到达率 60 req/s 均分到各节点", y=1.02)
    save(fig, "baseline")


def fig_concurrency():
    d = load("concurrency")
    if d is None:
        return
    for rtt in sorted(d.rtt_ms.unique()):
        dd = d[d.rtt_ms == rtt]
        fig, ax = plt.subplots(1, 4, figsize=(16, 3.8))
        for n, ls in ((3, "-"), (4, "--")):
            for m in MODES:
                s = dd[(dd["mode"] == m) & (dd.n == n)].sort_values("clients")
                if s.empty:
                    continue
                kw = dict(color=COLOR[m], marker=MARK[m], ls=ls, label="%s n=%d" % (NAME[m], n))
                ax[0].plot(s.clients, s.throughput, **kw)
                ax[1].plot(s.clients, s.p50_ms, **kw)
                ax[2].plot(s.clients, s.p99_ms, **kw)
                ax[3].plot(s.clients, s.conflict_rate * 100, **kw)
        for a, t, y in zip(ax, ["吞吐", "P50 延迟", "P99 延迟", "冲突率"], ["req/s", "ms", "ms", "%"]):
            a.set_xscale("log", base=2)
            a.set_xlabel("并发客户端数（均匀连到所有节点）")
            a.set_title(t)
            a.set_ylabel(y)
        ax[1].set_yscale("log")
        ax[2].set_yscale("log")
        ax[0].legend(fontsize=7)
        fig.suptitle("并发提案数扫描（RTT=%gms）" % rtt, y=1.02)
        save(fig, "concurrency_rtt%g" % rtt)


def fig_rtt():
    d = load("rtt")
    if d is None:
        return
    fig, ax = plt.subplots(1, 3, figsize=(13, 3.8))
    for n, ls in ((3, "-"), (4, "--")):
        for m in MODES:
            s = d[(d["mode"] == m) & (d.n == n)].sort_values("rtt_ms")
            x = s.rtt_measured_ms
            kw = dict(color=COLOR[m], marker=MARK[m], ls=ls, label="%s n=%d" % (NAME[m], n))
            ax[0].plot(x, s.throughput, **kw)
            ax[1].plot(x, s.p50_ms, **kw)
            ax[2].plot(x, s.p99_ms, **kw)
    xs = np.array([0.2, 100])
    ax[1].plot(xs, xs, color="gray", lw=0.8, ls=":", label="1×RTT")
    ax[1].plot(xs, 2 * xs, color="gray", lw=0.8, ls="-.", label="2×RTT")
    for a, t, y in zip(ax, ["吞吐（8 个闭环客户端）", "P50 延迟", "P99 延迟"], ["req/s", "ms", "ms"]):
        a.set_xscale("log")
        a.set_yscale("log")
        a.set_xlabel("实测链路 RTT (ms)")
        a.set_title(t)
        a.set_ylabel(y)
    ax[1].legend(fontsize=7)
    save(fig, "rtt")


def fig_loss():
    d = load("loss")
    if d is None:
        return
    d3 = d[d.n == 3]
    rtts = sorted(d3.rtt_ms.unique())
    fig, ax = plt.subplots(1, len(rtts), figsize=(4 * len(rtts), 3.8), sharey=False)
    for a, rtt in zip(ax, rtts):
        for m in MODES:
            s = d3[(d3["mode"] == m) & (d3.rtt_ms == rtt)].sort_values("loss")
            a.plot(s.loss * 100, s.throughput, color=COLOR[m], marker=MARK[m], label=NAME[m])
        a.set_title("RTT=%gms" % rtt)
        a.set_xlabel("报文丢失率 (%)")
        a.set_ylabel("吞吐 req/s")
        a.set_yscale("log")
    ax[0].legend(fontsize=8)
    fig.suptitle("吞吐随丢包率的变化（3 节点，8 个闭环客户端）", y=1.02)
    save(fig, "loss_throughput")

    fig, ax = plt.subplots(1, len(rtts), figsize=(4 * len(rtts), 3.8))
    for a, rtt in zip(ax, rtts):
        for m in MODES:
            s = d3[(d3["mode"] == m) & (d3.rtt_ms == rtt)].sort_values("loss")
            a.plot(s.loss * 100, s.p99_ms, color=COLOR[m], marker=MARK[m], label=NAME[m])
            a.plot(s.loss * 100, s.p50_ms, color=COLOR[m], marker=MARK[m], ls=":", alpha=0.6)
        a.set_title("RTT=%gms（实线 P99，虚线 P50）" % rtt, fontsize=9)
        a.set_xlabel("报文丢失率 (%)")
        a.set_ylabel("延迟 ms")
        a.set_yscale("log")
    ax[0].legend(fontsize=8)
    save(fig, "loss_latency")

    losses = sorted(d3.loss.unique())
    fig, ax = plt.subplots(1, 3, figsize=(14, 3.6), constrained_layout=True)
    for a, m in zip(ax, MODES):
        g = np.full((len(rtts), len(losses)), np.nan)
        for i, rtt in enumerate(rtts):
            base = d3[(d3["mode"] == m) & (d3.rtt_ms == rtt) & (d3.loss == 0)].throughput
            for j, ls in enumerate(losses):
                v = d3[(d3["mode"] == m) & (d3.rtt_ms == rtt) & (d3.loss == ls)].throughput
                if len(v) and len(base) and base.iloc[0] > 0:
                    g[i, j] = v.iloc[0] / base.iloc[0]
        im = a.imshow(g, cmap="RdYlGn", vmin=0, vmax=1, aspect="auto")
        for i in range(len(rtts)):
            for j in range(len(losses)):
                if not np.isnan(g[i, j]):
                    a.text(j, i, "%.0f%%" % (g[i, j] * 100), ha="center", va="center", fontsize=8)
        a.set_xticks(range(len(losses)))
        a.set_xticklabels(["%g%%" % (x * 100) for x in losses])
        a.set_yticks(range(len(rtts)))
        a.set_yticklabels(["%gms" % r for r in rtts])
        a.set_xlabel("丢包率")
        if m == MODES[0]:
            a.set_ylabel("RTT")
        a.set_title(NAME[m])
        a.grid(False)
    fig.colorbar(im, ax=ax, fraction=0.02, label="相对无丢包时的吞吐")
    fig.suptitle("吞吐保持率：相对同 RTT 下丢包率为 0 时的吞吐")
    fig.savefig(os.path.join(FIG, "loss_heatmap.png"), bbox_inches="tight")
    plt.close(fig)
    print("saved loss_heatmap")


def fig_fsync():
    d = load("fsync")
    if d is None:
        return
    a_ = d[d.note != "batching"]
    fig, ax = plt.subplots(1, 3, figsize=(13, 3.8))
    for k, rtt in enumerate(sorted(a_.rtt_ms.unique())):
        s = a_[a_.rtt_ms == rtt]
        x = np.arange(3)
        off = [s[(s["mode"] == m) & (s.fsync == 0)].throughput.iloc[0] for m in MODES]
        on = [s[(s["mode"] == m) & (s.fsync == 1)].throughput.iloc[0] for m in MODES]
        ax[k].bar(x - 0.2, off, 0.4, label="fsync 关", color="#8fbcbb")
        ax[k].bar(x + 0.2, on, 0.4, label="fsync 开", color="#5e81ac")
        for i in range(3):
            ax[k].text(x[i] + 0.2, on[i], "%.0f%%" % (on[i] / off[i] * 100), ha="center", va="bottom", fontsize=8)
        ax[k].set_xticks(x)
        ax[k].set_xticklabels([NAME[m] for m in MODES], fontsize=8)
        ax[k].set_title("吞吐，RTT=%gms（柱顶为开/关之比）" % rtt, fontsize=9)
        ax[k].set_ylabel("req/s")
        ax[k].legend(fontsize=8)
    b = d[d.note == "batching"]
    for fs, c in ((0, "#8fbcbb"), (1, "#5e81ac")):
        s = b[b.fsync == fs].sort_values("batch")
        ax[2].plot(s.batch, s.throughput, marker="o", color=c, label="fsync %s" % ("开" if fs else "关"))
    ax[2].set_xscale("log", base=2)
    ax[2].set_xlabel("批大小（每个实例的命令数上限）")
    ax[2].set_ylabel("req/s")
    ax[2].set_title("Multi-Paxos 批处理，RTT=0，64 客户端", fontsize=9)
    ax[2].legend(fontsize=8)
    save(fig, "fsync")


def fig_batch():
    d = load("batch")
    if d is None:
        return
    d = d.sort_values("batch")
    fig, ax = plt.subplots(1, 2, figsize=(9, 3.6))
    ax[0].plot(d.batch, d.throughput, marker="o", color=COLOR["multi"])
    ax[0].set_ylabel("吞吐 req/s")
    ax[1].plot(d.batch, d.p50_ms, marker="o", color=COLOR["multi"], label="P50")
    ax[1].plot(d.batch, d.p99_ms, marker="s", color="#555", label="P99")
    ax[1].set_ylabel("延迟 ms")
    ax[1].legend()
    for a in ax:
        a.set_xscale("log", base=2)
        a.set_xlabel("批大小")
    fig.suptitle("批处理优化：Multi-Paxos，RTT=10ms，128 客户端，流水线窗口=4", y=1.02)
    save(fig, "batch")


def _parse_note(d):
    d = d.copy()
    d["proposers"] = d.note.str.extract(r"proposers=(\d+)").astype(float)
    d["per"] = d.note.str.extract(r"clients_per_proposer=(\d+)").astype(float)
    return d


def fig_livelock():
    d = load("livelock")
    if d is None:
        return
    d = _parse_note(d)
    d3 = d[d.n == 3]
    fig, ax = plt.subplots(1, 4, figsize=(16, 3.8))
    labels = None
    cmap = plt.get_cmap("viridis")
    bos = sorted(d3.backoff_ms.unique())
    for i, bo in enumerate(bos):
        s = d3[d3.backoff_ms == bo].sort_values(["proposers", "per"])
        labels = ["%d×%d" % (p, c) for p, c in zip(s.proposers, s.per)]
        x = np.arange(len(s))
        kw = dict(marker="o", color=cmap(i / max(1, len(bos) - 1)), label="退避基数 %gms" % bo)
        ax[0].plot(x, s.throughput, **kw)
        ax[1].plot(x, s.rounds_per_op, **kw)
        ax[2].plot(x, s.conflict_rate * 100, **kw)
        ax[3].plot(x, s.p99_ms, **kw)
    for a, t, y in zip(ax, ["吞吐", "每条命令平均 Prepare 轮数", "冲突率（失败轮次占比）", "P99 延迟"],
                       ["req/s", "轮", "%", "ms"]):
        a.set_xticks(np.arange(len(labels)))
        a.set_xticklabels(labels)
        a.set_xlabel("Proposer 数 × 每个 Proposer 的并发客户端")
        a.set_title(t)
        a.set_ylabel(y)
    ax[1].set_yscale("log")
    ax[3].set_yscale("log")
    ax[0].legend(fontsize=8)
    fig.suptitle("Basic Paxos 活锁实验：3 节点，RTT=10ms，所有 Proposer 抢同一实例", y=1.02)
    save(fig, "livelock")


def fig_fastconflict():
    d = load("fastconflict")
    if d is None:
        return
    for rtt in sorted(d.rtt_ms.unique()):
        dd = d[d.rtt_ms == rtt]
        ns = sorted(dd.n.unique())
        fig, ax = plt.subplots(2, len(ns), figsize=(4.4 * len(ns), 7), squeeze=False)
        for j, n in enumerate(ns):
            f = dd[(dd["mode"] == "fast") & (dd.n == n)].sort_values("rate_per_node")
            m = dd[(dd["mode"] == "multi") & (dd.n == n)].sort_values("rate_per_node")
            a = ax[0][j]
            a.plot(f.rate_per_node * n, f.mean_ms, color=COLOR["fast"], marker="^", label="Fast 平均")
            a.plot(f.rate_per_node * n, f.p50_ms, color=COLOR["fast"], marker="^", ls=":", label="Fast P50")
            a.plot(m.rate_per_node * n, m.mean_ms, color=COLOR["multi"], marker="o", label="Multi 平均")
            a.plot(m.rate_per_node * n, m.p50_ms, color=COLOR["multi"], marker="o", ls=":", label="Multi P50")
            a.set_xscale("log")
            a.set_yscale("log")
            a.set_xlabel("总到达率 req/s")
            a.set_ylabel("延迟 ms")
            a.set_title("%d 节点（qc=%d, qf=%d）" % (n, n // 2 + 1, (2 * n - (n // 2 + 1)) // 2 + 1))
            a.legend(fontsize=7)
            a = ax[1][j]
            a.plot(f.conflict_rate * 100, f.mean_ms, color=COLOR["fast"], marker="^", label="Fast 平均延迟")
            a.plot(f.conflict_rate * 100, f.p99_ms, color=COLOR["fast"], marker="^", ls=":", alpha=0.6, label="Fast P99")
            if len(m):
                base = m.mean_ms.iloc[:4].mean()
                a.axhline(base, color=COLOR["multi"], ls="--", label="Multi 平均延迟 ≈ %.1fms" % base)
            a.set_xlabel("Fast Paxos 冲突率 (%)")
            a.set_ylabel("延迟 ms")
            a.set_yscale("log")
            a.legend(fontsize=7)
        fig.suptitle("Fast Paxos 冲突临界点（RTT=%gms，每个节点都是 Proposer，开环泊松到达）" % rtt, y=1.0)
        save(fig, "fastconflict_rtt%g" % rtt)


def _timeline(path, bucket=0.1):
    r = pd.read_csv(path)
    r = r[r.ok == 1]
    done = r.t_start_s + r.latency_ms / 1000.0
    edges = np.arange(0, done.max() + bucket, bucket)
    h, _ = np.histogram(done, edges)
    return edges[:-1], h / bucket, r


def fig_faults():
    d = load("faults")
    if d is None:
        return
    groups = [
        ("faults_multi3", "Multi-Paxos，3 节点", ["multi-n3-kill_follower", "multi-n3-kill_leader",
                                             "multi-n3-partition_leader", "multi-n3-partition_minority",
                                             "multi-n3-kill_many2"]),
        ("faults_quorum", "容错上限：4/5 节点", ["multi-n4-kill_follower", "multi-n4-kill_many2",
                                           "multi-n5-kill_many2", "multi-n5-kill_many3"]),
        ("faults_fast", "Fast / Basic Paxos", ["fast-n3-kill_follower", "fast-n4-kill_follower",
                                              "fast-n3-kill_leader", "fast-n4-kill_leader",
                                              "basic-n3-kill_follower", "basic-n3-kill_many2"]),
    ]
    zh = {"kill_follower": "杀 1 个 Follower", "kill_leader": "杀 Leader", "partition_leader": "隔离 Leader",
          "partition_minority": "隔离 1 个 Follower", "kill_many2": "杀 2 个节点", "kill_many3": "杀 3 个节点"}
    for fname, title, runs in groups:
        runs = [r for r in runs if os.path.exists(os.path.join(DATA, "raw", "faults", r + ".csv"))]
        if not runs:
            continue
        fig, ax = plt.subplots(len(runs), 1, figsize=(10, 1.9 * len(runs)), sharex=True, squeeze=False)
        for a, run in zip(ax[:, 0], runs):
            t, tp, _ = _timeline(os.path.join(DATA, "raw", "faults", run + ".csv"))
            mode, n, kind = run.split("-", 2)
            a.fill_between(t, tp, step="post", color=COLOR[mode], alpha=0.7)
            row = d[d.run == run].iloc[0]
            note = str(row.note)
            try:
                tf = float(note.split("t_fault=")[1].split()[0])
                th = float(note.split("t_heal=")[1].split()[0])
                a.axvline(tf, color="r", ls="--", lw=1)
                a.axvline(th, color="g", ls="--", lw=1)
            except Exception:
                pass
            a.set_ylabel("req/s")
            a.set_title("%s %s：%s（最长不可用窗口 %.0f ms）" % (NAME[mode], n, zh.get(kind, kind), row.recovery_ms),
                        fontsize=9, loc="left")
        ax[-1, 0].set_xlabel("时间 (s)   红线=注入故障，绿线=恢复（重启节点/解除分区）")
        fig.suptitle(title + "：每 100ms 的提交吞吐", y=1.0)
        save(fig, fname)


def fig_recovery():
    d = load("recovery")
    if d is None:
        return
    fig, ax = plt.subplots(figsize=(6.5, 4))
    for (mode, rtt), s in d.groupby(["mode", "rtt_ms"]):
        s = s.sort_values("election_ms")
        each = [np.array([float(x) for x in str(n).split("=")[1].split("/")]) for n in s.note]
        mean = np.array([e.mean() for e in each])
        lo = np.array([e.min() for e in each])
        hi = np.array([e.max() for e in each])
        ax.errorbar(s.election_ms, mean, yerr=[mean - lo, hi - mean], marker=MARK[mode], capsize=3,
                    color=COLOR[mode], ls="-" if rtt == 10 else "--", label="%s RTT=%gms" % (NAME[mode], rtt))
    x = np.array([150, 1200])
    ax.plot(x, x, color="gray", ls=":", lw=0.8, label="T（下界）")
    ax.plot(x, 2 * x, color="gray", ls="-.", lw=0.8, label="2T（上界）")
    ax.set_xlabel("选举超时基数 T (ms)，实际超时在 [T, 2T) 内随机")
    ax.set_ylabel("杀 Leader 后的不可用窗口 (ms)")
    ax.set_xscale("log")
    ax.set_yscale("log")
    ax.legend(fontsize=8)
    ax.set_title("Leader 故障恢复时间（每点 6 次，误差线为最小/最大）", fontsize=10)
    save(fig, "recovery")


if __name__ == "__main__":
    for f in (fig_baseline, fig_concurrency, fig_rtt, fig_loss, fig_fsync, fig_batch, fig_livelock,
              fig_fastconflict, fig_faults, fig_recovery):
        try:
            f()
        except Exception as e:  # noqa: BLE001
            import traceback
            traceback.print_exc()
            print("FAILED", f.__name__, e)
