"""把 report/实验报告.md（报告源稿）转成 report/实验报告.docx。

支持：# 标题、段落、**粗体**、`行内代码`、无序/有序列表、| 表格 |、```代码块```、![图题](路径)、
以及 <!--TABLE 名称--> 占位符（表格内容由本脚本直接从 data/*.csv 生成，避免手抄数字）。

  python report/build_report.py
"""
import os
import re
import sys

import pandas as pd
from docx import Document
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Cm, Pt, RGBColor

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
DATA = os.path.join(ROOT, "data")
NAME = {"basic": "Basic", "multi": "Multi", "fast": "Fast"}


def csv(name):
    return pd.read_csv(os.path.join(DATA, name + ".csv"))


def f(x, nd=1):
    if x is None or (isinstance(x, float) and x != x):
        return "—"
    if isinstance(x, (int,)) or float(x).is_integer() and nd == 0:
        return "%d" % x
    return ("%." + str(nd) + "f") % x


# ------------------------------------------------------------------ 数据表
def t_baseline():
    d = csv("baseline")
    rows = []
    for n in sorted(d.n.unique()):
        for m in ("basic", "multi", "fast"):
            r = d[(d.n == n) & (d["mode"] == m)].iloc[0]
            rows.append([n, NAME[m], f(r.mean_ms), f(r.p50_ms), f(r.p95_ms), f(r.p99_ms), f(r.msgs_per_op),
                         f(r.bytes_per_op, 0), f(r.conflict_rate * 100) + "%", f(r.rounds_per_op, 2), int(r.diverged)])
    return ["节点", "机制", "平均ms", "P50", "P95", "P99", "报文/命令", "字节/命令", "冲突率", "轮次/命令", "分歧实例数"], rows


def t_concurrency(rtt=10, n=3):
    d = csv("concurrency")
    d = d[(d.rtt_ms == rtt) & (d.n == n)]
    rows = []
    for cl in sorted(d.clients.unique()):
        row = [int(cl)]
        for m in ("basic", "multi", "fast"):
            r = d[(d.clients == cl) & (d["mode"] == m)]
            if r.empty or r.iloc[0].ok != r.iloc[0].ok:
                row += ["—", "—", "—", "—"]
                continue
            r = r.iloc[0]
            row += [f(r.throughput, 0), f(r.p50_ms), f(r.p99_ms), f(r.conflict_rate * 100, 0) + "%"]
        rows.append(row)
    hdr = ["并发"] + [NAME[m] + " " + x for m in ("basic", "multi", "fast") for x in ("吞吐", "P50", "P99", "冲突")]
    return hdr, rows


def t_rtt():
    d = csv("rtt")
    rows = []
    for n in sorted(d.n.unique()):
        for rtt in sorted(d.rtt_ms.unique()):
            row = [n, f(rtt, 0), f(d[(d.n == n) & (d.rtt_ms == rtt)].rtt_measured_ms.mean(), 2)]
            for m in ("basic", "multi", "fast"):
                r = d[(d.n == n) & (d.rtt_ms == rtt) & (d["mode"] == m)].iloc[0]
                row += [f(r.throughput, 0), f(r.p50_ms), f(r.p99_ms)]
            rows.append(row)
    hdr = ["节点", "RTT", "实测RTT"] + [NAME[m] + " " + x for m in ("basic", "multi", "fast") for x in ("吞吐", "P50", "P99")]
    return hdr, rows


def t_loss(mode):
    d = csv("loss")
    d = d[(d.n == 3) & (d["mode"] == mode)]
    losses = sorted(d.loss.unique())
    rows = []
    for rtt in sorted(d.rtt_ms.unique()):
        base = d[(d.rtt_ms == rtt) & (d.loss == 0)].throughput.iloc[0]
        row = ["%gms" % rtt]
        for ls in losses:
            r = d[(d.rtt_ms == rtt) & (d.loss == ls)]
            if r.empty or r.iloc[0].throughput != r.iloc[0].throughput:
                row.append("—")
            else:
                r = r.iloc[0]
                row.append("%s (%d%%) / %s" % (f(r.throughput, 0), round(r.throughput / base * 100), f(r.p99_ms, 0)))
        rows.append(row)
    return ["RTT ＼ 丢包"] + ["%g%%" % (x * 100) for x in losses], rows


