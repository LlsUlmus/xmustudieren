# -*- coding: utf-8 -*-
"""生成系统架构图与截图压缩（用于实验报告）。用法：python tools/arch_diagram.py 截图目录 输出目录"""
import os, sys
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib.patches import FancyBboxPatch
from PIL import Image

src, out = sys.argv[1], sys.argv[2]
os.makedirs(out, exist_ok=True)
plt.rcParams["font.sans-serif"] = ["Microsoft YaHei", "SimHei"]
plt.rcParams["axes.unicode_minus"] = False

fig, ax = plt.subplots(figsize=(13, 6.2), dpi=150)
ax.set_xlim(0, 13); ax.set_ylim(0, 6.2); ax.axis("off")

def box(x, y, w, h, title, lines, fc, ec):
    ax.add_patch(FancyBboxPatch((x, y), w, h, boxstyle="round,pad=0.02,rounding_size=0.12", fc=fc, ec=ec, lw=1.8))
    ax.text(x + w / 2, y + h - 0.32, title, ha="center", va="center", fontsize=13, weight="bold", color="#0b2545")
    for i, l in enumerate(lines):
        ax.text(x + 0.18, y + h - 0.78 - i * 0.36, "• " + l, ha="left", va="center", fontsize=9.5, color="#1b263b")

box(0.2, 0.5, 3.6, 5.2, "模拟数据端 simulator.py",
    ["注塑车间A / 装配车间B", "空压站及公辅", "数据中心 IT 机柜 / 制冷", "研发办公楼 / 充电桩群",
     "屋顶光伏 1.2MWp（辐照模型）", "储能电站 1MWh（峰谷策略）", "加速仿真时钟 1s = 5min",
     "故障注入：超限/突变/中断/", "  制冷故障/光伏故障", "历史数据补录（昨日 0 点起）"], "#e0f2fe", "#0284c7")
box(4.7, 0.5, 3.6, 5.2, "服务器端 server.py / engine.py",
    ["Flask REST 数据接入", "电表差分计算周期电量", "功率平衡：电网=负荷-光伏-储能", "SQLite 持久化 readings/alarms",
     "分时统计 / 占比排名 / 峰谷平", "电费估算 / 碳排放 / PUE", "告警规则引擎 + 数据中断看门狗",
     "SSE 实时推送看板快照", "故障指令队列 / 阈值配置", "CSV 导出"], "#dcfce7", "#16a34a")
box(9.2, 0.5, 3.6, 5.2, "可视化网页端 static/",
    ["ECharts 5 数字看板", "KPI 概览 / 三联仪表盘", "实时功率曲线（电价时段底色）", "监测对象卡片 / 能流桑基图",
     "占比饼图 / 能耗排名", "统计分析页（7 类分析）", "告警中心（统计/规则/记录）", "设备详情弹窗 / 阈值修改",
     "模拟控制台（异常注入）", "EventSource 自动重连"], "#fef3c7", "#d97706")

def arrow(x1, y1, x2, y2, text, c):
    ax.annotate("", xy=(x2, y2), xytext=(x1, y1), arrowprops=dict(arrowstyle="-|>", lw=2, color=c, mutation_scale=18))
    ax.text((x1 + x2) / 2, (y1 + y2) / 2 + (0.17 if y1 > 2 else -0.3), text, ha="center", fontsize=9, color=c)

arrow(3.85, 4.2, 4.65, 4.2, "POST /api/ingest", "#0369a1")
arrow(4.65, 1.0, 3.85, 1.0, "GET /api/sim/commands", "#b45309")
arrow(8.35, 4.2, 9.15, 4.2, "SSE /api/stream", "#15803d")
arrow(9.15, 1.0, 8.35, 1.0, "POST fault/threshold/ack", "#b45309")
ax.text(4.25, 3.6, "HTTP + JSON\n每 1 秒", ha="center", fontsize=8.5, color="#475569")
ax.text(8.75, 3.6, "实时推送\n快照", ha="center", fontsize=8.5, color="#475569")
ax.text(6.5, 6.0, "绿源零碳产业园 能源互联网监测平台 — 系统架构", ha="center", fontsize=15, weight="bold", color="#0b2545")
plt.savefig(os.path.join(out, "arch.png"), bbox_inches="tight", facecolor="white")

for sub in ("normal", "alarm"):
    for f in os.listdir(os.path.join(src, sub)):
        im = Image.open(os.path.join(src, sub, f)).convert("RGB")
        if f == "detail.png" or f == "console.png":
            im = im.crop((0, 0, 1920, 1080))
        im.save(os.path.join(out, f"{sub}_{f[:-4]}.jpg"), quality=85)
print(sorted(os.listdir(out)))
