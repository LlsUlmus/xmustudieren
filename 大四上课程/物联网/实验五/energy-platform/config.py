# -*- coding: utf-8 -*-
"""
绿源零碳产业园 能源互联网监测平台 —— 公共配置
模拟数据端（simulator.py）与服务器端（server.py）共用本文件。
"""

SITE_NAME = "绿源零碳产业园"

# ---------------- 监测对象 ----------------
# kind: load=用电负荷  pv=光伏发电  ess=储能电站
# rated: 额定功率 kW；threshold: 功率超限告警阈值 kW（可在看板上在线修改）
DEVICES = [
    {"id": "ws_a",   "name": "注塑车间A",      "kind": "load", "group": "生产制造", "rated": 900, "threshold": 820},
    {"id": "ws_b",   "name": "装配车间B",      "kind": "load", "group": "生产制造", "rated": 560, "threshold": 500},
    {"id": "air",    "name": "空压站及公辅",   "kind": "load", "group": "公辅动力", "rated": 300, "threshold": 270},
    {"id": "dc_it",  "name": "数据中心-IT机柜", "kind": "load", "group": "数据中心", "rated": 380, "threshold": 360},
    {"id": "dc_cool","name": "数据中心-制冷",   "kind": "load", "group": "数据中心", "rated": 220, "threshold": 200},
    {"id": "office", "name": "研发办公楼",      "kind": "load", "group": "办公生活", "rated": 300, "threshold": 270},
    {"id": "ev",     "name": "充电桩群",        "kind": "load", "group": "办公生活", "rated": 240, "threshold": 230},
    {"id": "pv",     "name": "屋顶光伏",        "kind": "pv",   "group": "新能源",   "rated": 1200, "threshold": None},
    {"id": "ess",    "name": "储能电站",        "kind": "ess",  "group": "新能源",   "rated": 500, "threshold": None,
     "capacity": 1000},
]
DEVICE_MAP = {d["id"]: d for d in DEVICES}
LOAD_IDS = [d["id"] for d in DEVICES if d["kind"] == "load"]

# ---------------- 分时电价（元/kWh，一般工商业两部制简化） ----------------
TARIFF = {
    "sharp":  {"name": "尖峰", "price": 1.32, "color": "#ff4d6d"},
    "peak":   {"name": "高峰", "price": 1.05, "color": "#ffa62b"},
    "flat":   {"name": "平段", "price": 0.66, "color": "#4cc9f0"},
    "valley": {"name": "低谷", "price": 0.31, "color": "#52b788"},
}
# 一天 24 小时对应的电价时段
HOUR_PERIOD = (
    ["valley"] * 8 +                    # 00-08
    ["peak"] * 2 + ["sharp"] +          # 08-10 峰, 10-11 尖
    ["flat"] * 2 +                      # 11-13 平
    ["peak"] + ["sharp"] + ["peak"] * 2 +  # 13-14 峰, 14-15 尖, 15-17 峰
    ["flat"] * 2 +                      # 17-19 平
    ["peak"] * 2 +                      # 19-21 峰
    ["flat"] * 3                        # 21-24 平
)
assert len(HOUR_PERIOD) == 24

# 电网排放因子 kgCO2/kWh（生态环境部 2022 年度全国电网平均排放因子 0.5703 tCO2/MWh）
GRID_EF = 0.5703

# ---------------- 告警规则 ----------------
PUE_LIMIT = 1.60          # 数据中心 PUE 超限
SOC_LOW = 0.10            # 储能 SOC 过低
PV_RATIO_LOW = 0.35       # 光伏实际/理论出力比低于该值视为异常（白天）
JUMP_RATIO = 0.60         # 单周期功率突变比例
OFFLINE_SECONDS = 4.0     # 超过 N 秒（真实时间）未收到数据判定为数据中断

# ---------------- 通信 ----------------
SERVER_HOST = "127.0.0.1"
SERVER_PORT = 5000