def t_loss_n4():
    d = csv("loss")
    d = d[(d.rtt_ms == 10)]
    rows = []
    for ls in sorted(d[d.n == 4].loss.unique()):
        row = ["%g%%" % (ls * 100)]
        for m in ("basic", "multi", "fast"):
            for n in (3, 4):
                r = d[(d.n == n) & (d.loss == ls) & (d["mode"] == m)]
                row.append(f(r.iloc[0].throughput, 0) if len(r) else "—")
        rows.append(row)
    return ["丢包"] + ["%s n=%d" % (NAME[m], n) for m in ("basic", "multi", "fast") for n in (3, 4)], rows


def t_fsync():
    d = csv("fsync")
    d = d[d.note != "batching"]
    rows = []
    for rtt in sorted(d.rtt_ms.unique()):
        for m in ("basic", "multi", "fast"):
            a = d[(d.rtt_ms == rtt) & (d["mode"] == m) & (d.fsync == 0)].iloc[0]
            b = d[(d.rtt_ms == rtt) & (d["mode"] == m) & (d.fsync == 1)].iloc[0]
            rows.append([f(rtt, 0), NAME[m], f(a.throughput, 0), f(b.throughput, 0),
                         "%d%%" % round(b.throughput / a.throughput * 100), f(a.p50_ms, 2), f(b.p50_ms, 2),
                         f(a.p99_ms, 1), f(b.p99_ms, 1), f(b.fsync_avg_ms, 2),
                         f(b.fsync_count / max(1, b.ok + b.failed), 2)])
    return ["RTT", "机制", "吞吐(关)", "吞吐(开)", "开/关", "P50(关)", "P50(开)", "P99(关)", "P99(开)",
            "单次fsync ms", "fsync次数/命令"], rows


def t_fsync_batch():
    d = csv("fsync")
    d = d[d.note == "batching"]
    rows = []
    for b in sorted(d.batch.unique()):
        a = d[(d.batch == b) & (d.fsync == 0)].iloc[0]
        c = d[(d.batch == b) & (d.fsync == 1)].iloc[0]
        rows.append([int(b), f(a.throughput, 0), f(a.p50_ms, 1), f(a.msgs_per_op, 2), f(c.throughput, 0),
                     f(c.p50_ms, 1), f(c.msgs_per_op, 2), f(c.fsync_count / max(1, c.ok), 3)])
    return ["批大小", "吞吐(fsync关)", "P50", "报文/命令", "吞吐(fsync开)", "P50", "报文/命令", "fsync次数/命令"], rows


def t_batch():
    d = csv("batch").sort_values("batch")
    return ["批大小", "吞吐", "P50 ms", "P99 ms", "报文/命令"], \
        [[int(r.batch), f(r.throughput, 0), f(r.p50_ms), f(r.p99_ms), f(r.msgs_per_op, 2)] for r in d.itertuples()]


def t_livelock(n=3):
    d = csv("livelock")
    d = d[d.n == n].copy()
    d["p"] = d.note.str.extract(r"proposers=(\d+)").astype(int)
    d["c"] = d.note.str.extract(r"clients_per_proposer=(\d+)").astype(int)
    rows = []
    for bo in sorted(d.backoff_ms.unique()):
        for r in d[d.backoff_ms == bo].sort_values(["p", "c"]).itertuples():
            rows.append([f(bo, 0), "%d×%d" % (r.p, r.c), int(r.ok), int(r.failed), f(r.throughput),
                         f(r.p50_ms), f(r.p99_ms, 0), f(r.rounds_per_op, 1), f(r.conflict_rate * 100) + "%"])
    return ["退避基数 ms", "Proposer 数×并发数", "成功请求", "超时请求", "吞吐", "P50", "P99", "Prepare 轮次/命令", "冲突率"], rows


def t_fastconflict(rtt, n):
    d = csv("fastconflict")
    d = d[(d.rtt_ms == rtt) & (d.n == n)]
    rows = []
    for rate in sorted(d.rate_per_node.unique()):
        a = d[(d.rate_per_node == rate) & (d["mode"] == "fast")]
        b = d[(d.rate_per_node == rate) & (d["mode"] == "multi")]
        if a.empty or b.empty:
            continue
        a, b = a.iloc[0], b.iloc[0]
        rows.append([f(rate * n, 1), f(a.conflict_rate * 100) + "%", f(a.mean_ms), f(a.p50_ms), f(a.p99_ms, 0),
                     f(a.msgs_per_op), f(b.mean_ms), f(b.p50_ms), f(b.p99_ms, 0), f(b.msgs_per_op),
                     "Fast" if a.mean_ms < b.mean_ms else "Multi"])
    return ["总到达率/s", "Fast冲突率", "Fast平均", "Fast P50", "Fast P99", "Fast报文", "Multi平均", "Multi P50",
            "Multi P99", "Multi报文", "平均时延较低者"], rows


