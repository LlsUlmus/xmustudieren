"""
智能交互中枢（Hub）

- HTTP：提供网页界面静态文件与 REST API
    GET  /api/state          当前环境 + 设备状态
    GET  /api/rules          自动化规则列表
    GET  /api/events         最近事件日志
    POST /api/command        {"text": "打开灯"} 或 {"device": "light", "params": {...}}
- WebSocket：
    /ws/device  设备网关接入（上报遥测、接收命令）
    /ws/ui      浏览器界面接入（发送用户输入、接收状态与事件推送）
- 交互决策：nlu.parse（文字/语音指令）+ RuleEngine（环境自动化规则）

运行：python hub/server.py [--port 8080]
"""
import argparse
import asyncio
import itertools
import json
import os
import sys
import time
from collections import deque

from aiohttp import web, WSMsgType

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import nlu  # noqa: E402
from rules import RuleEngine  # noqa: E402

WEB_DIR = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "web")

# 个性化偏好（模拟身份识别后读取的用户画像）
PROFILES = {
    "访客": {"color_temp": 4000, "brightness": 70, "ac_target": 26, "music": "轻音乐"},
    "爸爸": {"color_temp": 4500, "brightness": 85, "ac_target": 25, "music": "古典音乐"},
    "妈妈": {"color_temp": 3000, "brightness": 65, "ac_target": 27, "music": "轻音乐"},
    "小明": {"color_temp": 6000, "brightness": 95, "ac_target": 24, "music": "流行歌曲"},
}


class Hub:
    def __init__(self):
        self.state = {"env": {}, "devices": {}, "online": False, "auto": True, "user": "访客"}
        self.device_ws = None
        self.ui_clients = set()
        self.events = deque(maxlen=200)
        self.history = deque(maxlen=180)
        self.rules = RuleEngine()
        self.cmd_ids = itertools.count(1)
        self.pending = {}
        self.last_person = ""

    @property
    def profile(self):
        return PROFILES.get(self.state["user"], PROFILES["访客"])

    # ---------- 推送 ----------
    async def broadcast(self, msg):
        data = json.dumps(msg, ensure_ascii=False)
        for ws in list(self.ui_clients):
            try:
                await ws.send_str(data)
            except (ConnectionResetError, RuntimeError):
                self.ui_clients.discard(ws)

    async def log(self, stage, text, **extra):
        ev = {"ts": time.time(), "sim_time": self.state["env"].get("time", "--:--"),
              "stage": stage, "text": text, **extra}
        self.events.append(ev)
        print(f"[hub][{stage}] {text}")
        await self.broadcast({"type": "event", "event": ev})

    # ---------- 命令下发 ----------
    async def send_command(self, device, params, source):
        cmd_id = next(self.cmd_ids)
        msg = {"type": "command", "cmd_id": cmd_id, "topic": f"home/livingroom/{device}/set",
               "device": device, "params": params, "source": source}
        if self.device_ws is None or self.device_ws.closed:
            await self.log("响应", f"设备网关离线，命令 {device} {params} 未送达", level="error")
            return False
        self.pending[cmd_id] = msg
        await self.device_ws.send_json(msg)
        await self.log("响应", f"下发 {msg['topic']} {json.dumps(params, ensure_ascii=False)}",
                       source=source, device=device)
        return True

    async def execute(self, commands, source, manual):
        for c in commands:
            if manual:
                self.rules.note_manual(c["device"])
            await self.send_command(c["device"], c["params"], source)

    # ---------- 设备遥测 ----------
    async def on_telemetry(self, payload):
        prev_env = self.state["env"]
        self.state["env"] = payload["env"]
        self.state["devices"] = payload["devices"]
        self.state["curtain_target"] = payload.get("curtain_target")
        env = payload["env"]
        self.history.append({"t": env["time"], "temperature": env["temperature"],
                             "humidity": env["humidity"], "illuminance": env["illuminance"]})
        # 人员进出事件（感知）
        if env.get("presence") != prev_env.get("presence") and prev_env:
            if env.get("presence"):
                person = env.get("person") or "访客"
                if person in PROFILES:
                    self.state["user"] = person
                await self.log("感知", f"人体传感器：检测到「{person}」进入客厅（照度 {env['illuminance']:.0f} lux）")
                await self.broadcast({"type": "greet", "text": f"欢迎回家，{person}！",
                                      "profile": self.profile, "user": self.state["user"]})
            else:
                await self.log("感知", "人体传感器：客厅已无人")
        # 规则引擎（判断）
        for rule, cmds in self.rules.evaluate(self.state, self.profile):
            await self.log("判断", f"规则 {rule.rule_id}「{rule.name}」触发：{rule.description}", rule=rule.rule_id)
            await self.execute(cmds, source=f"rule:{rule.rule_id}", manual=False)
        await self.broadcast({"type": "state", "state": self.state,
                              "point": self.history[-1]})

    # ---------- 用户输入 ----------
    async def on_text(self, text, source="text"):
        result = nlu.parse(text, self.state, self.profile, self.state["user"])
        await self.log("感知", f"用户{'语音' if source == 'voice' else '文字'}输入：「{text}」")
        await self.log("判断", f"意图={result.intent}；" + " → ".join(result.trace[1:]) if len(result.trace) > 1
                       else f"意图={result.intent}")
        await self.broadcast({"type": "dialog", "user_text": text, "source": source, **result.to_dict()})
        await self.execute(result.commands, source=f"user:{source}", manual=True)
        return result


