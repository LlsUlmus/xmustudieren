"""农业物联网虚拟设备模拟器。

模拟 3 个番茄大棚中的：土壤/环境传感器、灌溉阀门与水泵、虫情测报灯、
摄像头，以及 1 架共享的巡检无人机。全部通过 MQTT 与服务器通讯。

运行：python -m simulator.sim
"""
import base64
import json
import math
import os
import random
import sys
import threading
import time
import uuid

import paho.mqtt.client as mqtt

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from common import config as C  # noqa: E402
from simulator import imagegen  # noqa: E402

T0 = time.time()


def sim_hour():
    """加速后的模拟时刻（0~24），从上午 9 点开始。"""
    return (9 + (time.time() - T0) * C.TIME_SCALE / 3600.0) % 24


def daylight():
    hr = sim_hour()
    return max(0.0, math.sin(math.pi * (hr - 6) / 12)) if 6 <= hr <= 18 else 0.0


def poisson(lam):
    if lam > 30:
        return max(0, int(random.gauss(lam, math.sqrt(lam))))
    L, k, p = math.exp(-lam), 0, 1.0
    while True:
        p *= random.random()
        if p < L:
            return k
        k += 1


class Greenhouse:
    DEFAULT_DISEASE = {"GH-1": 0.06, "GH-2": 0.16, "GH-3": 0.02}

    def __init__(self, idx, cfg):
        self.idx = idx
        self.cfg = cfg
        self.id = cfg["id"]
        self.moisture = cfg["moisture_low"] + 6 + idx * 2
        self.soil_temp = 20.0
        self.air_temp = 24.0
        self.humidity = 70.0
        self.ec = 1.9
        self.valve_open = False
        self.valve_close_at = None
        self.flow = 0.0
        self.total_liters = 0.0
        self.disease = self.DEFAULT_DISEASE[self.id]
        self.pest_mult = 1.0
        self.faults = set()
        self.stuck = None
        self.person_x = None
        self.person_v = 0

    # ---- 物理过程 ----
    def step(self, dt):
        dl = daylight()
        seedling = self.cfg["stage"] == "育苗期"
        evap = (0.07 + 0.16 * dl) * (1.3 if seedling else 1.0)
        if self.valve_close_at and time.time() >= self.valve_close_at:
            self.valve_open, self.valve_close_at = False, None
        if self.valve_open:
            self.flow = (9.0 if seedling else 18.0) + random.uniform(-0.6, 0.6)
            self.moisture += 1.05 * dt
            self.total_liters += self.flow * dt * C.TIME_SCALE / 60.0
        else:
            self.flow = 0.0
        self.moisture -= evap * dt
        self.moisture = max(18.0, min(96.0, self.moisture + random.uniform(-0.08, 0.08)))
        target_air = 15 + 15 * dl + (2 if self.idx == 1 else 0)
        self.air_temp += (target_air - self.air_temp) * 0.05 * dt + random.uniform(-0.1, 0.1)
        self.soil_temp += (self.air_temp - 3 - self.soil_temp) * 0.01 * dt
        target_h = 88 - 28 * dl + (6 if self.valve_open else 0)
        self.humidity += (target_h - self.humidity) * 0.05 * dt + random.uniform(-0.3, 0.3)
        self.humidity = max(30, min(99, self.humidity))
        self.ec = 1.6 + (70 - self.moisture) * 0.015 + random.uniform(-0.03, 0.03)
        # 病害：高湿促进发展，低湿缓慢消退
        if self.humidity > 85:
            self.disease += 0.0006 * dt
        else:
            self.disease -= 0.0002 * dt
        self.disease = max(0.01, min(0.95, self.disease))

    def soil_payload(self):
        dl = daylight()
        p = {
            "device": self.cfg["devices"]["soil"], "gh": self.id, "ts": time.time(),
            "soil_moisture": round(self.moisture, 2),
            "soil_temp": round(self.soil_temp, 2),
            "ec": round(self.ec, 3),
            "air_temp": round(self.air_temp, 2),
            "air_humidity": round(self.humidity, 2),
            "light": round(62000 * dl * random.uniform(0.85, 1.0), 0),
            "co2": round(420 + (1 - dl) * 330 + random.uniform(-15, 15), 0),
            "battery": round(92 - self.idx * 7 - (time.time() - T0) / 600, 1),
            "rssi": random.randint(-82, -60),
        }
        if "sensor_abnormal" in self.faults:
            # 两种异常交替出现：超量程跳变 / 数据卡死不变
            if random.random() < 0.5:
                p["soil_moisture"] = random.choice([-9.9, 0.0, 148.5])
                p["soil_temp"] = random.choice([85.0, -40.0])
            else:
                if self.stuck is None:
                    self.stuck = dict(p)
                for k in ("soil_moisture", "soil_temp", "ec"):
                    p[k] = self.stuck[k]
        else:
            self.stuck = None
        return p

    def irr_payload(self):
        return {
            "device": self.cfg["devices"]["valve"], "gh": self.id, "ts": time.time(),
            "valve": "open" if self.valve_open else "closed",
            "flow_lpm": round(self.flow, 2),
            "total_liters": round(self.total_liters, 1),
            "pressure_kpa": round(160 + random.uniform(-6, 6) if self.valve_open else 0, 1),
        }

    def pest_payload(self):
        temp_factor = max(0.2, (self.air_temp - 10) / 15)
        base = {"whitefly": 5.0, "aphid": 2.5, "thrips": 1.5}
        counts = {}
        for sp, b in base.items():
            mult = self.pest_mult if sp == "whitefly" else (1 + (self.pest_mult - 1) * 0.3)
            counts[sp] = poisson(b * temp_factor * mult * (1.3 if self.idx == 1 else 1))
        if "pest_false_alarm" in self.faults:
            # 误报：如飞入的大量非目标昆虫/光照干扰，计数暴增，但棚内实际虫口并未增加
            counts["whitefly"] += random.randint(90, 160)
        return {"device": self.cfg["devices"]["pest"], "gh": self.id, "ts": time.time(),
                "interval_s": C.PEST_INTERVAL, "counts": counts,
                "trap": "黄板+诱虫灯"}

    def whitefly_density(self):
        return min(1.0, 0.06 * self.pest_mult + max(0, self.air_temp - 20) * 0.004)