ZH = {"kill_follower": "终止 1 个非 Leader 节点", "kill_leader": "终止 Leader（协调者）", "partition_leader": "隔离 Leader",
      "partition_minority": "隔离 1 个 Follower", "kill_many2": "终止 2 个节点", "kill_many3": "终止 3 个节点"}


def t_faults():
    d = csv("faults")
    rows = []
    for r in d.itertuples():
        mode, n, kind = r.run.split("-", 2)
        note = str(r.note)
        g = lambda k: float(re.search(k + r"=([\d.]+)", note).group(1))  # noqa: E731
        tf, th = g("t_fault"), g("t_heal")
        raw = pd.read_csv(os.path.join(DATA, "raw", "faults", r.run + ".csv"))
        done = (raw.t_start_s + raw.latency_ms / 1000.0)[raw.ok == 1]
        # 按"完成时刻"统计：故障稳定后（注入 1.5s 之后）到恢复之前这段时间内完成的请求
        during = ((done >= tf + 1.5) & (done < th)).sum() / (th - tf - 1.5)
        before = ((done >= 0.5) & (done < tf)).sum() / (tf - 0.5)
        rows.append([NAME[mode], n[1:], ZH.get(kind, kind), f(before, 0), f(during, 0),
                     f(r.recovery_ms, 0), f(g("heal_gap_ms"), 0), int(raw.ok.eq(0).sum()), f(r.diverged, 0)])
    return ["机制", "节点", "故障", "故障前吞吐", "故障期间吞吐", "最长不可用窗口 ms", "恢复后最长停顿 ms",
            "超时请求数", "分歧实例数"], rows


def t_recovery():
    d = csv("recovery")
    rows = []
    for r in d.itertuples():
        each = str(r.note).split("=")[1]
        v = [float(x) for x in each.split("/")]
        rows.append([NAME[r.mode], f(r.rtt_ms, 0), f(r.election_ms, 0), each, f(min(v), 0), f(sum(v) / len(v), 0),
                     f(max(v), 0)])
    return ["机制", "RTT", "选举超时T", "6次不可用窗口 (ms)", "最小", "平均", "最大"], rows


def t_chaos():
    d = csv("chaos")
    rows = []
    for r in d.itertuples():
        rows.append([NAME[r.mode], r.n, str(r.note).split("=")[-1], int(r.ok), int(r.failed), int(r.slots_compared),
                     int(r.diverged), int(r.safety_violations), int(r.dup_commits)])
    return ["机制", "节点", "故障动作数", "成功请求", "超时请求", "比对实例数", "分歧实例", "节点自检违例", "重复提交(已去重)"], rows


def t_safety_all():
    rows = []
    tot = [0, 0, 0, 0]
    for name in ("baseline", "concurrency", "rtt", "loss", "fsync", "batch", "livelock", "fastconflict", "faults",
                 "recovery", "chaos"):
        p = os.path.join(DATA, name + ".csv")
        if not os.path.exists(p):
            continue
        d = pd.read_csv(p)
        ok = d[d.slots_compared.notna()]
        row = [name, len(d), len(ok), int(ok.slots_compared.sum()), int(ok.diverged.sum()),
               int(ok.safety_violations.sum())]
        rows.append(row)
        tot = [tot[0] + row[1], tot[1] + row[2], tot[2] + row[3], tot[3] + row[4] + row[5]]
    rows.append(["合计", tot[0], tot[1], tot[2], tot[3], ""])
    return ["实验组", "运行次数", "完成检验次数", "比对实例总数", "分歧实例数", "节点自检违例数"], rows


TABLES = {
    "baseline": t_baseline,
    "conc_n3_rtt10": lambda: t_concurrency(10, 3), "conc_n4_rtt10": lambda: t_concurrency(10, 4),
    "conc_n3_rtt0": lambda: t_concurrency(0, 3),
    "rtt": t_rtt,
    "loss_basic": lambda: t_loss("basic"), "loss_multi": lambda: t_loss("multi"), "loss_fast": lambda: t_loss("fast"),
    "loss_n4": t_loss_n4,
    "fsync": t_fsync, "fsync_batch": t_fsync_batch, "batch": t_batch,
    "livelock3": lambda: t_livelock(3), "livelock5": lambda: t_livelock(5),
    "fc_10_3": lambda: t_fastconflict(10, 3), "fc_10_4": lambda: t_fastconflict(10, 4),
    "fc_10_5": lambda: t_fastconflict(10, 5), "fc_50_3": lambda: t_fastconflict(50, 3),
    "fc_50_4": lambda: t_fastconflict(50, 4),
    "faults": t_faults, "recovery": t_recovery, "chaos": t_chaos, "safety_all": t_safety_all,
}


