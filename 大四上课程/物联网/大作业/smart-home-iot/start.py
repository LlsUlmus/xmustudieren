"""
一键启动：同时启动中枢服务(hub)与设备模拟器(simulator)，并自动打开浏览器。
用法：python start.py [--port 8080] [--no-browser]
"""
import argparse
import os
import subprocess
import sys
import time
import webbrowser

ROOT = os.path.dirname(os.path.abspath(__file__))

if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("--port", type=int, default=8080)
    ap.add_argument("--no-browser", action="store_true")
    args = ap.parse_args()
    env = dict(os.environ, PYTHONIOENCODING="utf-8", PYTHONUNBUFFERED="1")
    hub = subprocess.Popen([sys.executable, os.path.join(ROOT, "hub", "server.py"), "--port", str(args.port)], env=env)
    time.sleep(1.5)
    dev = subprocess.Popen([sys.executable, os.path.join(ROOT, "devices", "simulator.py"),
                            "--hub", f"ws://127.0.0.1:{args.port}/ws/device"], env=env)
    url = f"http://127.0.0.1:{args.port}/"
    print(f"\n系统已启动，请在浏览器访问 {url}  （Ctrl+C 退出）\n")
    if not args.no_browser:
        webbrowser.open(url)
    try:
        hub.wait()
    except KeyboardInterrupt:
        pass
    finally:
        for p in (dev, hub):
            p.terminate()
