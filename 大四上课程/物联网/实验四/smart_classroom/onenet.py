"""OneNET 公共部分：读取配置、生成鉴权 token、拼接 Topic。"""
import base64
import hmac
import json
import os
import time
from urllib.parse import quote

CONFIG_PATH = os.path.join(os.path.dirname(os.path.abspath(__file__)), "config.json")


def load_config():
    with open(CONFIG_PATH, encoding="utf-8") as f:
        return json.load(f)


def make_token(res, key_b64, version="2018-10-31", method="sha1", ttl=365 * 24 * 3600):
    """OneNET 安全鉴权 token。
    签名串 = et + '\\n' + method + '\\n' + res + '\\n' + version，密钥为 base64 解码后的 key。"""
    et = int(time.time()) + ttl
    key = base64.b64decode(key_b64)
    string_to_sign = f"{et}\n{method}\n{res}\n{version}"
    sign = base64.b64encode(hmac.new(key, string_to_sign.encode(), method).digest()).decode()
    return (
        f"version={version}&res={quote(res, safe='')}&et={et}"
        f"&method={method}&sign={quote(sign, safe='')}"
    )


def device_token(cfg, device):
    """设备级 token：作为 MQTT 连接密码。"""
    res = f"products/{cfg['product_id']}/devices/{device['name']}"
    return make_token(res, device["key"])


def api_token(cfg, device_name):
    """应用侧 API token：依次尝试用户级、产品级鉴权，都没配置时用该设备的设备级鉴权。"""
    if cfg.get("user_id") and cfg.get("user_access_key"):
        return make_token(f"userid/{cfg['user_id']}", cfg["user_access_key"], version="2022-05-01")
    if cfg.get("product_access_key"):
        return make_token(f"products/{cfg['product_id']}", cfg["product_access_key"])
    device = next(d for d in cfg["devices"] if d["name"] == device_name)
    return device_token(cfg, device)


def topic(cfg, device_name, suffix):
    return f"$sys/{cfg['product_id']}/{device_name}/thing/property/{suffix}"
