"""可视化监控后端：定时从 OneNET 查询设备最新属性，提供给网页大屏。

用法：
    python server.py            从 OneNET 云平台取数（正式演示用）
    python server.py --offline  不连云平台，本地生成数据，仅用于预览界面
浏览器打开 http://127.0.0.1:5000
"""
import sys
import threading
import time
from collections import deque

import requests
from flask import Flask, jsonify, request, send_from_directory

from classroom_model import CONTROLLABLE, ClassroomModel
from onenet import api_token, load_config

OFFLINE = "--offline" in sys.argv
POLL_INTERVAL = 2
HISTORY_LEN = 120

CASTS = {
    "temperature": float, "humidity": float,
    "co2": int, "pm25": int, "lux": int,
    "occupied": "bool", "ac": "bool", "light": "bool", "fan": "bool",
}

cfg = load_config()
app = Flask(__name__, static_folder="static")
lock = threading.Lock()
http = requests.Session()
http.trust_env = False  # 不走系统代理，直连 OneNET
state = {
    d["name"]: {"online": False, "props": {}, "ts": 0, "error": "", "history": deque(maxlen=HISTORY_LEN)}
    for d in cfg["devices"]
}
offline_models = {name: ClassroomModel() for name in state}


def cast(key, value):
    kind = CASTS[key]
    if kind == "bool":
        return str(value).lower() in ("true", "1")
    return kind(float(value))


def onenet_get(path, device):
    r = http.get(
        cfg["api_base"] + path,
        params={"product_id": cfg["product_id"], "device_name": device},
        headers={"authorization": api_token(cfg, device)},
        timeout=5,
    )
    body = r.json()
    if body.get("code") != 0:
        raise RuntimeError(f"OneNET 返回错误 code={body.get('code')} msg={body.get('msg')}")
    return body["data"]


def fetch_cloud(device):
    """查询设备最新属性和在线状态，返回 (online, props, ts毫秒)。"""
    items = onenet_get("/thingmodel/query-device-property", device)
    props, ts = {}, 0
    for item in items:
        key = item.get("identifier")
        if key in CASTS and item.get("value") not in (None, ""):
            props[key] = cast(key, item["value"])
            ts = max(ts, int(item.get("time") or 0))
    online = onenet_get("/device/detail", device).get("status") == 1
    return online, props, ts


def fetch_offline(device):
    model = offline_models[device]
    ts = int(time.time() // cfg["report_interval"]) * cfg["report_interval"] * 1000
    if ts != state[device]["ts"]:
        model.step()
    return True, model.properties(), ts


def poll_loop():
    fetch = fetch_offline if OFFLINE else fetch_cloud
    while True:
        for device, s in state.items():
            try:
                online, props, ts = fetch(device)
            except Exception as e:
                with lock:
                    s["error"] = str(e)
                continue
            with lock:
                s["online"], s["error"] = online, ""
                if props and ts != s["ts"]:
                    s["props"], s["ts"] = props, ts
                    s["history"].append({"ts": ts, **props})
        time.sleep(POLL_INTERVAL)


@app.get("/")
def index():
    return send_from_directory(app.static_folder, "index.html")


@app.get("/api/devices")
def devices():
    return jsonify({"devices": list(state), "offline": OFFLINE, "interval": cfg["report_interval"]})


@app.get("/api/state")
def get_state():
    s = state.get(request.args.get("device", ""))
    if s is None:
        return jsonify({"error": "未知设备"}), 404
    with lock:
        return jsonify({**s, "history": list(s["history"])})


@app.post("/api/control")
def control():
    body = request.get_json()
    device, key, value = body.get("device"), body.get("key"), bool(body.get("value"))
    if device not in state or key not in CONTROLLABLE:
        return jsonify({"ok": False, "msg": "参数错误"}), 400
    if OFFLINE:
        offline_models[device].set(key, value)
        return jsonify({"ok": True})
    try:
        r = http.post(
            cfg["api_base"] + "/thingmodel/set-device-property",
            json={"product_id": cfg["product_id"], "device_name": device, "params": {key: value}},
            headers={"authorization": api_token(cfg, device)},
            timeout=8,
        )
        result = r.json()
    except Exception as e:
        return jsonify({"ok": False, "msg": str(e)}), 502
    ok = result.get("code") == 0
    return jsonify({"ok": ok, "msg": result.get("msg", "")}), (200 if ok else 502)


if __name__ == "__main__":
    threading.Thread(target=poll_loop, daemon=True).start()
    print("数据来源：" + ("本地离线预览（未连接云平台）" if OFFLINE else "OneNET 云平台"))
    app.run(host="127.0.0.1", port=5000)