class Simulator:
    def __init__(self):
        self.ghs = {g["id"]: Greenhouse(i, g) for i, g in enumerate(C.GREENHOUSES)}
        self.lock = threading.Lock()
        self.drone_queue = []
        self.drone_rr = 0
        self.drone_battery = 100.0
        self.client = mqtt.Client(mqtt.CallbackAPIVersion.VERSION2, client_id="agri-simulator-" + uuid.uuid4().hex[:6])
        self.client.on_connect = self.on_connect
        self.client.on_message = self.on_message

    # ---- MQTT ----
    def on_connect(self, client, userdata, flags, rc, props=None):
        print("[sim] MQTT 已连接", rc)
        client.subscribe([("agri/+/irrigation/cmd", 1), (C.T_DRONE_CMD, 1),
                          (C.T_SIM_FAULT, 1), (C.T_SIM_SCENARIO, 1)])

    def on_message(self, client, userdata, msg):
        try:
            data = json.loads(msg.payload)
        except ValueError:
            return
        with self.lock:
            if msg.topic.endswith("/irrigation/cmd"):
                self.handle_irr_cmd(msg.topic.split("/")[1], data)
            elif msg.topic == C.T_DRONE_CMD:
                self.drone_queue.insert(0, data)
                print("[sim] 无人机收到巡检任务", data)
            elif msg.topic == C.T_SIM_FAULT:
                g = self.ghs.get(data.get("gh"))
                if g:
                    (g.faults.add if data.get("on") else g.faults.discard)(data.get("fault"))
                    print("[sim] 故障设置", data)
            elif msg.topic == C.T_SIM_SCENARIO:
                self.handle_scenario(data)

    def handle_irr_cmd(self, gh, data):
        g = self.ghs.get(gh)
        if not g:
            return
        if "valve_no_response" in g.faults:
            print(f"[sim] {gh} 阀门故障，忽略指令 {data.get('cmd_id')}")
            return
        action = data.get("action")
        if action == "open":
            g.valve_open = True
            dur = data.get("duration")
            g.valve_close_at = time.time() + float(dur) if dur else None
        elif action == "close":
            g.valve_open, g.valve_close_at = False, None
        ack = {"cmd_id": data.get("cmd_id"), "gh": gh, "ok": True, "ts": time.time(),
               "valve": "open" if g.valve_open else "closed"}
        self.client.publish(C.T_IRR_ACK.format(gh=gh), json.dumps(ack), qos=1)
        self.client.publish(C.T_IRR_STATUS.format(gh=gh), json.dumps(g.irr_payload()), qos=0)

    def handle_scenario(self, data):
        g = self.ghs.get(data.get("gh"))
        if not g:
            return
        name = data.get("name")
        if name == "pest_outbreak":     # 真实虫害爆发：虫口与病害同时上升，无人机图像可见
            g.pest_mult = 7.0
            g.disease = min(0.9, g.disease + 0.35)
        elif name == "reset":
            g.pest_mult = 1.0
            g.disease = Greenhouse.DEFAULT_DISEASE[g.id]
            g.faults.clear()
        elif name == "dry":             # 快速干旱：用于演示自动灌溉联动
            g.moisture = g.cfg["moisture_low"] + 1.5
        print("[sim] 场景", data)

    # ---- 发布循环 ----
    def publish(self, topic, payload, qos=0):
        self.client.publish(topic, json.dumps(payload, ensure_ascii=False), qos=qos)

    def run_drone(self, task):
        gh = task.get("gh")
        g = self.ghs[gh]
        time.sleep(1.5)  # 飞行+拍摄
        with self.lock:
            payload = {"mission_id": task.get("mission_id") or "R-" + uuid.uuid4().hex[:8],
                       "gh": gh, "drone": C.DRONE_ID, "ts": time.time(),
                       "reason": task.get("reason", "routine"), "altitude_m": 12,
                       "battery": round(self.drone_battery, 1)}
            self.drone_battery = self.drone_battery - 1.2 if self.drone_battery > 25 else 100.0
            if "drone_image_missing" in g.faults:
                if random.random() < 0.5:
                    payload["image"] = None
                    payload["error"] = "IMAGE_LOST"       # 图传丢失
                else:
                    payload["image"] = base64.b64encode(os.urandom(3000)).decode()  # 图像损坏
            else:
                jpg = imagegen.drone_image(g.idx, g.disease, g.whitefly_density(),
                                           seed=int(time.time() * 1000), stage=g.cfg["stage"])
                payload["image"] = base64.b64encode(jpg).decode()
        self.publish(C.T_DRONE_IMG.format(gh=gh), payload, qos=1)
        print(f"[sim] 无人机完成 {gh} 巡检 ({payload['reason']})")

    def drone_loop(self):
        last = 0
        while True:
            task = None
            with self.lock:
                if self.drone_queue:
                    task = self.drone_queue.pop(0)
                elif time.time() - last >= C.DRONE_INTERVAL:
                    task = {"gh": C.GH_IDS[self.drone_rr % len(C.GH_IDS)], "reason": "routine"}
                    self.drone_rr += 1
            if task and task.get("gh") in self.ghs:
                self.run_drone(task)
                last = time.time()
            time.sleep(0.5)

    def camera_frame(self, g):
        if g.person_x is None and random.random() < 0.07:
            g.person_x, g.person_v = (-20, random.randint(35, 55)) if random.random() < 0.5 else (500, -random.randint(35, 55))
        elif g.person_x is not None:
            g.person_x += g.person_v
            if g.person_x < -40 or g.person_x > 520:
                g.person_x = None
        hr = sim_hour()
        label = f"{g.cfg['devices']['camera']}  {g.cfg['name']}  模拟时刻 {int(hr):02d}:{int((hr % 1) * 60):02d}"
        jpg = imagegen.camera_frame(g.idx, daylight(), g.person_x, label, seed=int(time.time()))
        return {"device": g.cfg["devices"]["camera"], "gh": g.id, "ts": time.time(),
                "image": base64.b64encode(jpg).decode(), "daylight": round(daylight(), 3)}

    def run(self):
        self.client.reconnect_delay_set(1, 5)
        while True:
            try:
                self.client.connect(C.MQTT_HOST, C.MQTT_PORT, keepalive=30)
                break
            except OSError:
                print("[sim] 等待 MQTT Broker 启动 ...")
                time.sleep(2)
        self.client.loop_start()
        threading.Thread(target=self.drone_loop, daemon=True).start()
        tick, last = 0, time.time()
        while True:
            time.sleep(1)
            now = time.time()
            dt, last = now - last, now
            tick += 1
            with self.lock:
                for g in self.ghs.values():
                    g.step(dt)
                    if tick % C.SOIL_INTERVAL == 0 and "sensor_offline" not in g.faults:
                        self.publish(C.T_SOIL.format(gh=g.id), g.soil_payload())
                    if tick % C.IRR_INTERVAL == 0:
                        self.publish(C.T_IRR_STATUS.format(gh=g.id), g.irr_payload())
                    if tick % C.PEST_INTERVAL == 0:
                        self.publish(C.T_PEST.format(gh=g.id), g.pest_payload())
                    if tick % C.CAMERA_INTERVAL == 0 and "camera_offline" not in g.faults:
                        self.publish(C.T_CAMERA.format(gh=g.id), self.camera_frame(g))
                if tick % 3 == 0:
                    self.publish(C.T_SIM_STATUS, {
                        "ts": now, "sim_hour": round(sim_hour(), 2), "daylight": round(daylight(), 3),
                        "faults": {k: sorted(v.faults) for k, v in self.ghs.items()},
                        "truth": {k: {"disease": round(v.disease, 3), "pest_mult": v.pest_mult}
                                  for k, v in self.ghs.items()}})


if __name__ == "__main__":
    Simulator().run()
