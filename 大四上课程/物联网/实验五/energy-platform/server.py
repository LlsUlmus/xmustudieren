# -*- coding: utf-8 -*-
"""
服务器端：Flask REST 接口 + SSE（Server-Sent Events）实时推送 + 静态可视化网页。

接口一览：
    POST /api/ingest           模拟数据端上报一个周期的数据
    POST /api/ingest/batch     批量上报（历史补录）
    GET  /api/stream           SSE 实时推送看板快照
    GET  /api/snapshot         获取当前看板快照（JSON）
    GET  /api/sim/commands     模拟器拉取故障注入指令
    POST /api/fault            看板下发故障注入指令 {device, type, ticks}
    POST /api/threshold        修改功率告警阈值 {device, threshold}
    POST /api/alarm/ack        告警确认 {id}（不带 id 为全部确认）
    GET  /api/export.csv       导出今日分时能耗统计
"""
import json
import os

from flask import Flask, Response, jsonify, request, send_from_directory

import config as C
from engine import Engine

BASE = os.path.dirname(os.path.abspath(__file__))
app = Flask(__name__, static_folder=os.path.join(BASE, "static"), static_url_path="/static")
engine = Engine(os.path.join(BASE, "data", "energy.db"))


@app.get("/")
def index():
    return send_from_directory(app.static_folder, "index.html")


@app.get("/api/ping")
def ping():
    return jsonify(ok=True)


@app.post("/api/ingest")
def ingest():
    engine.ingest(request.get_json(force=True))
    return jsonify(ok=True)


@app.post("/api/ingest/batch")
def ingest_batch():
    items = request.get_json(force=True).get("items", [])
    engine.ingest_batch(items)
    return jsonify(ok=True, count=len(items))


@app.get("/api/snapshot")
def snapshot():
    return jsonify(engine.snapshot())


@app.get("/api/stream")
def stream():
    def gen():
        last = -1
        while True:
            last = engine.wait(last, timeout=2.0)
            snap = engine.snapshot()
            yield f"data: {json.dumps(snap, ensure_ascii=False)}\n\n"
    return Response(gen(), mimetype="text/event-stream",
                    headers={"Cache-Control": "no-cache", "X-Accel-Buffering": "no"})


@app.get("/api/sim/commands")
def sim_commands():
    return jsonify(commands=engine.pop_commands())


@app.post("/api/fault")
def fault():
    d = request.get_json(force=True)
    if d.get("type") != "clear" and d.get("device") not in C.DEVICE_MAP:
        return jsonify(ok=False, msg="未知设备"), 400
    engine.add_command({"device": d.get("device"), "type": d["type"], "ticks": int(d.get("ticks", 6))})
    return jsonify(ok=True)


@app.post("/api/threshold")
def threshold():
    d = request.get_json(force=True)
    if d.get("device") not in C.DEVICE_MAP:
        return jsonify(ok=False), 400
    engine.set_threshold(d["device"], d["threshold"])
    return jsonify(ok=True)


@app.post("/api/alarm/ack")
def ack():
    d = request.get_json(silent=True) or {}
    engine.ack(d.get("id"))
    return jsonify(ok=True)


@app.get("/api/export.csv")
def export_csv():
    return Response(engine.export_csv(), mimetype="text/csv",
                    headers={"Content-Disposition": "attachment; filename=energy_hourly.csv"})


if __name__ == "__main__":
    print(f"能源监测平台已启动： http://{C.SERVER_HOST}:{C.SERVER_PORT}")
    app.run(host=C.SERVER_HOST, port=C.SERVER_PORT, threaded=True, debug=False)
