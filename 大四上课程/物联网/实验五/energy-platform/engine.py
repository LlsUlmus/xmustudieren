# -*- coding: utf-8 -*-
"""
服务器端核心：数据接入、存储、统计分析与异常告警。
"""
import json
import os
import sqlite3
import threading
import time
from collections import defaultdict, deque
from datetime import datetime, timedelta

import config as C

GROUPS = []
for _d in C.DEVICES:
    if _d["kind"] == "load" and _d["group"] not in GROUPS:
        GROUPS.append(_d["group"])

LEVEL_ORDER = {"严重": 0, "警告": 1, "提示": 2}


def r2(x, n=2):
    return None if x is None else round(x, n)


class Engine:
    def __init__(self, db_path):
        self.lock = threading.RLock()
        self.cond = threading.Condition(self.lock)
        self.version = 0
        self.thresholds = {d["id"]: d["threshold"] for d in C.DEVICES}
        self.latest = {}                 # did -> 最近一次读数
        self.last_seen = {}              # did -> 真实时间戳（用于数据中断判定）
        self.prev_meter = {}             # did -> 上一次电表累计值
        self.ticks = deque(maxlen=12 * 24 * 3)
        # hourly[(date, hour)][key] -> kWh / 元
        self.hourly = defaultdict(lambda: defaultdict(float))
        self.alarms = []                 # 告警记录（新的在前）
        self.active = {}                 # (did, type) -> alarm
        self.alarm_seq = 0
        self.commands = []               # 待下发给模拟器的故障注入指令
        self.sim_time = None
        self.step_h = 5 / 60
        self._snap_cache = (-1, None)
        os.makedirs(os.path.dirname(db_path), exist_ok=True)
        self.db = sqlite3.connect(db_path, check_same_thread=False)
        self.db.executescript("""
            CREATE TABLE IF NOT EXISTS readings(ts TEXT, device TEXT, power REAL, energy REAL, status TEXT, extra TEXT);
            CREATE TABLE IF NOT EXISTS alarms(id INTEGER, device TEXT, type TEXT, level TEXT, msg TEXT,
                                              start_ts TEXT, end_ts TEXT);
            DELETE FROM readings; DELETE FROM alarms;
        """)
        threading.Thread(target=self._watchdog, daemon=True).start()

    # ======================= 数据接入 =======================
    def ingest(self, payload, backfill=False):
        with self.lock:
            ts = datetime.strptime(payload["ts"], "%Y-%m-%d %H:%M")
            step_h = float(payload.get("step_min", 5)) / 60
            self.step_h = step_h
            self.sim_time = ts
            date, hour = ts.strftime("%Y-%m-%d"), ts.hour
            price = C.TARIFF[C.HOUR_PERIOD[hour]]["price"]
            H = self.hourly[(date, hour)]
            now = time.time()
            rows = []
            prev_power = {k: v["power"] for k, v in self.latest.items()}
            received = set()

            for r in payload["readings"]:
                did = r["id"]
                if did not in C.DEVICE_MAP:
                    continue
                received.add(did)
                prev = self.prev_meter.get(did)
                delta = r["energy"] - prev if prev is not None and r["energy"] >= prev else r["power"] * step_h
                self.prev_meter[did] = r["energy"]
                kind = C.DEVICE_MAP[did]["kind"]
                if kind == "load":
                    H[did] += delta
                    H["load"] += delta
                    H["cost_" + did] += delta * price
                elif kind == "pv":
                    H["pv"] += delta
                elif kind == "ess":
                    H["ess_dis"] += delta
                    ce = r.get("extra", {}).get("charge_energy")
                    if ce is not None:
                        pc = self.prev_meter.get("ess_charge")
                        H["ess_chg"] += (ce - pc) if pc is not None and ce >= pc else 0
                        self.prev_meter["ess_charge"] = ce
                self.latest[did] = {"power": r["power"], "status": r.get("status", "run"),
                                    "extra": r.get("extra", {}), "ts": payload["ts"]}
                if not backfill:
                    self.last_seen[did] = now
                rows.append((payload["ts"], did, r["power"], r["energy"], r.get("status"),
                             json.dumps(r.get("extra", {}))))

            # 园区功率平衡：电网 = 负荷 - 光伏 - 储能放电
            p = {did: self.latest.get(did, {}).get("power", 0.0) for did in C.DEVICE_MAP}
            load = sum(p[d] for d in C.LOAD_IDS)
            grid = load - p["pv"] - p["ess"]
            imp, exp_ = max(grid, 0.0), max(-grid, 0.0)
            H["grid"] += imp * step_h
            H["export"] += exp_ * step_h
            H["cost"] += imp * step_h * price
            H["carbon"] += imp * step_h * C.GRID_EF
            it = p["dc_it"] or 1e-6
            pue = (p["dc_it"] + p["dc_cool"] + 0.04 * p["dc_it"]) / it
            soc = self.latest.get("ess", {}).get("extra", {}).get("soc")
            self.ticks.append({"ts": payload["ts"], "load": load, "pv": p["pv"], "ess": p["ess"],
                               "grid": grid, "pue": pue, "soc": soc, "p": p,
                               "online": sorted(received)})
            self.db.executemany("INSERT INTO readings VALUES(?,?,?,?,?,?)", rows)

            if not backfill:
                self._evaluate(received, prev_power, pue, payload["ts"])
                self.db.commit()
                self._bump()

    def ingest_batch(self, items):
        with self.lock:
            for it in items:
                self.ingest(it, backfill=True)
            self.db.commit()
            self._bump()

    # ======================= 异常监测与告警 =======================
    def _raise(self, did, atype, level, msg, value=None, event=False):
        key = (did, atype)
        a = self.active.get(key)
        if a and not event:
            a["msg"], a["value"] = msg, value
            if LEVEL_ORDER[level] < LEVEL_ORDER[a["level"]]:
                a["level"] = level
            return
        self.alarm_seq += 1
        ts = self.sim_time.strftime("%Y-%m-%d %H:%M") if self.sim_time else ""
        a = {"id": self.alarm_seq, "device": did, "name": C.DEVICE_MAP[did]["name"], "type": atype,
             "level": level, "msg": msg, "value": value, "start_ts": ts, "end_ts": ts if event else None,
             "active": not event, "acked": False, "real_time": time.strftime("%H:%M:%S")}
        self.alarms.insert(0, a)
        del self.alarms[300:]
        if not event:
            self.active[key] = a
        self.db.execute("INSERT INTO alarms VALUES(?,?,?,?,?,?,?)",
                        (a["id"], did, atype, level, msg, ts, a["end_ts"]))

    def _clear(self, did, atype):
        a = self.active.pop((did, atype), None)
        if a:
            a["active"] = False
            a["end_ts"] = self.sim_time.strftime("%Y-%m-%d %H:%M") if self.sim_time else ""
            self.db.execute("UPDATE alarms SET end_ts=? WHERE id=?", (a["end_ts"], a["id"]))

    def _evaluate(self, received, prev_power, pue, ts):
        for did in received:
            dev, cur = C.DEVICE_MAP[did], self.latest[did]
            pw = cur["power"]
            self._clear(did, "offline")
            if dev["kind"] == "load":
                th = self.thresholds.get(did)
                if th and pw > th:
                    lvl = "严重" if pw > dev["rated"] else "警告"
                    self._raise(did, "overload", lvl, f"功率超限：{pw:.1f} kW > 阈值 {th:.0f} kW", r2(pw, 1))
                else:
                    self._clear(did, "overload")
                pp = prev_power.get(did)
                if pp is not None and abs(pw - pp) > 0.2 * dev["rated"] and \
                        abs(pw - pp) / max(pp, 0.1 * dev["rated"]) > C.JUMP_RATIO:
                    self._raise(did, "jump", "提示", f"功率突变：{pp:.1f} → {pw:.1f} kW", r2(pw, 1), event=True)
            elif dev["kind"] == "pv":
                irr = cur["extra"].get("irradiance", 0)
                expv = cur["extra"].get("expected", 0)
                if irr > 200 and expv > 0 and pw < C.PV_RATIO_LOW * expv:
                    self._raise(did, "pv_low", "警告",
                                f"光伏出力异常：实际 {pw:.0f} kW / 理论 {expv:.0f} kW（辐照 {irr:.0f} W/m²）", r2(pw, 1))
                else:
                    self._clear(did, "pv_low")
            elif dev["kind"] == "ess":
                soc = cur["extra"].get("soc")
                if soc is not None and soc < C.SOC_LOW:
                    self._raise(did, "soc_low", "提示", f"储能 SOC 过低：{soc*100:.1f}%", r2(soc, 3))
                else:
                    self._clear(did, "soc_low")
        if {"dc_it", "dc_cool"} <= received:
            if pue > C.PUE_LIMIT:
                self._raise("dc_cool", "pue_high", "警告", f"数据中心 PUE 超限：{pue:.2f} > {C.PUE_LIMIT}", r2(pue))
            else:
                self._clear("dc_cool", "pue_high")

    def _watchdog(self):
        """后台线程：检测监测数据中断（超过 OFFLINE_SECONDS 未收到某对象数据）。"""
        while True:
            time.sleep(0.5)
            with self.lock:
                now, changed = time.time(), False
                for did, seen in self.last_seen.items():
                    gap = now - seen
                    if gap > C.OFFLINE_SECONDS and (did, "offline") not in self.active:
                        self._raise(did, "offline", "严重", f"监测数据中断：已 {gap:.0f} 秒未收到数据")
                        changed = True
                if changed:
                    self.db.commit()
                    self._bump()

    # ======================= 控制接口 =======================
    def add_command(self, cmd):
        with self.lock:
            self.commands.append(cmd)

    def pop_commands(self):
        with self.lock:
            c, self.commands = self.commands, []
            return c

    def set_threshold(self, did, value):
        with self.lock:
            self.thresholds[did] = float(value)
            self._bump()

    def ack(self, aid=None):
        with self.lock:
            for a in self.alarms:
                if aid is None or a["id"] == aid:
                    a["acked"] = True
            self._bump()

    def _bump(self):
        self.version += 1
        self.cond.notify_all()

    def wait(self, last, timeout=2.0):
        with self.cond:
            self.cond.wait_for(lambda: self.version != last, timeout=timeout)
            return self.version

    # ======================= 统计分析 =======================
    def _h(self, date, hour, key):
        return self.hourly.get((date, hour), {}).get(key, 0.0)

    def stats(self):
        t = self.sim_time
        today = t.strftime("%Y-%m-%d")
        yday = (t - timedelta(days=1)).strftime("%Y-%m-%d")
        cur_h, frac = t.hour, (t.minute + 5) / 60

        def series(date, key, upto=None):
            return [r2(self._h(date, h, key)) if upto is None or h <= upto else None for h in range(24)]

        def total(date, key, upto=None):
            return sum(self._h(date, h, key) for h in range(24 if upto is None else upto + 1))

        def same_time(date, key):          # 昨日截至同一时刻的累计值
            return sum(self._h(date, h, key) for h in range(cur_h)) + self._h(date, cur_h, key) * min(frac, 1)

        loads = [d for d in C.DEVICES if d["kind"] == "load"]
        # ---- (1) 分时段能耗统计及趋势 ----
        hourly = {
            "today_load": series(today, "load", cur_h), "yday_load": series(yday, "load"),
            "today_pv": series(today, "pv", cur_h), "yday_pv": series(yday, "pv"),
            "today_grid": series(today, "grid", cur_h), "today_cost": series(today, "cost", cur_h),
            "today_carbon": series(today, "carbon", cur_h),
            "group": {g: [r2(sum(self._h(today, h, d["id"]) for d in loads if d["group"] == g))
                          if h <= cur_h else None for h in range(24)] for g in GROUPS},
        }
        # ---- (2) 区域/设备能耗占比与排名 ----
        dev_kwh = {d["id"]: total(today, d["id"]) for d in loads}
        load_today = sum(dev_kwh.values()) or 1e-6
        ranking = sorted(
            [{"id": d["id"], "name": d["name"], "group": d["group"], "kwh": r2(dev_kwh[d["id"]], 1),
              "pct": r2(dev_kwh[d["id"]] / load_today * 100, 1),
              "yday": r2(same_time(yday, d["id"]), 1),
              "cost": r2(total(today, "cost_" + d["id"]), 1)} for d in loads],
            key=lambda x: -x["kwh"])
        for x in ranking:
            x["change"] = r2((x["kwh"] - x["yday"]) / x["yday"] * 100, 1) if x["yday"] else None
        group_share = [{"name": g, "kwh": r2(sum(dev_kwh[d["id"]] for d in loads if d["group"] == g), 1)}
                       for g in GROUPS]
        # ---- (3) 峰谷平时段分析 ----
        periods = []
        for k, v in C.TARIFF.items():
            hs = [h for h in range(24) if C.HOUR_PERIOD[h] == k]
            periods.append({"key": k, "name": v["name"], "price": v["price"], "color": v["color"],
                            "hours": len(hs),
                            "load": r2(sum(self._h(today, h, "load") for h in hs), 1),
                            "grid": r2(sum(self._h(today, h, "grid") for h in hs), 1),
                            "cost": r2(sum(self._h(today, h, "cost") for h in hs), 1)})

        def load_curve_stats(date):
            pts = [x for x in self.ticks if x["ts"].startswith(date)]
            if not pts:
                return None
            mx = max(pts, key=lambda x: x["load"])
            mn = min(pts, key=lambda x: x["load"])
            avg = sum(x["load"] for x in pts) / len(pts)
            return {"max": r2(mx["load"], 1), "max_ts": mx["ts"][11:], "min": r2(mn["load"], 1),
                    "min_ts": mn["ts"][11:], "avg": r2(avg, 1), "diff": r2(mx["load"] - mn["load"], 1),
                    "factor": r2(avg / mx["load"] * 100, 1), "diff_rate": r2((mx["load"] - mn["load"]) / mx["load"] * 100, 1)}
        peak = {"today": load_curve_stats(today), "yday": load_curve_stats(yday)}
        hl = [self._h(today, h, "load") for h in range(cur_h + 1)]
        peak["peak_hour"] = int(max(range(len(hl)), key=lambda i: hl[i])) if hl else None

        # ---- (4) 电费估算 / 绿电 / 碳排放 ----
        pv_t, grid_t, exp_t = total(today, "pv"), total(today, "grid"), total(today, "export")
        cost_t = total(today, "cost")
        cost_tariff = sum(x["cost"] for x in ranking)          # 若全部从电网购电的电费
        energy = {
            "load": r2(load_today, 1), "pv": r2(pv_t, 1), "grid": r2(grid_t, 1), "export": r2(exp_t, 1),
            "ess_dis": r2(total(today, "ess_dis"), 1), "ess_chg": r2(total(today, "ess_chg"), 1),
            "cost": r2(cost_t, 1), "cost_tariff": r2(cost_tariff, 1), "saving": r2(cost_tariff - cost_t, 1),
            "avg_price": r2(cost_t / grid_t, 3) if grid_t else None,
            "green_ratio": r2(min(100.0, (pv_t - exp_t) / load_today * 100), 1),
            "pv_self_use": r2((pv_t - exp_t) / pv_t * 100, 1) if pv_t else None,
            "carbon": r2(total(today, "carbon"), 1), "carbon_avoided": r2(pv_t * C.GRID_EF, 1),
            "yday_load_same": r2(same_time(yday, "load"), 1), "yday_load": r2(total(yday, "load"), 1),
            "yday_cost": r2(total(yday, "cost"), 1), "yday_carbon": r2(total(yday, "carbon"), 1),
            "yday_pv": r2(total(yday, "pv"), 1),
        }
        energy["load_change"] = r2((load_today - energy["yday_load_same"]) / energy["yday_load_same"] * 100, 1) \
            if energy["yday_load_same"] else None
        # ---- (5) 数据中心能效 ----
        pts = [x for x in self.ticks if x["ts"].startswith(today)]
        it_k, cool_k = dev_kwh["dc_it"], dev_kwh["dc_cool"]
        dc = {"pue_avg": r2((it_k + cool_k + 0.04 * it_k) / it_k, 3) if it_k else None,
              "it_kwh": r2(it_k, 1), "cool_kwh": r2(cool_k, 1),
              "pue_max": r2(max((x["pue"] for x in pts), default=0), 3),
              "cool_ratio": r2(cool_k / (it_k + cool_k) * 100, 1) if it_k else None}
        return {"today": today, "yday": yday, "hour": cur_h, "hourly": hourly, "ranking": ranking,
                "group_share": group_share, "periods": periods, "peak": peak, "energy": energy, "dc": dc}

    # ======================= 看板快照 =======================
    def snapshot(self):
        with self.lock:
            if self._snap_cache[0] == self.version:
                return self._snap_cache[1]
            if self.sim_time is None:
                return {"ready": False}
            now = time.time()
            st = self.stats()
            today_kwh = {r["id"]: r["kwh"] for r in st["ranking"]}
            devices = []
            for d in C.DEVICES:
                did = d["id"]
                cur = self.latest.get(did, {"power": 0, "status": "idle", "extra": {}})
                alarms = [a for (k, _), a in self.active.items() if k == did]
                offline = any(a["type"] == "offline" for a in alarms)
                state = "offline" if offline else ("alarm" if alarms else
                                                   ("standby" if cur["status"] == "idle" else "normal"))
                if d["kind"] == "pv":
                    kwh = st["energy"]["pv"]
                elif d["kind"] == "ess":
                    kwh = st["energy"]["ess_dis"]
                else:
                    kwh = today_kwh.get(did)
                devices.append({
                    "id": did, "name": d["name"], "kind": d["kind"], "group": d["group"], "rated": d["rated"],
                    "threshold": self.thresholds.get(did), "power": r2(cur["power"], 1), "today_kwh": kwh,
                    "status": cur["status"], "state": state, "extra": cur["extra"],
                    "rate": r2(abs(cur["power"]) / d["rated"] * 100, 1),
                    "alarm_msgs": [a["msg"] for a in alarms],
                    "last_seen": r2(now - self.last_seen[did], 1) if did in self.last_seen else None,
                })
            last = self.ticks[-1]
            pts = list(self.ticks)[-144:]
            realtime = {"ts": [x["ts"][11:] for x in pts],
                        "date": [x["ts"][:10] for x in pts],
                        "load": [r2(x["load"], 1) for x in pts], "pv": [r2(x["pv"], 1) for x in pts],
                        "grid": [r2(x["grid"], 1) for x in pts], "ess": [r2(x["ess"], 1) for x in pts],
                        "pue": [r2(x["pue"], 3) for x in pts],
                        "soc": [r2((x["soc"] or 0) * 100, 1) for x in pts],
                        "dev": {did: [r2(x["p"][did], 1) if did in x["online"] else None for x in pts]
                                for did in C.DEVICE_MAP}}
            period = C.HOUR_PERIOD[self.sim_time.hour]
            total_rated = sum(d["rated"] for d in C.DEVICES if d["kind"] == "load")
            kpi = {"load": r2(last["load"], 1), "pv": r2(last["pv"], 1), "grid": r2(last["grid"], 1),
                   "ess": r2(last["ess"], 1), "soc": r2((last["soc"] or 0) * 100, 1), "pue": r2(last["pue"], 3),
                   "load_rate": r2(last["load"] / total_rated * 100, 1), "total_rated": total_rated,
                   "carbon_intensity": r2(max(last["grid"], 0) * C.GRID_EF, 1)}
            active = sorted(self.active.values(), key=lambda a: (LEVEL_ORDER[a["level"]], -a["id"]))
            snap = {
                "ready": True, "site": C.SITE_NAME, "sim_time": self.sim_time.strftime("%Y-%m-%d %H:%M"),
                "period": {"key": period, **C.TARIFF[period]},
                "sim_online": any(now - s < C.OFFLINE_SECONDS for s in self.last_seen.values()),
                "devices": devices, "kpi": kpi, "realtime": realtime, "stats": st,
                "alarms": {"active": active, "recent": self.alarms[:60], "total": len(self.alarms),
                           "unacked": sum(1 for a in self.alarms if not a["acked"])},
                "config": {"pue_limit": C.PUE_LIMIT, "grid_ef": C.GRID_EF,
                           "tariff": C.TARIFF, "hour_period": C.HOUR_PERIOD},
            }
            self._snap_cache = (self.version, snap)
            return snap

    def export_csv(self):
        with self.lock:
            st = self.stats()
            loads = [d for d in C.DEVICES if d["kind"] == "load"]
            head = ["小时", "电价时段"] + [d["name"] + "(kWh)" for d in loads] + \
                   ["总负荷电量(kWh)", "光伏发电(kWh)", "电网购电(kWh)", "电费(元)", "碳排放(kgCO2)"]
            lines = [",".join(head)]
            for h in range(st["hour"] + 1):
                row = [f"{h:02d}:00", C.TARIFF[C.HOUR_PERIOD[h]]["name"]] + \
                      [f"{self._h(st['today'], h, d['id']):.2f}" for d in loads] + \
                      [f"{self._h(st['today'], h, k):.2f}" for k in ("load", "pv", "grid", "cost", "carbon")]
                lines.append(",".join(row))
            return "﻿" + "\n".join(lines)
