# -*- coding: utf-8 -*-
"""
模拟数据端：模拟园区各监测对象的智能电表 / 逆变器 / BMS 数据，
通过 HTTP REST 接口（JSON）周期性上报到服务器。

时间采用“加速仿真时钟”：默认每 1 秒（真实时间）上报一次，每次推进 5 分钟（仿真时间），
即 1 个仿真日 ≈ 4.8 分钟，便于在短时间内观察一天的负荷变化规律。

用法：
    python simulator.py                         # 默认参数
    python simulator.py --interval 1 --step 5 --start 07:00 --backfill
    python simulator.py --random-fault 0.01     # 随机注入异常的概率（每周期）
"""
import argparse
import math
import random
import time
from datetime import datetime, timedelta

import requests

from config import DEVICES, DEVICE_MAP, HOUR_PERIOD, SERVER_HOST, SERVER_PORT


def smooth(h, a, b, ramp=0.5):
    """h 在 [a, b) 区间内为 1，两侧 ramp 小时内线性过渡，用于构造平滑的作息曲线。"""
    if h < a - ramp or h >= b + ramp:
        return 0.0
    if h < a:
        return (h - (a - ramp)) / ramp
    if h >= b:
        return 1 - (h - b) / ramp
    return 1.0


class ParkSimulator:
    def __init__(self, seed=None):
        self.rng = random.Random(seed)
        self.meter = {d["id"]: 0.0 for d in DEVICES}       # 累计电量（电表读数）kWh
        self.meter["ess_charge"] = 0.0
        self.soc = 0.45                                     # 储能 SOC
        self.cloud = 0.95                                   # 云量因子（随机游走）
        self.faults = {}                                    # device -> {"type", "left"}
        self.prev_power = {}

    # ---------------- 各对象功率模型 ----------------
    def noise(self, base, pct=0.04):
        return base * (1 + self.rng.gauss(0, pct))

    def model_loads(self, t: datetime):
        h = t.hour + t.minute / 60
        weekend = t.weekday() >= 5
        k = 0.55 if weekend else 1.0
        p = {}
        # 注塑车间A：两班制，白班满负荷、午间小幅下降、夜班轻载
        day = smooth(h, 8, 20, 0.5)
        night = smooth(h, 20, 23.5, 0.3)
        lunch = 0.82 if 12 <= h < 13 else 1.0
        p["ws_a"] = self.noise(130 + k * (560 * day * lunch + 260 * night), 0.05)
        # 装配车间B：单班 8-22 点
        p["ws_b"] = self.noise(45 + k * 360 * smooth(h, 8, 22, 0.5) * (0.75 if 12 <= h < 13 else 1.0), 0.05)
        # 空压站及公辅：随生产车间联动
        p["air"] = self.noise(30 + 0.24 * (p["ws_a"] + p["ws_b"]), 0.03)
        # 数据中心 IT：全天平稳，白天业务略高
        p["dc_it"] = self.noise(285 + 25 * smooth(h, 9, 21, 1.5), 0.015)
        # 数据中心制冷：IT × (PUE-1)，PUE 随室外温度（午后最高）变化
        temp = 0.5 + 0.5 * math.cos((h - 14.5) / 24 * 2 * math.pi)
        pue = 1.27 + 0.13 * temp + self.rng.gauss(0, 0.008)
        p["dc_cool"] = p["dc_it"] * (pue - 1)
        # 研发办公楼
        p["office"] = self.noise(42 + 175 * smooth(h, 8.5, 18, 0.5) * (0.85 if 12 <= h < 13 else 1.0)
                                 + 70 * smooth(h, 18.75, 21, 0.5), 0.05)
        # 充电桩群：早到岗、午休、下班后三个高峰
        p["ev"] = self.noise(18 + 150 * smooth(h, 8, 9.5, 0.3) + 95 * smooth(h, 12, 13.5, 0.3)
                             + 175 * smooth(h, 18, 22, 0.5), 0.10)
        return {k_: max(0.0, v) for k_, v in p.items()}

    def model_pv(self, t: datetime):
        h = t.hour + t.minute / 60
        self.cloud = min(1.0, max(0.55, self.cloud + self.rng.gauss(0, 0.03)))
        g = math.sin(math.pi * (h - 6.2) / 12.0) if 6.2 <= h <= 18.2 else 0.0
        irradiance = 1000 * max(0.0, g) ** 1.15 * self.cloud
        expected = DEVICE_MAP["pv"]["rated"] * 0.85 * irradiance / 1000
        return irradiance, expected

    def model_ess(self, t, net_load, step_h):
        """储能策略：谷时充电、峰/尖时放电、光伏富余时消纳。正值=放电，负值=充电。"""
        cap = DEVICE_MAP["ess"]["capacity"]
        rated = DEVICE_MAP["ess"]["rated"]
        period = HOUR_PERIOD[t.hour]
        if net_load < 0:                                 # 光伏富余 -> 充电消纳
            p = max(-rated, net_load)
        elif period == "valley" and t.hour < 7:
            p = -300.0
        elif period == "flat" and 11 <= t.hour < 13:     # 午间平段补充充电，供下午尖峰放电
            p = -300.0
        elif period == "sharp":
            p = 450.0
        elif period == "peak":
            p = 260.0
        else:
            p = 0.0
        if p < 0 and self.soc >= 0.95:
            p = 0.0
        if p > 0 and self.soc <= 0.12:
            p = 0.0
        p = min(p, max(net_load, 0)) if p > 0 else p     # 不向电网反送
        eff = 0.95
        if p > 0:                                        # 放电不低于 SOC 下限 12%
            p = min(p, max(0.0, (self.soc - 0.12) * cap * eff / step_h))
        elif p < 0:                                      # 充电不超过 SOC 上限 95%
            p = max(p, -max(0.0, (0.95 - self.soc) * cap / eff / step_h))
        if p < 0:
            self.soc = min(1.0, self.soc + (-p) * step_h * eff / cap)
        else:
            self.soc = max(0.0, self.soc - p * step_h / eff / cap)
        return p

    # ---------------- 异常注入 ----------------
    def apply_fault(self, did, ftype, ticks):
        self.faults[did] = {"type": ftype, "left": int(ticks)}
        print(f"[故障注入] {DEVICE_MAP[did]['name']} -> {ftype}，持续 {ticks} 个周期")

    def random_fault(self, prob):
        if self.rng.random() < prob:
            did = self.rng.choice(["ws_a", "ws_b", "air", "office", "ev", "dc_cool", "pv"])
            ftype = {"dc_cool": "cooling", "pv": "pv_fault"}.get(did, self.rng.choice(["overload", "overload", "offline", "surge"]))
            self.apply_fault(did, ftype, self.rng.randint(3, 8))

    # ---------------- 生成一个周期的数据 ----------------
    def step(self, t: datetime, step_min: float):
        step_h = step_min / 60
        loads = self.model_loads(t)
        irr, pv_expected = self.model_pv(t)
        pv = pv_expected * self.rng.uniform(0.96, 1.0)

        offline = set()
        for did, f in list(self.faults.items()):
            rated = DEVICE_MAP[did]["rated"]
            if f["type"] == "overload" and did in loads:
                loads[did] = rated * self.rng.uniform(1.02, 1.15)
            elif f["type"] == "surge" and did in loads:
                loads[did] = min(rated * 1.2, loads[did] * 1.9 + 0.3 * rated)
            elif f["type"] == "cooling":
                loads["dc_cool"] = loads["dc_it"] * self.rng.uniform(0.72, 0.85)   # 制冷故障，PUE≈1.8
            elif f["type"] == "pv_fault":
                pv = pv_expected * 0.12
            elif f["type"] == "offline":
                offline.add(did)
            f["left"] -= 1
            if f["left"] <= 0:
                del self.faults[did]
                print(f"[故障恢复] {DEVICE_MAP[did]['name']}")

        net = sum(loads.values()) - pv
        ess = self.model_ess(t, net, step_h)

        readings = []
        for did, pw in loads.items():
            self.meter[did] += pw * step_h
            readings.append({"id": did, "power": round(pw, 2), "energy": round(self.meter[did], 3),
                             "status": "run" if pw > 0.08 * DEVICE_MAP[did]["rated"] else "idle"})
        self.meter["pv"] += pv * step_h
        readings.append({"id": "pv", "power": round(pv, 2), "energy": round(self.meter["pv"], 3),
                         "status": "run" if pv > 1 else "idle",
                         "extra": {"irradiance": round(irr, 1), "expected": round(pv_expected, 2)}})
        if ess > 0:
            self.meter["ess"] += ess * step_h
        else:
            self.meter["ess_charge"] += -ess * step_h
        readings.append({"id": "ess", "power": round(ess, 2), "energy": round(self.meter["ess"], 3),
                         "status": "discharge" if ess > 1 else ("charge" if ess < -1 else "idle"),
                         "extra": {"soc": round(self.soc, 4), "charge_energy": round(self.meter["ess_charge"], 3)}})
        readings = [r for r in readings if r["id"] not in offline]
        return {"ts": t.strftime("%Y-%m-%d %H:%M"), "step_min": step_min, "readings": readings}