# ------------------------------------------------------------------ docx 输出
def set_font(run, size=None, bold=None, mono=False, color=None):
    name = "Consolas" if mono else "Times New Roman"
    east = "等线" if mono else "宋体"
    run.font.name = name
    rpr = run._element.get_or_add_rPr()
    rf = rpr.find(qn("w:rFonts"))
    if rf is None:
        rf = OxmlElement("w:rFonts")
        rpr.append(rf)
    rf.set(qn("w:eastAsia"), "等线" if mono else east)
    if size:
        run.font.size = Pt(size)
    if bold is not None:
        run.font.bold = bold
    if color:
        run.font.color.rgb = RGBColor(*color)


def shade(el, fill):
    pr = el.get_or_add_tcPr() if el.tag.endswith("tc") else el.get_or_add_pPr()
    shd = OxmlElement("w:shd")
    shd.set(qn("w:val"), "clear")
    shd.set(qn("w:color"), "auto")
    shd.set(qn("w:fill"), fill)
    pr.append(shd)


INLINE = re.compile(r"(\*\*.+?\*\*|`[^`]+`)")


def add_inline(p, text, size=10.5):
    for part in INLINE.split(text):
        if not part:
            continue
        if part.startswith("**"):
            set_font(p.add_run(part[2:-2]), size, bold=True)
        elif part.startswith("`"):
            set_font(p.add_run(part[1:-1]), size - 1, mono=True, color=(0x8B, 0x1A, 0x1A))
        else:
            set_font(p.add_run(part), size)


def add_table(doc, header, rows, size=8):
    t = doc.add_table(rows=1, cols=len(header))
    t.style = "Table Grid"
    t.alignment = WD_TABLE_ALIGNMENT.CENTER
    for i, h in enumerate(header):
        c = t.rows[0].cells[i]
        c.text = ""
        p = c.paragraphs[0]
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        add_inline(p, str(h), size)
        for r in p.runs:
            r.font.bold = True
        shade(c._element, "D9E2F3")
    for row in rows:
        cells = t.add_row().cells
        for i, v in enumerate(row):
            cells[i].text = ""
            p = cells[i].paragraphs[0]
            p.alignment = WD_ALIGN_PARAGRAPH.CENTER if len(str(v)) < 28 else WD_ALIGN_PARAGRAPH.LEFT
            add_inline(p, str(v), size)
    for row in t.rows:
        for c in row.cells:
            for p in c.paragraphs:
                p.paragraph_format.space_after = Pt(0)
                p.paragraph_format.space_before = Pt(0)
    doc.add_paragraph().paragraph_format.space_after = Pt(2)


