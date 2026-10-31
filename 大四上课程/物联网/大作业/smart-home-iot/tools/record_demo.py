"""
自动化演示脚本：启动系统 → 用浏览器自动执行一组交互 → 录制演示视频并截取关键界面。
依赖：pip install playwright（使用本机 Edge/Chrome，无需下载浏览器）
用法：python tools/record_demo.py [--out docs]
"""
import argparse
import os
import shutil
import subprocess
import sys
import time

from playwright.sync_api import sync_playwright

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
PORT = 8090
W, H = 1440, 900

CAPTION_JS = """(t) => {
  let el = document.getElementById('demo-cap');
  if (!el) {
    el = document.createElement('div'); el.id = 'demo-cap';
    el.style.cssText = 'position:fixed;left:50%;bottom:22px;transform:translateX(-50%);z-index:9999;' +
      'background:rgba(10,14,28,.92);color:#fff;border:1px solid #4f8cff;border-radius:12px;' +
      'padding:10px 22px;font:600 20px "Microsoft YaHei";box-shadow:0 8px 30px rgba(0,0,0,.5);transition:opacity .3s';
    document.body.appendChild(el);
  }
  el.textContent = t; el.style.opacity = t ? 1 : 0;
}"""


def main(out_dir):
    shots = os.path.join(out_dir, "screenshots")
    os.makedirs(shots, exist_ok=True)
    env = dict(os.environ, PYTHONIOENCODING="utf-8")
    hub = subprocess.Popen([sys.executable, os.path.join(ROOT, "hub", "server.py"), "--port", str(PORT)],
                           env=env, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    time.sleep(1.5)
    dev = subprocess.Popen([sys.executable, os.path.join(ROOT, "devices", "simulator.py"),
                            "--hub", f"ws://127.0.0.1:{PORT}/ws/device"],
                           env=env, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    time.sleep(1.5)
    video_tmp = os.path.join(out_dir, "_video")
    try:
        with sync_playwright() as p:
            browser = p.chromium.launch(channel="msedge")
            ctx = browser.new_context(viewport={"width": W, "height": H}, device_scale_factor=1,
                                      record_video_dir=video_tmp, record_video_size={"width": W, "height": H})
            page = ctx.new_page()
            page.goto(f"http://127.0.0.1:{PORT}/")
            cap = lambda t: page.evaluate(CAPTION_JS, t)
            shot = lambda name: page.screenshot(path=os.path.join(shots, name))

            def say(text, wait=3.5):
                page.fill("#chat-input", "")
                page.type("#chat-input", text, delay=90)
                page.wait_for_timeout(300)
                page.press("#chat-input", "Enter")
                page.wait_for_timeout(int(wait * 1000))

            page.wait_for_timeout(2500)
            cap("① 系统启动：设备网关通过 WebSocket 接入中枢，每秒上报温湿度/光照/人体存在")
            page.wait_for_timeout(3500)
            cap("模拟时间 20:00 已入夜 → 规则 R4「日落关窗帘」自动关闭窗帘")
            page.wait_for_timeout(3000)
            shot("01_初始状态.png")

            cap("② 情形一【环境感知】：小明进入客厅 → 识别身份、加载偏好，R1 人来灯亮(夜间暖光)、R3 高温制冷")
            page.select_option("#sim-person", "小明")
            page.click("#sim-enter")
            page.wait_for_timeout(6000)
            shot("02_人员进入自动响应.png")

            cap("③ 情形二【文字指令】：一句话多设备、中文数字解析")
            say("把灯调成冷光，亮度调到百分之六十")
            say("打开风扇和音乐", 3)
            shot("03_文字指令控制.png")

            cap("④ 情形三【模糊表达 + 环境推理】：系统读取当前室温与空调状态再决策")
            say("我有点热", 4)
            shot("04_模糊表达推理.png")

            cap("⑤ 按钮交互：界面直接操控设备，设备回执后卡片闪烁确认")
            page.click("#card-fan .seg button[data-val='3']")
            page.wait_for_timeout(1500)
            page.click("#card-ac [data-step='1']")
            page.wait_for_timeout(2500)

            cap("⑥ 情形四【场景联动】：“我要看电影” → 灯光 / 窗帘 / 音箱 多设备联动")
            say("我要看电影", 4.5)
            shot("05_观影场景联动.png")

            cap("⑦ 时间跳转到 07:00 白天，说“太暗了” → 白天优先拉开窗帘利用自然光")
            page.click("[data-time='420']")
            page.wait_for_timeout(2000)
            say("太暗了", 5)
            shot("06_白天太暗开窗帘.png")

            cap("⑧ 状态查询：“现在温度多少”")
            say("现在温度多少", 4)
            shot("07_状态查询.png")

            cap("⑨ 人员离开 → 无人 10 分钟后规则 R2 自动关闭所有用电设备（节能）")
            page.select_option("#sim-speed", "6")
            page.click("#sim-leave")
            page.wait_for_timeout(7000)
            shot("08_人走设备关.png")
            page.select_option("#sim-speed", "1")
            cap("完整闭环：感知（传感器/用户输入）→ 判断（规则引擎/NLU）→ 响应（设备执行 + 界面动态更新）")
            page.wait_for_timeout(4500)
            cap("")
            shot("09_事件流全貌.png")
            video_path = page.video.path()
            ctx.close()
            browser.close()
        webm = os.path.join(out_dir, "演示视频.webm")
        os.replace(video_path, webm)
        shutil.rmtree(video_tmp, ignore_errors=True)
        try:  # 有 ffmpeg 时额外转为 mp4，兼容性更好
            import imageio_ffmpeg
            mp4 = os.path.join(out_dir, "演示视频.mp4")
            subprocess.run([imageio_ffmpeg.get_ffmpeg_exe(), "-y", "-loglevel", "error", "-i", webm,
                            "-c:v", "libx264", "-pix_fmt", "yuv420p", "-crf", "23", "-movflags", "+faststart", mp4],
                           check=True)
            print("视频：", mp4)
        except Exception as e:  # noqa
            print("未转换 mp4：", e, "\n视频：", webm)
    finally:
        dev.terminate()
        hub.terminate()


if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("--out", default=os.path.join(ROOT, "docs"))
    main(os.path.abspath(ap.parse_args().out))
