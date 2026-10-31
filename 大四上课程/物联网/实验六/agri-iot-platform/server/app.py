"""服务器端：内置 MQTT Broker + MQTT 数据接入 + RESTful API + WebSocket 实时推送 + 网页托管。

运行：python -m server.app
"""
import asyncio
import json
import logging
import os
import sys
import uuid
from contextlib import asynccontextmanager

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import paho.mqtt.client as mqtt  # noqa: E402
from fastapi import Body, FastAPI, HTTPException, WebSocket, WebSocketDisconnect  # noqa: E402
from fastapi.responses import FileResponse, Response  # noqa: E402
from fastapi.staticfiles import StaticFiles  # noqa: E402

from common import config as C  # noqa: E402
from server.core import Platform  # noqa: E402

logging.basicConfig(level=logging.WARNING)
log = logging.getLogger("agri")
log.setLevel(logging.INFO)

WEB_DIR = os.path.join(C.BASE_DIR, "web")
EMBED_BROKER = os.environ.get("AGRI_EMBED_BROKER", "1") == "1"

platform = Platform()
clients: set[WebSocket] = set()


def start_mqtt_client():
    cli = mqtt.Client(mqtt.CallbackAPIVersion.VERSION2, client_id="agri-server-" + uuid.uuid4().hex[:6])

    def on_connect(c, u, flags, rc, props=None):
        log.info("服务器 MQTT 客户端已连接 rc=%s", rc)
        c.subscribe([("agri/+/soil", 0), ("agri/+/irrigation/status", 0), ("agri/+/irrigation/ack", 1),
                     ("agri/+/pest", 0), ("agri/+/drone/image", 1), ("agri/+/camera/frame", 0),
                     (C.T_SIM_STATUS, 0)])

    def on_message(c, u, msg):
        try:
            platform.on_message(msg.topic, msg.payload)
        except Exception:  # noqa: BLE001
            log.exception("处理消息失败 %s", msg.topic)

    cli.on_connect = on_connect
    cli.on_message = on_message
    cli.reconnect_delay_set(1, 5)
    cli.connect_async(C.MQTT_HOST, C.MQTT_PORT, keepalive=30)
    cli.loop_start()
    platform.mqtt = lambda topic, payload: cli.publish(topic, payload, qos=1)
    return cli


async def ticker():
    while True:
        await asyncio.sleep(1)
        try:
            platform.tick()
        except Exception:  # noqa: BLE001
            log.exception("tick 失败")


async def broadcaster():
    while True:
        await asyncio.sleep(1)
        if not clients:
            platform.events.clear()
            continue
        snap = await asyncio.to_thread(platform.snapshot)
        text = json.dumps({"type": "snapshot", "data": snap}, ensure_ascii=False)
        for ws in list(clients):
            try:
                await ws.send_text(text)
            except Exception:  # noqa: BLE001
                clients.discard(ws)


@asynccontextmanager
async def lifespan(app):
    broker = None
    if EMBED_BROKER:
        from amqtt.broker import Broker
        broker = Broker({"listeners": {"default": {"type": "tcp", "bind": f"0.0.0.0:{C.MQTT_PORT}"}}})
        await broker.start()
        log.info("内置 MQTT Broker 已启动 tcp://0.0.0.0:%s", C.MQTT_PORT)
    cli = start_mqtt_client()
    tasks = [asyncio.create_task(ticker()), asyncio.create_task(broadcaster())]
    log.info("平台已启动：http://127.0.0.1:%s", C.HTTP_PORT)
    yield
    for t in tasks:
        t.cancel()
    cli.loop_stop()
    if broker:
        await broker.shutdown()


app = FastAPI(title="智慧番茄温室物联网监测平台", lifespan=lifespan)


# ---------------- RESTful API ----------------
@app.get("/api/greenhouses")
def greenhouses():
    return C.GREENHOUSES


@app.get("/api/snapshot")
def snapshot():
    return platform.snapshot()


@app.get("/api/history/soil")
def soil_history(gh: str, limit: int = 300):
    rows = platform.store.query("SELECT ts, valid, data FROM soil WHERE gh=? ORDER BY ts DESC LIMIT ?", (gh, limit))
    return [{**json.loads(r["data"]), "valid": bool(r["valid"])} for r in rows][::-1]


@app.get("/api/history/pest")
def pest_history(gh: str, limit: int = 300):
    return platform.store.query("SELECT * FROM pest WHERE gh=? ORDER BY ts DESC LIMIT ?", (gh, limit))[::-1]


@app.get("/api/history/irrigation")
def irrigation_history(gh: str, limit: int = 300):
    return platform.store.query("SELECT * FROM irrigation WHERE gh=? ORDER BY ts DESC LIMIT ?", (gh, limit))[::-1]


@app.get("/api/drone/missions")
def drone_missions(gh: str | None = None, limit: int = 50):
    if gh:
        return platform.store.query("SELECT * FROM drone WHERE gh=? ORDER BY ts DESC LIMIT ?", (gh, limit))
    return platform.store.query("SELECT * FROM drone ORDER BY ts DESC LIMIT ?", (limit,))


