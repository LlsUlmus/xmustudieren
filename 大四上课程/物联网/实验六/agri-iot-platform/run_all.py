"""一键启动：服务器（含内置 MQTT Broker）+ 设备模拟器。

python run_all.py      然后浏览器打开 http://127.0.0.1:8000
"""
import os
import subprocess
import sys
import time

ROOT = os.path.dirname(os.path.abspath(__file__))


def main():
    env = dict(os.environ, PYTHONIOENCODING="utf-8", PYTHONUNBUFFERED="1")
    server = subprocess.Popen([sys.executable, "-m", "server.app"], cwd=ROOT, env=env)
    time.sleep(3)
    sim = subprocess.Popen([sys.executable, "-m", "simulator.sim"], cwd=ROOT, env=env)
    print("\n平台已启动：http://127.0.0.1:8000  （手机访问请使用电脑局域网 IP:8000）  Ctrl+C 退出\n")
    try:
        while server.poll() is None and sim.poll() is None:
            time.sleep(1)
    except KeyboardInterrupt:
        pass
    finally:
        for p in (sim, server):
            p.terminate()


if __name__ == "__main__":
    main()