hub = Hub()


async def ws_device(request):
    ws = web.WebSocketResponse(heartbeat=10)
    await ws.prepare(request)
    hub.device_ws = ws
    hub.state["online"] = True
    try:
        async for msg in ws:
            if msg.type != WSMsgType.TEXT:
                continue
            data = json.loads(msg.data)
            t = data.get("type")
            if t == "register":
                await hub.log("感知", f"设备网关 {data['device_id']} 上线，设备：{', '.join(data['devices'])}")
            elif t == "telemetry":
                await hub.on_telemetry(data["payload"])
            elif t == "ack":
                cmd = hub.pending.pop(data.get("cmd_id"), None)
                ok = "成功" if data.get("ok") else f"失败：{data.get('error')}"
                await hub.log("响应", f"设备回执 #{data.get('cmd_id')} {data.get('device')} 执行{ok}",
                              device=data.get("device"))
                await hub.broadcast({"type": "ack", "cmd": cmd, "ok": data.get("ok")})
    finally:
        if hub.device_ws is ws:
            hub.device_ws = None
            hub.state["online"] = False
            await hub.log("感知", "设备网关断开连接", level="error")
            await hub.broadcast({"type": "state", "state": hub.state, "point": None})
    return ws


async def ws_ui(request):
    ws = web.WebSocketResponse(heartbeat=10)
    await ws.prepare(request)
    hub.ui_clients.add(ws)
    await ws.send_json({"type": "init", "state": hub.state, "history": list(hub.history),
                        "events": list(hub.events)[-50:], "rules": hub.rules.describe(),
                        "profiles": PROFILES})
    try:
        async for msg in ws:
            if msg.type != WSMsgType.TEXT:
                continue
            data = json.loads(msg.data)
            t = data.get("type")
            if t == "text":
                await hub.on_text(data.get("text", ""), data.get("source", "text"))
            elif t == "control":  # 界面按钮/滑块
                await hub.log("感知", f"用户界面操作：{nlu.DEVICE_NAMES.get(data['device'], data['device'])} "
                                     f"{json.dumps(data['params'], ensure_ascii=False)}")
                await hub.execute([{"device": data["device"], "params": data["params"]}], "user:button", manual=True)
            elif t == "sim":  # 仿真控制（人员进出、时间、室外温度）
                if hub.device_ws is not None:
                    await hub.device_ws.send_json(data)
                await hub.log("感知", f"仿真事件：{data.get('action')} = {json.dumps(data.get('value'), ensure_ascii=False)}",
                              level="sim")
            elif t == "auto":
                hub.rules.enabled = hub.state["auto"] = bool(data.get("enabled"))
                await hub.log("判断", f"自动化规则已{'启用' if hub.state['auto'] else '停用'}")
                await hub.broadcast({"type": "state", "state": hub.state, "point": None})
            elif t == "user":
                if data.get("user") in PROFILES:
                    hub.state["user"] = data["user"]
                    await hub.log("感知", f"当前用户切换为「{data['user']}」")
                    await hub.broadcast({"type": "state", "state": hub.state, "point": None})
    finally:
        hub.ui_clients.discard(ws)
    return ws


async def api_state(request):
    return web.json_response(hub.state)


async def api_rules(request):
    return web.json_response(hub.rules.describe())


async def api_events(request):
    return web.json_response(list(hub.events)[-100:])


async def api_command(request):
    body = await request.json()
    if "text" in body:
        r = await hub.on_text(body["text"], body.get("source", "http"))
        return web.json_response(r.to_dict())
    if "device" in body:
        ok = await hub.send_command(body["device"], body.get("params", {}), "user:http")
        hub.rules.note_manual(body["device"])
        return web.json_response({"ok": ok})
    return web.json_response({"error": "需要 text 或 device 字段"}, status=400)


async def index(request):
    return web.FileResponse(os.path.join(WEB_DIR, "index.html"))


def make_app():
    app = web.Application()
    app.router.add_get("/", index)
    app.router.add_get("/ws/device", ws_device)
    app.router.add_get("/ws/ui", ws_ui)
    app.router.add_get("/api/state", api_state)
    app.router.add_get("/api/rules", api_rules)
    app.router.add_get("/api/events", api_events)
    app.router.add_post("/api/command", api_command)
    app.router.add_static("/static/", WEB_DIR)
    return app


if __name__ == "__main__":
    ap = argparse.ArgumentParser(description="智能交互中枢")
    ap.add_argument("--host", default="127.0.0.1")
    ap.add_argument("--port", type=int, default=8080)
    args = ap.parse_args()
    print(f"[hub] 界面地址：http://{args.host}:{args.port}/")
    web.run_app(make_app(), host=args.host, port=args.port, print=None)