@app.get("/api/alarms")
def alarms(status: str | None = None, limit: int = 100):
    if status:
        return platform.store.query("SELECT * FROM alarm WHERE status=? ORDER BY start_ts DESC LIMIT ?", (status, limit))
    return platform.store.query("SELECT * FROM alarm ORDER BY start_ts DESC LIMIT ?", (limit,))


@app.get("/api/commands")
def commands(limit: int = 100):
    return platform.store.query("SELECT * FROM command ORDER BY sent_ts DESC LIMIT ?", (limit,))


@app.get("/api/linkage")
def linkage(limit: int = 100):
    return platform.store.query("SELECT * FROM linkage ORDER BY ts DESC LIMIT ?", (limit,))


def _gh(gh):
    if gh not in platform.gh:
        raise HTTPException(404, "未知大棚")
    return platform.gh[gh]


@app.post("/api/irrigation/{gh}/command")
def irrigation_cmd(gh: str, body: dict = Body(...)):
    g = _gh(gh)
    action = body.get("action")
    if action not in ("open", "close"):
        raise HTTPException(400, "action 应为 open/close")
    with platform.lock:
        g.auto = False   # 手动控制后切换为手动模式
        cmd = platform.send_irr_cmd(gh, action, "manual", body.get("duration"))
        platform.notify("info", gh, f"{gh} 手动{'开阀' if action == 'open' else '关阀'}指令已下发（自动模式已关闭）")
    return cmd


@app.post("/api/irrigation/{gh}/mode")
def irrigation_mode(gh: str, body: dict = Body(...)):
    g = _gh(gh)
    with platform.lock:
        if "auto" in body:
            g.auto = bool(body["auto"])
        if "low" in body:
            g.low = float(body["low"])
        if "high" in body:
            g.high = float(body["high"])
        if g.low >= g.high:
            raise HTTPException(400, "下限必须小于上限")
        platform.notify("info", gh, f"{gh} 灌溉模式：{'自动' if g.auto else '手动'}，阈值 {g.low}%~{g.high}%")
    return {"auto": g.auto, "low": g.low, "high": g.high}


@app.post("/api/drone/{gh}/mission")
def drone_mission(gh: str):
    _gh(gh)
    with platform.lock:
        mid = platform.request_drone(gh, "manual")
        platform.notify("info", gh, f"已派发无人机巡检任务 {mid} → {gh}")
    return {"mission_id": mid}


@app.get("/api/camera/{gh}/latest.jpg")
def camera_latest(gh: str):
    g = _gh(gh)
    if not g.cam_jpg:
        raise HTTPException(404, "暂无画面")
    return Response(g.cam_jpg, media_type="image/jpeg", headers={"Cache-Control": "no-store"})


@app.post("/api/camera/{gh}/snapshot")
def camera_snapshot(gh: str):
    _gh(gh)
    with platform.lock:
        snap = platform.manual_snapshot(gh)
    if not snap:
        raise HTTPException(409, "摄像头离线，无法抓拍")
    return snap


@app.post("/api/fault")
def fault(body: dict = Body(...)):
    gh, ft, on = body.get("gh"), body.get("fault"), bool(body.get("on"))
    _gh(gh)
    if ft not in C.FAULT_TYPES:
        raise HTTPException(400, "未知故障类型")
    platform.publish(C.T_SIM_FAULT, {"gh": gh, "fault": ft, "on": on})
    with platform.lock:
        platform.notify("warning" if on else "ok", gh,
                        f"[故障模拟] {gh} {'注入' if on else '解除'}：{C.FAULT_TYPES[ft]}")
    return {"ok": True}


@app.post("/api/scenario")
def scenario(body: dict = Body(...)):
    gh, name = body.get("gh"), body.get("name")
    _gh(gh)
    platform.publish(C.T_SIM_SCENARIO, {"gh": gh, "name": name})
    names = {"pest_outbreak": "真实虫害爆发", "reset": "恢复正常", "dry": "快速干旱"}
    with platform.lock:
        platform.notify("info", gh, f"[场景模拟] {gh} {names.get(name, name)}")
    return {"ok": True}


@app.post("/api/alarms/{aid}/ack")
def alarm_ack(aid: str):
    with platform.lock:
        for a in platform.alarm_hist:
            if a["id"] == aid:
                a["acked"] = True
                return a
    raise HTTPException(404)


@app.websocket("/ws")
async def ws_endpoint(ws: WebSocket):
    await ws.accept()
    clients.add(ws)
    try:
        snap = await asyncio.to_thread(platform.snapshot)
        await ws.send_text(json.dumps({"type": "snapshot", "data": snap}, ensure_ascii=False))
        while True:
            await ws.receive_text()   # 保持连接（前端心跳）
    except WebSocketDisconnect:
        pass
    finally:
        clients.discard(ws)


os.makedirs(C.IMG_DIR, exist_ok=True)
app.mount("/images", StaticFiles(directory=C.IMG_DIR), name="images")
app.mount("/static", StaticFiles(directory=WEB_DIR), name="static")


@app.get("/")
def index():
    return FileResponse(os.path.join(WEB_DIR, "index.html"))


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host=C.HTTP_HOST, port=C.HTTP_PORT, log_level="warning")
