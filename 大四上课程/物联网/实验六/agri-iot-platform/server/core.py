"""平台核心：MQTT 数据接入、存储、分析、故障检测、联动规则。"""
import base64
import json
import os
import sqlite3
import threading
import time
import uuid
from collections import deque

from common import config as C
from server import analysis as A
from server.recognition import FrameDiff, RecognitionError, recognize

ALARM_TEXT = {
    "sensor_abnormal": "土壤传感器数据异常",
    "sensor_offline": "土壤传感器中断",
    "valve_no_response": "灌溉阀门不响应",
    "pest_suspect": "虫情计数异常(待核实)",
    "pest_false_alarm": "虫情监测误报",
    "pest_confirmed": "虫害确认",
    "drone_fail": "无人机图像缺失/识别失败",
    "disease_warning": "病害预警",
    "camera_offline": "摄像头离线",
}


class Store:
    """SQLite 持久化。"""

    def __init__(self, path):
        os.makedirs(os.path.dirname(path), exist_ok=True)
        self.db = sqlite3.connect(path, check_same_thread=False)
        self.lock = threading.Lock()
        self.db.executescript("""
        CREATE TABLE IF NOT EXISTS soil(gh TEXT, ts REAL, valid INTEGER, data TEXT);
        CREATE TABLE IF NOT EXISTS irrigation(gh TEXT, ts REAL, valve TEXT, flow REAL, total REAL);
        CREATE TABLE IF NOT EXISTS pest(gh TEXT, ts REAL, whitefly INTEGER, aphid INTEGER, thrips INTEGER, flag TEXT);
        CREATE TABLE IF NOT EXISTS drone(mission_id TEXT, gh TEXT, ts REAL, reason TEXT, status TEXT,
                                         grade TEXT, result TEXT, raw TEXT, ann TEXT);
        CREATE TABLE IF NOT EXISTS camera_event(gh TEXT, ts REAL, kind TEXT, change REAL, img TEXT);
        CREATE TABLE IF NOT EXISTS alarm(id TEXT PRIMARY KEY, gh TEXT, device TEXT, type TEXT, level TEXT,
                                         msg TEXT, start_ts REAL, end_ts REAL, status TEXT, recover_msg TEXT);
        CREATE TABLE IF NOT EXISTS command(cmd_id TEXT PRIMARY KEY, gh TEXT, action TEXT, source TEXT,
                                           sent_ts REAL, ack_ts REAL, status TEXT, retries INTEGER);
        CREATE TABLE IF NOT EXISTS linkage(ts REAL, gh TEXT, rule TEXT, msg TEXT);
        CREATE INDEX IF NOT EXISTS i_soil ON soil(gh, ts);
        """)

    def exec(self, sql, args=()):
        with self.lock:
            self.db.execute(sql, args)
            self.db.commit()

    def query(self, sql, args=()):
        with self.lock:
            cur = self.db.execute(sql, args)
            cols = [c[0] for c in cur.description]
            return [dict(zip(cols, r)) for r in cur.fetchall()]


class GHState:
    def __init__(self, cfg):
        self.cfg = cfg
        self.id = cfg["id"]
        self.low = cfg["moisture_low"]
        self.high = cfg["moisture_high"]
        self.auto = True
        # 土壤
        self.soil_all = deque(maxlen=600)       # 所有记录（含无效）用于图表
        self.soil_valid = deque(maxlen=600)     # 有效记录用于分析
        self.soil_last_seen = None
        self.soil_bad_streak = 0
        self.soil_good_streak = 0
        self.soil_latest = None
        # 灌溉
        self.irr = None
        self.irr_last_seen = None
        self.base_liters = None
        self.water_series = deque(maxlen=200)
        self.irr_events = []
        self.cmd_fail = False
        self.auto_next_try = 0
        # 虫情
        self.pest = deque(maxlen=240)
        self.pest_pending = None    # 待无人机核实的异常
        self.pest_suppress = False  # 已核实为误报后，持续的异常计数直接标记为误报
        self.pest_normal_streak = 0
        # 无人机
        self.drone = deque(maxlen=50)
        self.drone_retry_at = None
        # 摄像头
        self.cam_jpg = None
        self.cam_last_seen = None
        self.cam_diff = FrameDiff()
        self.cam_series = deque(maxlen=120)
        self.cam_frames = 0
        self.cam_first = None
        self.motion_events = 0
        self.last_snap_ts = 0
        self.snapshots = deque(maxlen=12)


