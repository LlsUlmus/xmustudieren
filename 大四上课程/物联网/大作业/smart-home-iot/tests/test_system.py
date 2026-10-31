"""
端到端集成测试：真实启动中枢 + 设备模拟器，模拟浏览器通过 WebSocket/HTTP 交互，
验证 “感知 → 判断 → 响应” 全链路。运行：python tests/test_system.py
"""
import asyncio
import json
import os
import subprocess
import sys
import time

import aiohttp

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
PORT = 18080
BASE = f"http://127.0.0.1:{PORT}"


class UI:
    """模拟一个浏览器界面客户端"""

    def __init__(self, ws):
        self.ws = ws
        self.state = {}
        self.events = []
        self.dialogs = []

    async def pump(self, seconds):
        end = time.time() + seconds
        while time.time() < end:
            try:
                msg = await self.ws.receive(timeout=max(0.05, end - time.time()))
            except asyncio.TimeoutError:
                break
            if msg.type != aiohttp.WSMsgType.TEXT:
                continue
            m = json.loads(msg.data)
            if m["type"] in ("init", "state"):
                self.state = m["state"]
            elif m["type"] == "event":
                self.events.append(m["event"])
            elif m["type"] == "dialog":
                self.dialogs.append(m)

    def dev(self, name):
        return self.state["devices"][name]


results = []


def check(name, cond, detail=""):
    results.append((name, bool(cond)))
    print(f"  [{'PASS' if cond else 'FAIL'}] {name} {detail}")


async def main():
    async with aiohttp.ClientSession() as s:
        async with s.ws_connect(f"{BASE}/ws/ui") as ws:
            ui = UI(ws)
            await ui.pump(3)
            print("\n① 设备接入与数据上报")
            check("设备网关在线", ui.state.get("online"))
            check("收到遥测数据", "temperature" in ui.state.get("env", {}), str(ui.state.get("env", {}).get("temperature")))

            print("\n② 情形一：环境感知自动响应（人员进入 → 规则 R1 开灯）")
            await ws.send_json({"type": "sim", "action": "time", "value": 21 * 60})
            await ws.send_json({"type": "sim", "action": "presence", "value": {"present": True, "person": "妈妈"}})
            await ui.pump(3)
            check("感知到人员进入", ui.state["env"]["presence"] and ui.state["env"]["person"] == "妈妈")
            check("规则 R1 触发", any(e.get("rule") == "R1" for e in ui.events))
            check("灯已打开且为暖光", ui.dev("light")["on"] and ui.dev("light")["color_temp"] <= 3500,
                  str(ui.dev("light")))
            check("用户身份切换并加载偏好", ui.state["user"] == "妈妈")

            print("\n③ 情形二：文字指令控制")
            await ws.send_json({"type": "text", "text": "空调调到二十五度", "source": "text"})
            await ui.pump(2)
            check("NLU 解析为设备控制", ui.dialogs and ui.dialogs[-1]["intent"] == "control")
            check("空调开启且目标 25℃", ui.dev("ac")["on"] and ui.dev("ac")["target"] == 25, str(ui.dev("ac")))

            print("\n④ 情形三：模糊表达结合环境推理")
            await ws.send_json({"type": "text", "text": "太暗了", "source": "voice"})
            await ui.pump(2)
            check("意图为 feeling_dark", ui.dialogs[-1]["intent"] == "feeling_dark")
            check("灯光亮度提升", ui.dev("light")["brightness"] >= 80, str(ui.dev("light")))

            print("\n⑤ 情形四：场景联动（晚安 → 多设备）")
            await ws.send_json({"type": "text", "text": "晚安"})
            await ui.pump(3)
            check("灯光 10% 夜灯", ui.dev("light")["brightness"] == 10)
            check("播放白噪音", ui.dev("speaker")["on"] and ui.dev("speaker")["track"] == "白噪音")

            print("\n⑥ 按钮控制 + HTTP REST 接口")
            await ws.send_json({"type": "control", "device": "fan", "params": {"on": True, "speed": 3}})
            await ui.pump(1.5)
            check("按钮控制风扇 3 档", ui.dev("fan")["on"] and ui.dev("fan")["speed"] == 3)
            async with s.post(f"{BASE}/api/command", json={"text": "把风扇关了"}) as r:
                body = await r.json()
            await ui.pump(1.5)
            check("HTTP 指令接口", body["intent"] == "control" and not ui.dev("fan")["on"])
            async with s.get(f"{BASE}/api/state") as r:
                st = await r.json()
            check("HTTP 状态查询接口", st["online"] and "devices" in st)

            print("\n⑦ 人员离开 → 规则 R2 节能关闭")
            await ws.send_json({"type": "sim", "action": "speed", "value": 6})
            await ws.send_json({"type": "sim", "action": "presence", "value": {"present": False}})
            await ui.pump(4)
            check("规则 R2 触发", any(e.get("rule") == "R2" for e in ui.events))
            check("用电设备全部关闭", not any(ui.dev(d)["on"] for d in ("light", "fan", "ac", "speaker")))

            acks = [e for e in ui.events if "设备回执" in e["text"]]
            check("所有命令均收到设备回执", acks and all("成功" in e["text"] for e in acks), f"{len(acks)} 条")


if __name__ == "__main__":
    env = dict(os.environ, PYTHONIOENCODING="utf-8")
    log = open(os.path.join(ROOT, "tests", "system_test.log"), "w", encoding="utf-8")
    hub = subprocess.Popen([sys.executable, os.path.join(ROOT, "hub", "server.py"), "--port", str(PORT)],
                           env=env, stdout=log, stderr=subprocess.STDOUT)
    time.sleep(1.5)
    dev = subprocess.Popen([sys.executable, os.path.join(ROOT, "devices", "simulator.py"),
                            "--hub", f"ws://127.0.0.1:{PORT}/ws/device"], env=env, stdout=log, stderr=subprocess.STDOUT)
    try:
        asyncio.run(main())
    finally:
        dev.terminate()
        hub.terminate()
    passed = sum(ok for _, ok in results)
    print(f"\n结果：{passed}/{len(results)} 项通过")
    sys.exit(0 if passed == len(results) else 1)