def main():
    ap = argparse.ArgumentParser(description="园区能源数据模拟器")
    ap.add_argument("--server", default=f"http://{SERVER_HOST}:{SERVER_PORT}")
    ap.add_argument("--interval", type=float, default=1.0, help="上报周期（真实秒）")
    ap.add_argument("--step", type=float, default=5.0, help="每周期推进的仿真分钟数")
    ap.add_argument("--start", default="07:00", help="仿真起始时刻 HH:MM（日期为今天）")
    ap.add_argument("--backfill", action="store_true", default=True, help="启动时补齐昨日 0 点以来的历史数据")
    ap.add_argument("--no-backfill", dest="backfill", action="store_false")
    ap.add_argument("--random-fault", type=float, default=0.0, help="每周期随机注入异常的概率")
    ap.add_argument("--seed", type=int, default=2026)
    args = ap.parse_args()

    sim = ParkSimulator(args.seed)
    hh, mm = map(int, args.start.split(":"))
    now = datetime.now().replace(hour=hh, minute=mm, second=0, microsecond=0)
    t = now.replace(hour=0, minute=0) - timedelta(days=1) if args.backfill else now
    step = timedelta(minutes=args.step)
    s = requests.Session()

    # 等待服务器就绪
    while True:
        try:
            s.get(args.server + "/api/ping", timeout=2)
            break
        except requests.RequestException:
            print("等待服务器启动 ...")
            time.sleep(1)

    if args.backfill:
        batch = []
        while t < now:
            batch.append(sim.step(t, args.step))
            t += step
            if len(batch) >= 100:
                s.post(args.server + "/api/ingest/batch", json={"items": batch}, timeout=30)
                batch = []
        if batch:
            s.post(args.server + "/api/ingest/batch", json={"items": batch}, timeout=30)
        print(f"历史数据补齐完成，仿真时钟从 {now:%Y-%m-%d %H:%M} 开始实时上报")

    while True:
        t0 = time.time()
        try:
            # 拉取看板下发的故障注入指令
            cmds = s.get(args.server + "/api/sim/commands", timeout=2).json().get("commands", [])
            for c in cmds:
                if c.get("type") == "clear":
                    sim.faults.clear()
                    print("[故障清除] 全部恢复")
                elif c.get("device") in DEVICE_MAP:
                    sim.apply_fault(c["device"], c["type"], c.get("ticks", 6))
            sim.random_fault(args.random_fault)
            payload = sim.step(t, args.step)
            s.post(args.server + "/api/ingest", json=payload, timeout=3)
            total = sum(r["power"] for r in payload["readings"] if DEVICE_MAP[r["id"]]["kind"] == "load")
            print(f"{payload['ts']}  上报 {len(payload['readings'])} 个对象，园区总负荷 {total:7.1f} kW")
            t += step
        except requests.RequestException as e:
            print("上报失败：", e)
        time.sleep(max(0.0, args.interval - (time.time() - t0)))


if __name__ == "__main__":
    main()