class Platform:
    def __init__(self):
        os.makedirs(os.path.join(C.IMG_DIR, "drone"), exist_ok=True)
        os.makedirs(os.path.join(C.IMG_DIR, "snap"), exist_ok=True)
        self.store = Store(C.DB_PATH)
        self.lock = threading.RLock()
        self.gh = {g["id"]: GHState(g) for g in C.GREENHOUSES}
        self.alarms = {}                 # (gh,type) -> alarm
        self.alarm_hist = deque(maxlen=200)
        self.commands = {}               # cmd_id -> cmd
        self.cmd_log = deque(maxlen=60)
        self.linkage = deque(maxlen=80)
        self.events = []                 # 待推送给前端的通知
        self.sim = {"faults": {}, "sim_hour": None, "last_seen": None}
        self.mqtt = None                 # 由 app 注入 publish 函数
        self.started = time.time()
        self.msg_count = 0

    # ---------------- 工具 ----------------
    def publish(self, topic, payload):
        if self.mqtt:
            self.mqtt(topic, json.dumps(payload, ensure_ascii=False))

    def notify(self, level, gh, msg):
        self.events.append({"ts": time.time(), "level": level, "gh": gh, "msg": msg})

    def link(self, gh, rule, msg):
        item = {"ts": time.time(), "gh": gh, "rule": rule, "msg": msg}
        self.linkage.appendleft(item)
        self.store.exec("INSERT INTO linkage VALUES(?,?,?,?)", (item["ts"], gh, rule, msg))
        self.notify("info", gh, f"[联动] {msg}")

    def raise_alarm(self, gh, typ, level, msg, device=""):
        key = (gh, typ)
        a = self.alarms.get(key)
        if a:
            a["msg"], a["last_ts"] = msg, time.time()
            return a
        a = {"id": uuid.uuid4().hex[:10], "gh": gh, "device": device, "type": typ,
             "title": ALARM_TEXT.get(typ, typ), "level": level, "msg": msg,
             "start_ts": time.time(), "last_ts": time.time(), "end_ts": None,
             "status": "active", "recover_msg": "", "acked": False}
        self.alarms[key] = a
        self.alarm_hist.appendleft(a)
        self.store.exec("INSERT INTO alarm VALUES(?,?,?,?,?,?,?,?,?,?)",
                        (a["id"], gh, device, typ, level, msg, a["start_ts"], None, "active", ""))
        self.notify(level, gh, f"【告警】{gh} {a['title']}：{msg}")
        return a

    def resolve_alarm(self, gh, typ, recover_msg):
        a = self.alarms.pop((gh, typ), None)
        if not a:
            return
        a["status"], a["end_ts"], a["recover_msg"] = "recovered", time.time(), recover_msg
        self.store.exec("UPDATE alarm SET status='recovered', end_ts=?, recover_msg=? WHERE id=?",
                        (a["end_ts"], recover_msg, a["id"]))
        self.notify("ok", gh, f"【恢复】{gh} {a['title']}：{recover_msg}（持续 {a['end_ts'] - a['start_ts']:.0f}s）")

    def has_alarm(self, gh, typ):
        return (gh, typ) in self.alarms

    # ---------------- MQTT 消息入口 ----------------
    def on_message(self, topic, payload):
        try:
            data = json.loads(payload)
        except ValueError:
            return
        parts = topic.split("/")
        with self.lock:
            self.msg_count += 1
            if topic == C.T_SIM_STATUS:
                self.sim.update(data)
                self.sim["last_seen"] = time.time()
                return
            gh = parts[1]
            if gh not in self.gh:
                return
            kind = "/".join(parts[2:])
            handler = {
                "soil": self.on_soil, "irrigation/status": self.on_irr_status,
                "irrigation/ack": self.on_irr_ack, "pest": self.on_pest,
                "drone/image": self.on_drone, "camera/frame": self.on_camera,
            }.get(kind)
            if handler:
                handler(self.gh[gh], data)

    # ---------- 1. 土壤/环境 ----------
    def validate_soil(self, g, d):
        problems = []
        for k, (lo, hi) in C.VALID_RANGE.items():
            v = d.get(k)
            if v is None or not (lo <= v <= hi):
                problems.append(f"{k}={v} 超出量程[{lo},{hi}]")
        recent = list(g.soil_all)[-(C.STUCK_COUNT - 1):]
        if len(recent) == C.STUCK_COUNT - 1 and all(
                r["soil_moisture"] == d["soil_moisture"] and r["soil_temp"] == d["soil_temp"] for r in recent):
            problems.append(f"连续 {C.STUCK_COUNT} 次读数完全相同（数据卡死）")
        return problems

    def on_soil(self, g, d):
        now = time.time()
        if g.soil_last_seen is None or self.has_alarm(g.id, "sensor_offline"):
            if self.has_alarm(g.id, "sensor_offline"):
                self.resolve_alarm(g.id, "sensor_offline", f"数据恢复上报，中断 {now - g.soil_last_seen:.0f}s")
        g.soil_last_seen = now
        problems = self.validate_soil(g, d)
        d["valid"] = not problems
        d["problems"] = problems
        g.soil_all.append(d)
        g.soil_latest = d
        self.store.exec("INSERT INTO soil VALUES(?,?,?,?)", (g.id, d["ts"], int(d["valid"]), json.dumps(d)))
        if problems:
            g.soil_bad_streak += 1
            g.soil_good_streak = 0
            self.raise_alarm(g.id, "sensor_abnormal", "critical", "；".join(problems)[:120], d.get("device", ""))
        else:
            g.soil_good_streak += 1
            g.soil_bad_streak = 0
            g.soil_valid.append(d)
            if self.has_alarm(g.id, "sensor_abnormal") and g.soil_good_streak >= 3:
                self.resolve_alarm(g.id, "sensor_abnormal",
                                   f"连续 3 次读数正常，湿度 {d['soil_moisture']}%")

    # ---------- 2. 灌溉 ----------
    def on_irr_status(self, g, d):
        prev = g.irr
        g.irr = d
        g.irr_last_seen = time.time()
        if g.base_liters is None:
            g.base_liters = d["total_liters"]
        g.water_series.append({"ts": d["ts"], "total": round(d["total_liters"] - g.base_liters, 1),
                               "flow": d["flow_lpm"]})
        self.store.exec("INSERT INTO irrigation VALUES(?,?,?,?,?)",
                        (g.id, d["ts"], d["valve"], d["flow_lpm"], d["total_liters"]))
        m = g.soil_valid[-1]["soil_moisture"] if g.soil_valid else None
        if prev and prev["valve"] != d["valve"]:
            if d["valve"] == "open":
                g.irr_events.append({"start_ts": d["ts"], "m_start": m, "l_start": d["total_liters"]})
            elif g.irr_events and not g.irr_events[-1].get("end_ts"):
                e = g.irr_events[-1]
                e.update(end_ts=d["ts"], m_end=m, liters=round(d["total_liters"] - e["l_start"], 1))
                if e["m_start"] is None or m is None:
                    e["m_start"] = e["m_end"] = 0

    def send_irr_cmd(self, gh, action, source, duration=None):
        g = self.gh[gh]
        cmd = {"cmd_id": uuid.uuid4().hex[:8], "gh": gh, "action": action, "source": source,
               "duration": duration, "sent_ts": time.time(), "ack_ts": None, "status": "pending",
               "retries": 0}
        self.commands[cmd["cmd_id"]] = cmd
        self.cmd_log.appendleft(cmd)
        self.store.exec("INSERT INTO command VALUES(?,?,?,?,?,?,?,?)",
                        (cmd["cmd_id"], gh, action, source, cmd["sent_ts"], None, "pending", 0))
        self.publish(C.T_IRR_CMD.format(gh=gh), {"cmd_id": cmd["cmd_id"], "action": action,
                                                 "duration": duration, "source": source,
                                                 "device": g.cfg["devices"]["valve"]})
        return cmd

    def on_irr_ack(self, g, d):
        cmd = self.commands.get(d.get("cmd_id"))
        if not cmd:
            return
        cmd["ack_ts"] = time.time()
        cmd["status"] = "done"
        cmd["latency_ms"] = round((cmd["ack_ts"] - cmd["sent_ts"]) * 1000)
        self.store.exec("UPDATE command SET ack_ts=?, status='done' WHERE cmd_id=?", (cmd["ack_ts"], cmd["cmd_id"]))
        if g.cmd_fail or self.has_alarm(g.id, "valve_no_response"):
            g.cmd_fail = False
            self.resolve_alarm(g.id, "valve_no_response", f"指令 {cmd['cmd_id']} 已应答，阀门{'开启' if d['valve'] == 'open' else '关闭'}")

    def check_commands(self, now):
        for cmd in list(self.commands.values()):
            if cmd["status"] not in ("pending", "retrying"):
                continue
            if now - cmd["sent_ts"] < C.CMD_ACK_TIMEOUT * (cmd["retries"] + 1):
                continue
            g = self.gh[cmd["gh"]]
            if cmd["retries"] < C.CMD_MAX_RETRY:
                cmd["retries"] += 1
                cmd["status"] = "retrying"
                self.publish(C.T_IRR_CMD.format(gh=cmd["gh"]), {"cmd_id": cmd["cmd_id"], "action": cmd["action"],
                                                                "duration": cmd["duration"], "source": cmd["source"],
                                                                "retry": cmd["retries"]})
                self.notify("warning", cmd["gh"], f"{cmd['gh']} 灌溉指令 {cmd['cmd_id']} 应答超时，第 {cmd['retries']} 次重发")
            else:
                cmd["status"] = "failed"
                g.cmd_fail = True
                self.store.exec("UPDATE command SET status='failed', retries=? WHERE cmd_id=?",
                                (cmd["retries"], cmd["cmd_id"]))
                self.raise_alarm(cmd["gh"], "valve_no_response", "critical",
                                 f"{g.cfg['devices']['valve']} 对指令「{'开阀' if cmd['action'] == 'open' else '关阀'}」"
                                 f"重试 {cmd['retries']} 次仍无应答，请现场检修", g.cfg["devices"]["valve"])

    def auto_irrigation(self, g, now):
        """联动规则1：土壤湿度滞环控制自动灌溉。"""
        if not g.auto or not g.irr:
            return
        if self.has_alarm(g.id, "sensor_abnormal") or self.has_alarm(g.id, "sensor_offline"):
            if g.irr["valve"] == "open" and now >= g.auto_next_try:
                self.send_irr_cmd(g.id, "close", "auto-safety")
                g.auto_next_try = now + 10
                self.link(g.id, "安全保护", f"{g.id} 土壤传感器故障，自动灌溉暂停并关阀，避免误灌")
            return
        if len(g.soil_valid) < 3 or now < g.auto_next_try:
            return
        if any(c["gh"] == g.id and c["status"] in ("pending", "retrying") for c in self.commands.values()):
            return
        m = sum(r["soil_moisture"] for r in list(g.soil_valid)[-3:]) / 3
        if m < g.low and g.irr["valve"] == "closed":
            self.send_irr_cmd(g.id, "open", "auto")
            g.auto_next_try = now + 8
            self.link(g.id, "自动灌溉", f"{g.id} 土壤湿度均值 {m:.1f}% < 下限 {g.low}%，自动下发开阀指令")
        elif m > g.high and g.irr["valve"] == "open":
            self.send_irr_cmd(g.id, "close", "auto")
            g.auto_next_try = now + 8
            self.link(g.id, "自动灌溉", f"{g.id} 土壤湿度均值 {m:.1f}% > 上限 {g.high}%，自动关阀")

    # ---------- 3. 虫情 ----------
    def on_pest(self, g, d):
        item = {"ts": d["ts"], "counts": d["counts"], "flag": None}
        g.pest.append(item)
        an = A.pest_analysis(list(g.pest))
        if an and an["anomaly"] and g.pest_suppress:
            item["flag"] = "false_alarm"
            g.pest_normal_streak = 0
        elif an and an["anomaly"] and self.has_alarm(g.id, "pest_confirmed"):
            item["flag"] = "confirmed"      # 已确认虫害期间不重复派发核查
        elif an and an["anomaly"]:
            item["flag"] = "suspect"
            if not g.pest_pending:
                mid = "P-" + uuid.uuid4().hex[:6]
                g.pest_pending = {"mission_id": mid, "ts": time.time(), "count": d["counts"]["whitefly"],
                                  "baseline": an["baseline"], "z": an["zscore"]}
                self.raise_alarm(g.id, "pest_suspect", "warning",
                                 f"白粉虱计数 {d['counts']['whitefly']} 头，基线 {an['baseline']}，z={an['zscore']}",
                                 g.cfg["devices"]["pest"])
                self.request_drone(g.id, "pest_verify", mid)
                self.link(g.id, "虫情→无人机核查",
                          f"{g.id} 虫情计数突增（z={an['zscore']}），已派无人机 {mid} 前往核查")
        elif g.pest_pending:
            item["flag"] = "pending"
        elif g.pest_suppress:
            g.pest_normal_streak += 1
            if g.pest_normal_streak >= 2:
                g.pest_suppress = False
                self.resolve_alarm(g.id, "pest_false_alarm", "测报灯计数恢复正常")
        self.store.exec("INSERT INTO pest VALUES(?,?,?,?,?,?)",
                        (g.id, d["ts"], d["counts"]["whitefly"], d["counts"]["aphid"], d["counts"]["thrips"],
                         item["flag"]))

    # ---------- 4. 无人机 ----------
    def request_drone(self, gh, reason, mission_id=None):
        mid = mission_id or ("M-" + uuid.uuid4().hex[:6])
        self.publish(C.T_DRONE_CMD, {"mission_id": mid, "gh": gh, "reason": reason})
        return mid

    def on_drone(self, g, d):
        mid = d.get("mission_id")
        rec = {"mission_id": mid, "gh": g.id, "ts": d["ts"], "reason": d.get("reason"),
               "battery": d.get("battery"), "status": "ok", "result": None, "raw": None, "ann": None}
        raw = None
        try:
            if not d.get("image"):
                raise RecognitionError("图像缺失（" + d.get("error", "无数据") + "）")
            raw = base64.b64decode(d["image"])
            fname = f"{mid}_{int(d['ts'])}"
            with open(os.path.join(C.IMG_DIR, "drone", fname + "_raw.jpg"), "wb") as f:
                f.write(raw)
            rec["raw"] = f"/images/drone/{fname}_raw.jpg"
            res, ann = recognize(raw)
            with open(os.path.join(C.IMG_DIR, "drone", fname + "_ann.jpg"), "wb") as f:
                f.write(ann)
            rec["ann"] = f"/images/drone/{fname}_ann.jpg"
            rec["result"] = res
        except (RecognitionError, ValueError) as e:
            rec["status"] = "failed"
            rec["error"] = str(e)
        g.drone.appendleft(rec)
        self.store.exec("INSERT INTO drone VALUES(?,?,?,?,?,?,?,?,?)",
                        (mid, g.id, d["ts"], rec["reason"], rec["status"],
                         rec["result"]["grade"] if rec["result"] else None,
                         json.dumps(rec["result"], ensure_ascii=False) if rec["result"] else rec.get("error"),
                         rec["raw"], rec["ann"]))
        if rec["status"] == "failed":
            self.raise_alarm(g.id, "drone_fail", "warning", f"任务 {mid}：{rec['error']}", C.DRONE_ID)
            if g.drone_retry_at is None:
                g.drone_retry_at = time.time() + 5
                self.notify("warning", g.id, f"{g.id} 无人机识别失败，5 秒后自动重新巡检")
            return
        g.drone_retry_at = None
        if self.has_alarm(g.id, "drone_fail"):
            self.resolve_alarm(g.id, "drone_fail", f"任务 {mid} 图像正常，识别结果：{rec['result']['grade']}")
        res = rec["result"]
        # 联动规则3：识别为中度及以上 → 病害预警
        if res["level"] >= 2:
            self.raise_alarm(g.id, "disease_warning", "warning",
                             f"无人机识别 {res['grade']}：病斑 {res['lesion_count']} 处，白粉虱 {res['whitefly_clusters']} 簇，"
                             f"建议：通风降湿，摘除病叶，悬挂黄板/释放丽蚜小蜂", C.DRONE_ID)
        elif self.has_alarm(g.id, "disease_warning") and res["level"] <= 1:
            self.resolve_alarm(g.id, "disease_warning", f"复查结果 {res['grade']}，病情得到控制")
        # 联动规则2：虫情异常的核查结论
        if g.pest_pending:
            p = g.pest_pending
            g.pest_pending = None
            confirmed = res["whitefly_clusters"] >= 8 or res["level"] >= 2
            for it in g.pest:
                if it["flag"] in ("suspect", "pending"):
                    it["flag"] = "confirmed" if confirmed else "false_alarm"
            self.resolve_alarm(g.id, "pest_suspect", "无人机核查完成")
            if confirmed:
                self.raise_alarm(g.id, "pest_confirmed", "critical",
                                 f"无人机核查确认：白粉虱 {res['whitefly_clusters']} 簇、病斑 {res['lesion_count']} 处（{res['grade']}）",
                                 g.cfg["devices"]["pest"])
                self.link(g.id, "核查结论", f"{g.id} 虫情异常经无人机图像确认为真实虫害，已生成防治告警")
            else:
                g.pest_suppress = True
                g.pest_normal_streak = 0
                self.raise_alarm(g.id, "pest_false_alarm", "warning",
                                 f"计数 {p['count']} 头但图像仅见白粉虱 {res['whitefly_clusters']} 簇（{res['grade']}），"
                                 f"判定为误报；异常数据已剔除出基线，请检查测报灯（光源干扰/非目标昆虫）",
                                 g.cfg["devices"]["pest"])
                self.link(g.id, "核查结论", f"{g.id} 虫情计数突增经无人机核查为误报，已剔除异常数据")
        elif self.has_alarm(g.id, "pest_confirmed") and res["whitefly_clusters"] < 4:
            self.resolve_alarm(g.id, "pest_confirmed", f"复查白粉虱 {res['whitefly_clusters']} 簇，虫害已缓解")

    # ---------- 5. 摄像头 ----------
    def on_camera(self, g, d):
        now = time.time()
        if self.has_alarm(g.id, "camera_offline"):
            self.resolve_alarm(g.id, "camera_offline", f"画面恢复，离线 {now - g.cam_last_seen:.0f}s")
            g.cam_diff.prev = None
        g.cam_last_seen = now
        if g.cam_first is None:
            g.cam_first = now
        g.cam_frames += 1
        jpg = base64.b64decode(d["image"])
        g.cam_jpg = jpg
        change, bright = g.cam_diff.update(jpg)
        motion = change > 1.2
        g.cam_series.append({"ts": d["ts"], "change": round(change, 2), "bright": round(bright, 1),
                             "motion": motion})
        if motion and now - g.last_snap_ts > 6:
            g.last_snap_ts = now
            g.motion_events += 1
            fname = f"{g.id}_{int(now * 1000)}.jpg"
            with open(os.path.join(C.IMG_DIR, "snap", fname), "wb") as f:
                f.write(jpg)
            snap = {"ts": now, "kind": "motion", "change": round(change, 2), "img": f"/images/snap/{fname}"}
            g.snapshots.appendleft(snap)
            self.store.exec("INSERT INTO camera_event VALUES(?,?,?,?,?)", (g.id, now, "motion", change, snap["img"]))

    def manual_snapshot(self, gh):
        g = self.gh[gh]
        if not g.cam_jpg or self.has_alarm(gh, "camera_offline"):
            return None
        now = time.time()
        fname = f"{gh}_{int(now * 1000)}_m.jpg"
        with open(os.path.join(C.IMG_DIR, "snap", fname), "wb") as f:
            f.write(g.cam_jpg)
        snap = {"ts": now, "kind": "manual", "change": 0, "img": f"/images/snap/{fname}"}
        g.snapshots.appendleft(snap)
        self.store.exec("INSERT INTO camera_event VALUES(?,?,?,?,?)", (gh, now, "manual", 0, snap["img"]))
        return snap

    # ---------------- 定时巡检（每秒） ----------------
    def tick(self):
        now = time.time()
        with self.lock:
            for g in self.gh.values():
                if g.soil_last_seen and now - g.soil_last_seen > C.SOIL_TIMEOUT:
                    self.raise_alarm(g.id, "sensor_offline", "critical",
                                     f"{g.cfg['devices']['soil']} 已 {now - g.soil_last_seen:.0f}s 未上报数据",
                                     g.cfg["devices"]["soil"])
                if g.cam_last_seen and now - g.cam_last_seen > C.CAMERA_TIMEOUT:
                    self.raise_alarm(g.id, "camera_offline", "warning",
                                     f"{g.cfg['devices']['camera']} 已 {now - g.cam_last_seen:.0f}s 无画面，显示最后一帧",
                                     g.cfg["devices"]["camera"])
                if g.drone_retry_at and now >= g.drone_retry_at:
                    g.drone_retry_at = now + 30   # 防止重复派发，收到结果后重置
                    mid = g.pest_pending["mission_id"] if g.pest_pending else None
                    self.request_drone(g.id, "retry", mid)
                    self.link(g.id, "故障重试", f"{g.id} 无人机图像异常，自动重新派发巡检任务")
                # 虫情核查超时（无人机持续失败）
                if g.pest_pending and now - g.pest_pending["ts"] > 60:
                    g.pest_pending = None
                    self.resolve_alarm(g.id, "pest_suspect", "核查超时，需人工巡田确认")
                self.auto_irrigation(g, now)
            self.check_commands(now)

    # ---------------- 前端快照 ----------------
    def device_status(self, g, now):
        def st(last, timeout, alarm_types):
            if last is None:
                return "unknown"
            if any(self.has_alarm(g.id, t) for t in alarm_types):
                return "fault"
            return "online" if now - last <= timeout else "offline"
        drone_last = g.drone[0]["ts"] if g.drone else None
        return [
            {"id": g.cfg["devices"]["soil"], "type": "土壤/环境传感器",
             "status": st(g.soil_last_seen, C.SOIL_TIMEOUT, ["sensor_abnormal", "sensor_offline"])},
            {"id": g.cfg["devices"]["valve"], "type": "电磁阀+流量计",
             "status": st(g.irr_last_seen, 8, ["valve_no_response"])},
            {"id": g.cfg["devices"]["pest"], "type": "虫情测报灯",
             "status": st(g.pest[-1]["ts"] if g.pest else None, 15, ["pest_suspect"])},
            {"id": C.DRONE_ID, "type": "巡检无人机(共享)",
             "status": st(drone_last, 600, ["drone_fail"])},
            {"id": g.cfg["devices"]["camera"], "type": "高清摄像头",
             "status": st(g.cam_last_seen, C.CAMERA_TIMEOUT, ["camera_offline"])},
        ]

    def gh_snapshot(self, g, now):
        soil_series = [{"ts": r["ts"], "m": r["soil_moisture"], "st": r["soil_temp"], "at": r["air_temp"],
                        "ah": r["air_humidity"], "light": r["light"], "co2": r["co2"], "ec": r["ec"],
                        "valid": r["valid"]} for r in list(g.soil_all)[-90:]]
        ma = A.moving_avg([s["m"] for s in soil_series if s["valid"]], 5)
        soil_an = A.soil_analysis(list(g.soil_valid), g.low, g.high)
        irr_an = A.irrigation_analysis(g.irr_events, g.irr["total_liters"] if g.irr else 0,
                                       g.base_liters or 0, g.cfg["area_m2"])
        pest_series = list(g.pest)
        pest_an = A.pest_analysis(pest_series)
        drones = list(g.drone)
        ok = [x for x in drones if x["status"] == "ok"]
        grade_dist = {}
        for x in ok:
            grade_dist[x["result"]["grade"]] = grade_dist.get(x["result"]["grade"], 0) + 1
        cam = list(g.cam_series)
        expected = max(1, (now - g.cam_first) / C.CAMERA_INTERVAL) if g.cam_first else 1
        changes = [c["change"] for c in cam[-30:]]
        return {
            "id": g.id, "cfg": g.cfg, "auto": g.auto, "low": g.low, "high": g.high,
            "devices": self.device_status(g, now),
            "soil": {"latest": g.soil_latest, "series": soil_series, "ma": ma, "analysis": soil_an},
            "irrigation": {"status": g.irr, "cmd_fail": g.cmd_fail, "analysis": irr_an,
                           "water_series": list(g.water_series)[-90:],
                           "events": [dict(e) for e in g.irr_events[-6:]][::-1],
                           "pending": [c for c in self.commands.values()
                                       if c["gh"] == g.id and c["status"] in ("pending", "retrying")]},
            "pest": {"series": [{"ts": p["ts"], **p["counts"], "flag": p["flag"]} for p in pest_series[-36:]],
                     "analysis": pest_an, "pending": g.pest_pending},
            "drone": {"latest": drones[0] if drones else None,
                      "history": [{**x, "result": {k: v for k, v in x["result"].items() if k != "detections"}
                                   if x["result"] else None} for x in drones[:8]],
                      "stats": {"missions": len(drones), "success": len(ok),
                                "success_rate": round(len(ok) / len(drones) * 100, 1) if drones else None,
                                "grade_dist": grade_dist,
                                "lesion_trend": [x["result"]["lesion_count"] for x in ok[:10]][::-1],
                                "fly_trend": [x["result"]["whitefly_clusters"] for x in ok[:10]][::-1]}},
            "camera": {"online": not self.has_alarm(g.id, "camera_offline") and g.cam_last_seen is not None,
                       "last_ts": g.cam_last_seen, "series": cam[-60:],
                       "stats": {"frames": g.cam_frames, "online_rate": round(min(100.0, g.cam_frames / expected * 100), 1),
                                 "motion_events": g.motion_events,
                                 "avg_change": round(sum(changes) / len(changes), 2) if changes else 0,
                                 "brightness": cam[-1]["bright"] if cam else None},
                       "snapshots": list(g.snapshots)[:6]},
        }

    def snapshot(self):
        now = time.time()
        with self.lock:
            events, self.events = self.events, []
            data = {
                "ts": now, "uptime": round(now - self.started), "msg_count": self.msg_count,
                "sim": {"online": bool(self.sim.get("last_seen")) and now - self.sim["last_seen"] < 10,
                        "hour": self.sim.get("sim_hour"), "daylight": self.sim.get("daylight"),
                        "faults": self.sim.get("faults", {})},
                "greenhouses": {gid: self.gh_snapshot(g, now) for gid, g in self.gh.items()},
                "alarms": sorted(self.alarms.values(), key=lambda a: -a["start_ts"]),
                "alarm_hist": list(self.alarm_hist)[:40],
                "linkage": list(self.linkage)[:30],
                "commands": list(self.cmd_log)[:20],
                "events": events,
            }
            return json.loads(json.dumps(data, ensure_ascii=False, default=str))
