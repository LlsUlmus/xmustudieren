"""
物联网设备模拟器（模拟“客厅网关”及其下挂的传感器与执行器）

- 传感器：温度、湿度、光照、人体存在(PIR)、室外温度、模拟时钟
- 执行器：灯（开关/亮度/色温）、风扇（档位）、窗帘（开合度）、空调（模式/目标温度）、音箱
- 通信：作为 WebSocket 客户端连接中枢 ws://<hub>/ws/device，
        每秒上报一次遥测数据(topic: home/livingroom/telemetry)，
        接收命令(topic: home/livingroom/<device>/set)并回执(ack)
- 环境模型：室温受室外温度、空调影响；光照受日照、窗帘、灯光影响；
        1 秒真实时间 = 1 分钟模拟时间（可在界面上调整时钟）

运行：python devices/simulator.py [--hub ws://127.0.0.1:8080/ws/device]
"""
import argparse
import asyncio
import json
import math
import random
import time

import aiohttp

DEVICE_ID = "livingroom-gateway-01"
TRACKS = ["轻音乐", "流行歌曲", "古典音乐", "爵士乐", "白噪音"]


class LivingRoom:
    def __init__(self, seed=None):
        self.rng = random.Random(seed)
        self.minute = 20 * 60 + 0  # 模拟时钟，从 20:00 开始（便于演示夜间场景）
        self.speed = 1.0           # 每个 tick 推进的模拟分钟数
        self.outdoor_offset = 0.0  # 界面可调的室外温度偏移（模拟热浪/降温）
        self.temperature = 29.0
        self.humidity = 62.0
        self.presence = False
        self.person = ""
        self.vacant_minutes = 999.0
        self.curtain_target = 100
        self.devices = {
            "light": {"on": False, "brightness": 60, "color_temp": 4000},
            "fan": {"on": False, "speed": 1},
            "curtain": {"position": 100},
            "ac": {"on": False, "mode": "cool", "target": 26},
            "speaker": {"on": False, "track": "轻音乐", "volume": 40},
        }

    # ---------- 环境物理模型 ----------
    @property
    def hour(self):
        return int(self.minute // 60) % 24

    def outdoor_temp(self):
        h = (self.minute / 60.0) % 24
        # 夏季昼夜温度曲线：14 点最高约 35℃，凌晨约 25℃
        return 30 + 5 * math.sin((h - 8) / 24 * 2 * math.pi) + self.outdoor_offset

    def daylight(self):
        h = (self.minute / 60.0) % 24
        if h < 6 or h > 19:
            return 0.0
        return max(0.0, math.sin((h - 6) / 13 * math.pi)) * 900

    def illuminance(self):
        pos = self.devices["curtain"]["position"] / 100
        natural = self.daylight() * (0.04 + 0.96 * pos)
        lamp = self.devices["light"]["brightness"] * 4.5 if self.devices["light"]["on"] else 0
        return max(0.0, natural + lamp + self.rng.uniform(-3, 3))

    def step(self):
        dt = self.speed
        self.minute = (self.minute + dt) % (24 * 60)
        out = self.outdoor_temp()
        # 室内外热交换 + 有人时的人体/电器发热
        self.temperature += 0.02 * dt * (out - self.temperature) + (0.01 * dt if self.presence else 0)
        ac = self.devices["ac"]
        if ac["on"]:
            if ac["mode"] == "cool" and self.temperature > ac["target"]:
                self.temperature -= min(0.12 * dt, self.temperature - ac["target"])
            elif ac["mode"] == "heat" and self.temperature < ac["target"]:
                self.temperature += min(0.12 * dt, ac["target"] - self.temperature)
            elif ac["mode"] == "dry":
                self.humidity -= 0.6 * dt
                if self.temperature > ac["target"]:
                    self.temperature -= 0.04 * dt
        # 湿度向 65% 回归，空调制冷会顺带除湿
        self.humidity += 0.02 * dt * (65 - self.humidity) - (0.15 * dt if ac["on"] and ac["mode"] == "cool" else 0)
        self.humidity = min(95, max(30, self.humidity + self.rng.uniform(-0.2, 0.2)))
        self.temperature += self.rng.uniform(-0.03, 0.03)
        # 窗帘电机：每 tick 移动 20%
        pos = self.devices["curtain"]["position"]
        if pos != self.curtain_target:
            delta = max(-20, min(20, self.curtain_target - pos))
            self.devices["curtain"]["position"] = pos + delta
        self.vacant_minutes = 0 if self.presence else self.vacant_minutes + dt

    def telemetry(self):
        return {
            "device_id": DEVICE_ID,
            "env": {
                "time": f"{self.hour:02d}:{int(self.minute % 60):02d}",
                "hour": self.hour,
                "temperature": round(self.temperature, 2),
                "humidity": round(self.humidity, 1),
                "illuminance": round(self.illuminance(), 1),
                "presence": self.presence,
                "person": self.person if self.presence else "",
                "vacant_minutes": round(min(self.vacant_minutes, 999), 1),
                "outdoor_temp": round(self.outdoor_temp(), 1),
            },
            "devices": json.loads(json.dumps(self.devices)),
            "curtain_target": self.curtain_target,
        }

    # ---------- 执行器 ----------
    def apply(self, device, params):
        if device not in self.devices:
            raise ValueError(f"未知设备 {device}")
        d = self.devices[device]
        clamp = lambda v, lo, hi: max(lo, min(hi, int(v)))
        for k, v in params.items():
            if k == "on":
                d["on"] = bool(v)
            elif k == "brightness":
                d["brightness"] = clamp(v, 1, 100)
            elif k == "color_temp":
                d["color_temp"] = clamp(v, 2700, 6500)
            elif k == "speed":
                d["speed"] = clamp(v, 1, 3)
            elif k == "position":
                self.curtain_target = clamp(v, 0, 100)
            elif k == "target":
                d["target"] = clamp(v, 16, 30)
            elif k == "mode" and v in ("cool", "heat", "dry"):
                d["mode"] = v
            elif k == "volume":
                d["volume"] = clamp(v, 0, 100)
            elif k == "track":
                d["track"] = str(v)[:12]
            elif k == "next":
                i = TRACKS.index(d["track"]) if d["track"] in TRACKS else -1
                d["track"] = TRACKS[(i + 1) % len(TRACKS)]
        return d

    def sim(self, action, value):
        """仿真控制：人员进出、时间跳转、室外温度变化"""
        if action == "presence":
            self.presence = bool(value.get("present"))
            self.person = value.get("person", "") if self.presence else ""
        elif action == "time":
            self.minute = int(value) % (24 * 60)
        elif action == "speed":
            self.speed = max(0.5, min(10, float(value)))
        elif action == "outdoor":
            self.outdoor_offset = max(-15, min(15, self.outdoor_offset + float(value)))
            self.temperature += float(value) * 0.5


async def run(hub_url, interval):
    room = LivingRoom()
    while True:
        try:
            async with aiohttp.ClientSession() as session:
                async with session.ws_connect(hub_url, heartbeat=10) as ws:
                    print(f"[device] 已连接中枢 {hub_url}")
                    await ws.send_json({"type": "register", "device_id": DEVICE_ID,
                                        "devices": list(room.devices.keys()),
                                        "sensors": ["temperature", "humidity", "illuminance", "presence"]})

                    async def publish_loop():
                        while True:
                            room.step()
                            await ws.send_json({"type": "telemetry", "topic": "home/livingroom/telemetry",
                                                "ts": time.time(), "payload": room.telemetry()})
                            await asyncio.sleep(interval)

                    pub = asyncio.create_task(publish_loop())
                    try:
                        async for msg in ws:
                            if msg.type != aiohttp.WSMsgType.TEXT:
                                continue
                            data = json.loads(msg.data)
                            if data.get("type") == "command":
                                try:
                                    st = room.apply(data["device"], data.get("params", {}))
                                    ack = {"ok": True, "state": st}
                                    print(f"[device] 执行 {data['topic']} {data.get('params')}")
                                except Exception as e:  # noqa
                                    ack = {"ok": False, "error": str(e)}
                                await ws.send_json({"type": "ack", "cmd_id": data.get("cmd_id"),
                                                    "device": data.get("device"), **ack})
                                # 命令执行后立刻上报一次，界面响应更及时
                                await ws.send_json({"type": "telemetry", "topic": "home/livingroom/telemetry",
                                                    "ts": time.time(), "payload": room.telemetry()})
                            elif data.get("type") == "sim":
                                room.sim(data.get("action"), data.get("value"))
                                print(f"[device] 仿真事件 {data.get('action')} = {data.get('value')}")
                    finally:
                        pub.cancel()
        except (aiohttp.ClientError, ConnectionError, OSError) as e:
            print(f"[device] 连接中枢失败：{e}，3 秒后重连…")
        await asyncio.sleep(3)


if __name__ == "__main__":
    ap = argparse.ArgumentParser(description="客厅物联网设备模拟器")
    ap.add_argument("--hub", default="ws://127.0.0.1:8080/ws/device")
    ap.add_argument("--interval", type=float, default=1.0, help="遥测上报间隔(秒)")
    args = ap.parse_args()
    try:
        asyncio.run(run(args.hub, args.interval))
    except KeyboardInterrupt:
        pass