def build(md_path, out_path):
    doc = Document()
    sec = doc.sections[0]
    sec.page_width, sec.page_height = Cm(21), Cm(29.7)
    sec.left_margin = sec.right_margin = Cm(2.2)
    sec.top_margin = sec.bottom_margin = Cm(2.2)
    st = doc.styles["Normal"]
    st.font.name = "Times New Roman"
    st.font.size = Pt(10.5)
    st.element.rPr.rFonts.set(qn("w:eastAsia"), "宋体")
    lines = open(md_path, encoding="utf-8").read().split("\n")
    i = 0
    missing = []
    while i < len(lines):
        ln = lines[i]
        s = ln.strip()
        if not s:
            i += 1
            continue
        if s.startswith("```"):
            code = []
            i += 1
            while i < len(lines) and not lines[i].strip().startswith("```"):
                code.append(lines[i])
                i += 1
            i += 1
            p = doc.add_paragraph()
            p.paragraph_format.left_indent = Cm(0.3)
            p.paragraph_format.space_after = Pt(6)
            p.paragraph_format.line_spacing = 1.0
            shade(p._element, "F2F2F2")
            set_font(p.add_run("\n".join(code)), 8, mono=True)
            continue
        if re.match(r"表 \d+-\d+ ", s) and not s.endswith("。"):
            cap = doc.add_paragraph()
            cap.alignment = WD_ALIGN_PARAGRAPH.CENTER
            cap.paragraph_format.space_after = Pt(2)
            cap.paragraph_format.keep_with_next = True
            add_inline(cap, s, 9.5)
            for r_ in cap.runs:
                r_.font.bold = True
            i += 1
            continue
        m = re.match(r"<!--TABLE (\w+)-->", s)
        if m:
            try:
                hdr, rows = TABLES[m.group(1)]()
                add_table(doc, hdr, rows)
            except Exception as e:  # noqa: BLE001
                missing.append("%s: %r" % (m.group(1), e))
                add_inline(doc.add_paragraph(), "[表 %s 缺数据]" % m.group(1))
            i += 1
            continue
        if s == "<!--PAGEBREAK-->":
            doc.add_page_break()
            i += 1
            continue
        m = re.match(r"!\[(.*?)\]\((.+?)\)", s)
        if m:
            path = os.path.join(HERE, m.group(2))
            if os.path.exists(path):
                doc.add_picture(path, width=Cm(16.4))
                doc.paragraphs[-1].alignment = WD_ALIGN_PARAGRAPH.CENTER
            else:
                missing.append("image " + m.group(2))
            cap = doc.add_paragraph()
            cap.alignment = WD_ALIGN_PARAGRAPH.CENTER
            add_inline(cap, m.group(1), 9)
            i += 1
            continue
        m = re.match(r"(#{1,4}) (.+)", s)
        if m:
            lvl = len(m.group(1))
            if lvl == 1 and m.group(2).startswith("TITLE:"):
                p = doc.add_paragraph()
                p.alignment = WD_ALIGN_PARAGRAPH.CENTER
                p.paragraph_format.space_before = Pt(18)
                p.paragraph_format.space_after = Pt(12)
                set_font(p.add_run(m.group(2)[6:].strip()), 20, bold=True)
            else:
                h = doc.add_heading(level=lvl)
                r = h.add_run(m.group(2).replace("`", ""))
                set_font(r, {1: 16, 2: 13.5, 3: 11.5, 4: 10.5}[lvl], bold=True, color=(0, 0, 0))
                r._element.get_or_add_rPr().find(qn("w:rFonts")).set(qn("w:eastAsia"), "黑体")
            i += 1
            continue
        if s.startswith("|"):
            tbl = []
            while i < len(lines) and lines[i].strip().startswith("|"):
                tbl.append([c.strip() for c in lines[i].strip().strip("|").split("|")])
                i += 1
            rows = [r for r in tbl[1:] if not all(re.fullmatch(r":?-+:?", c) for c in r)]
            add_table(doc, tbl[0], rows, size=9)
            continue
        m = re.match(r"(\s*)([-*]|\d+\.) (.+)", ln)
        if m:
            if m.group(2) in "-*":
                p = doc.add_paragraph(style="List Bullet")
                if len(m.group(1)) >= 2:
                    p.paragraph_format.left_indent = Cm(1.4)
                add_inline(p, m.group(3))
            else:
                # 有序列表用源文件里的编号：Word 的自动编号会跨列表延续
                p = doc.add_paragraph()
                p.paragraph_format.left_indent = Cm(0.74)
                p.paragraph_format.first_line_indent = Cm(-0.6)
                add_inline(p, m.group(2) + " " + m.group(3))
            p.paragraph_format.space_after = Pt(2)
            i += 1
            continue
        if s.startswith(">"):
            p = doc.add_paragraph()
            p.paragraph_format.left_indent = Cm(0.6)
            shade(p._element, "FFF8E1")
            add_inline(p, s.lstrip("> "), 10)
            i += 1
            continue
        para = [s]
        i += 1
        while i < len(lines) and lines[i].strip() and not re.match(r"(#{1,4} |\||```|!\[|<!--|\s*([-*]|\d+\.) |>)", lines[i]):
            para.append(lines[i].strip())
            i += 1
        p = doc.add_paragraph()
        p.paragraph_format.first_line_indent = Cm(0.74)
        p.paragraph_format.space_after = Pt(5)
        p.paragraph_format.line_spacing = 1.25
        add_inline(p, "".join(para))
    doc.save(out_path)
    print("saved", out_path)
    for m in missing:
        print("MISSING", m)


if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "tables":
        for k, fn in TABLES.items():
            try:
                hdr, rows = fn()
                print("\n##", k)
                print(" | ".join(map(str, hdr)))
                for r in rows:
                    print(" | ".join(map(str, r)))
            except Exception as e:  # noqa: BLE001
                print("\n##", k, "ERR", repr(e))
    else:
        build(os.path.join(HERE, "实验报告.md"), os.path.join(HERE, "实验报告.docx"))
