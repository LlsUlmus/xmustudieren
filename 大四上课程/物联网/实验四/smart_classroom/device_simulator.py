"""智慧教室设备模拟程序：通过 MQTT 把教室环境数据周期性上传到 OneNET。

用法：python device_simulator.py
"""
import json
import threading
import time
from datetime import datetime

import paho.mqtt.client as mqtt

from classroom_model import ClassroomModel
from onenet import device_token, load_config, topic


def log(name, text):
    print(f"[{datetime.now():%H:%M:%S}] [{name}] {text}", flush=True)


class SimulatedDevice:
    def __init__(self, cfg, device):
        self.cfg = cfg
        self.name = device["name"]
        self.model = ClassroomModel()
        self.lock = threading.Lock()
        self.msg_id = 0

        self.client = mqtt.Client(mqtt.CallbackAPIVersion.VERSION2, client_id=self.name)
        self.client.username_pw_set(cfg["product_id"], device_token(cfg, device))
        if cfg.get("mqtt_ca_cert"):
            self.client.tls_set(ca_certs=cfg["mqtt_ca_cert"])
        self.client.on_connect = self.on_connect
        self.client.on_disconnect = self.on_disconnect
        self.client.on_message = self.on_message

    def on_connect(self, client, userdata, flags, reason_code, properties):
        if reason_code.is_failure:
            log(self.name, f"连接失败：{reason_code}（请检查产品ID、设备名称、设备密钥）")
            return
        log(self.name, f"已连接 OneNET {self.cfg['mqtt_host']}:{self.cfg['mqtt_port']}")
        client.subscribe(topic(self.cfg, self.name, "post/reply"))
        client.subscribe(topic(self.cfg, self.name, "set"))

    def on_disconnect(self, client, userdata, flags, reason_code, properties):
        log(self.name, f"连接断开：{reason_code}，将自动重连")

    def on_message(self, client, userdata, msg):
        data = json.loads(msg.payload)
        if msg.topic.endswith("post/reply"):
            if data.get("code") == 200:
                log(self.name, f"  ↳ 云端确认 id={data.get('id')} 上报成功")
            else:
                log(self.name, f"  ↳ 云端拒绝 id={data.get('id')}：{data}")
            return

        # 云端下发属性设置（远程控制空调 / 灯光 / 新风）
        params = data.get("params", {})
        with self.lock:
            ok = all([self.model.set(k, v) for k, v in params.items()])
        log(self.name, f"收到云端控制命令 {params}，{'已执行' if ok else '含不支持的属性'}")
        reply = {"id": data.get("id"), "code": 200 if ok else 400, "msg": "success" if ok else "fail"}
        client.publish(topic(self.cfg, self.name, "set_reply"), json.dumps(reply), qos=1)
        self.report()

    def report(self):
        with self.lock:
            props = self.model.properties()
            self.msg_id += 1
            msg_id = str(self.msg_id)
        payload = {
            "id": msg_id,
            "version": "1.0",
            "params": {k: {"value": v} for k, v in props.items()},
        }
        self.client.publish(topic(self.cfg, self.name, "post"), json.dumps(payload), qos=1)
        log(
            self.name,
            f"上报 id={msg_id}  温度 {props['temperature']}℃  湿度 {props['humidity']}%  "
            f"CO2 {props['co2']}ppm  PM2.5 {props['pm25']}  光照 {props['lux']}lx  "
            f"有人={props['occupied']}  空调={props['ac']}  灯={props['light']}  新风={props['fan']}",
        )

    def run(self):
        self.client.connect(self.cfg["mqtt_host"], self.cfg["mqtt_port"], keepalive=60)
        self.client.loop_start()
        while True:
            time.sleep(self.cfg.get("report_interval", 5))
            if not self.client.is_connected():
                continue
            with self.lock:
                events = self.model.step()
            for e in events:
                log(self.name, f"★ {e}")
            self.report()


def main():
    cfg = load_config()
    for device in cfg["devices"]:
        threading.Thread(target=SimulatedDevice(cfg, device).run, daemon=True).start()
    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        print("模拟设备已停止")


if __name__ == "__main__":
    main()
