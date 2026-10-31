"""自动演示脚本：驱动浏览器完成一遍完整演示，录制视频并截取报告用截图。

前提：平台已运行（python run_all.py）且已运行 ≥70 秒（虫情基线需要历史数据）。
运行：python tools/demo_capture.py   需要 playwright + 本机 Edge 浏览器
输出：docs/screenshots/*.png、docs/demo.webm（以及 demo.mp4，若安装了 imageio-ffmpeg）
"""
import json
import os
import shutil
import subprocess
import time
import urllib.request

from playwright.sync_api import sync_playwright

BASE = "http://127.0.0.1:8000"
ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SHOT = os.path.join(ROOT, "docs", "screenshots")
VID = os.path.join(ROOT, "docs", "_video")


def post(path, body):
    req = urllib.request.Request(BASE + path, data=json.dumps(body).encode(), method="POST",
                                 headers={"Content-Type": "application/json"})
    urllib.request.urlopen(req).read()


def snap():
    return json.loads(urllib.request.urlopen(BASE + "/api/snapshot").read())


CAPTION_JS = """(t) => {
  let el = document.getElementById('__cap');
  if (!el) { el = document.createElement('div'); el.id='__cap';
    el.style.cssText='position:fixed;left:50%;top:70px;transform:translateX(-50%);z-index:99;background:rgba(15,40,25,.92);color:#fff;padding:10px 22px;border-radius:12px;font:600 20px "Microsoft YaHei";box-shadow:0 6px 24px rgba(0,0,0,.3);max-width:90%;text-align:center';
    document.body.appendChild(el); }
  el.textContent = t; }"""


def main():
    os.makedirs(SHOT, exist_ok=True)
    shutil.rmtree(VID, ignore_errors=True)
    while snap()["uptime"] < 70:
        print("等待平台积累基线数据…")
        time.sleep(5)
    for g in ("GH-1", "GH-2", "GH-3"):
        post("/api/scenario", {"gh": g, "name": "reset"})
        post(f"/api/irrigation/{g}/mode", {"auto": True})

    with sync_playwright() as p:
        browser = p.chromium.launch(channel="msedge")
        # 手机端截图
        m = browser.new_context(viewport={"width": 390, "height": 844}, device_scale_factor=2,
                                color_scheme="light", is_mobile=True, has_touch=True)
        mp = m.new_page()
        mp.goto(BASE)
        mp.wait_for_timeout(3500)
        mp.screenshot(path=os.path.join(SHOT, "09_mobile_top.png"))
        mp.locator("#secDrone").scroll_into_view_if_needed()
        mp.wait_for_timeout(800)
        mp.screenshot(path=os.path.join(SHOT, "10_mobile_drone.png"))
        m.close()

        ctx = browser.new_context(viewport={"width": 1440, "height": 900}, color_scheme="light",
                                  record_video_dir=VID, record_video_size={"width": 1440, "height": 900})
        page = ctx.new_page()
        page.goto(BASE)
        page.wait_for_timeout(1500)
        t_start = time.time()

        def cap(text, wait=0.0):
            page.evaluate(CAPTION_JS, text)
            if wait:
                page.wait_for_timeout(int(wait * 1000))

        def tab(gh):
            page.click(f".gh-tab[data-gh='{gh}']")
            page.wait_for_timeout(600)

        def scroll(sel):
            page.evaluate(f"document.querySelector('{sel}').scrollIntoView({{behavior:'smooth',block:'start'}})")
            page.wait_for_timeout(700)

        def shot(name, sel=None):
            if sel:
                # 元素截图时临时隐藏字幕与通知，截完再恢复
                page.add_style_tag(content="#__cap,#toasts{visibility:hidden!important}")
                page.locator(sel).screenshot(path=os.path.join(SHOT, name))
                page.evaluate("document.querySelectorAll('style').forEach(s=>{if(s.textContent.includes('__cap'))s.remove()})")
                page.evaluate("window.scrollTo(0,0)")
            else:
                page.evaluate("window.scrollTo(0,0)")
                page.wait_for_timeout(300)
                page.screenshot(path=os.path.join(SHOT, name))

        def wait_until(fn, timeout=20):
            end = time.time() + timeout
            while time.time() < end:
                if fn(snap()):
                    return True
                page.wait_for_timeout(500)
            return False

        # 1. 总览
        tab("GH-1")
        cap("智慧番茄温室监测平台：3 栋大棚 × 五项功能，MQTT 接入 + WebSocket 实时推送", 5)
        shot("01_overview.png")
        page.screenshot(path=os.path.join(SHOT, "00_fullpage.png"), full_page=True)
        shot("s_env.png", "#secEnv")
        scroll("#secIrr")
        cap("灌溉状态与控制 / 用水量统计；虫情测报灯堆叠计数与增长率、z-score 分析", 4)
        shot("s_irr.png", "#secIrr")
        shot("s_pest.png", "#secPest")
        scroll("body")

        # 2. 联动：湿度过低 → 自动灌溉
        cap("【联动】模拟 1 号棚快速干旱 → 土壤湿度低于下限 60% → 自动下发开阀指令")
        post("/api/scenario", {"gh": "GH-1", "name": "dry"})
        wait_until(lambda s: s["greenhouses"]["GH-1"]["irrigation"]["status"]["valve"] == "open", 15)
        page.wait_for_timeout(4000)
        shot("02_auto_irrigation.png")
        shot("s_irr_open.png", "#secIrr")

        # 3. 故障：传感器数据异常
        cap("【故障】注入传感器数据异常（超量程/数据卡死）→ 告警、数据剔除、自动灌溉安全暂停")
        post("/api/fault", {"gh": "GH-1", "fault": "sensor_abnormal", "on": True})
        page.wait_for_timeout(7000)
        shot("03_sensor_fault.png")
        cap("【恢复】解除故障 → 连续 3 次读数正常后告警自动恢复")
        post("/api/fault", {"gh": "GH-1", "fault": "sensor_abnormal", "on": False})
        wait_until(lambda s: not any(a["type"] == "sensor_abnormal" for a in s["alarms"]), 15)
        page.wait_for_timeout(1500)

        # 4. 故障：阀门不响应
        cap("【故障】注入阀门不响应 → 手动开阀 → 应答超时、自动重发、判定控制失败")
        post("/api/fault", {"gh": "GH-1", "fault": "valve_no_response", "on": True})
        page.click("#btnClose")
        wait_until(lambda s: any(a["type"] == "valve_no_response" for a in s["alarms"]), 15)
        page.wait_for_timeout(1500)
        shot("04_valve_fault.png")
        shot("s_irr_fail.png", "#secIrr")
        cap("【恢复】阀门恢复后重新下发指令 → 设备应答，告警恢复")
        post("/api/fault", {"gh": "GH-1", "fault": "valve_no_response", "on": False})
        page.click("#btnClose")
        wait_until(lambda s: not any(a["type"] == "valve_no_response" for a in s["alarms"]), 10)
        post("/api/irrigation/GH-1/mode", {"auto": True})
        page.wait_for_timeout(2000)

        # 5. 虫情误报 → 无人机核查
        tab("GH-2")
        cap("【故障+联动】2 号棚虫情计数误报 → z-score 异常 → 自动派无人机核查 → 图像无虫，判定误报")
        post("/api/fault", {"gh": "GH-2", "fault": "pest_false_alarm", "on": True})
        wait_until(lambda s: any(a["type"] == "pest_false_alarm" for a in s["alarms"]), 25)
        page.wait_for_timeout(2500)
        shot("05_pest_false_alarm.png")
        shot("s_pest_false.png", "#secPest")
        shot("s_drone_gh2.png", "#secDrone")
        post("/api/fault", {"gh": "GH-2", "fault": "pest_false_alarm", "on": False})

        # 6. 真实虫害
        tab("GH-3")
        cap("【联动】3 号棚真实虫害爆发 → 无人机核查识别出病斑与白粉虱 → 确认虫害并给出防治建议")
        post("/api/scenario", {"gh": "GH-3", "name": "pest_outbreak"})
        wait_until(lambda s: any(a["type"] == "pest_confirmed" for a in s["alarms"]), 25)
        page.wait_for_timeout(2500)
        shot("06_pest_confirmed.png")
        shot("s_drone_gh3.png", "#secDrone")
        shot("s_pest_confirmed.png", "#secPest")

        # 7. 摄像头离线 + 无人机图像缺失
        cap("【故障】3 号棚摄像头离线 + 无人机图像缺失 → 画面置灰告警、识别失败自动重试")
        post("/api/fault", {"gh": "GH-3", "fault": "camera_offline", "on": True})
        post("/api/fault", {"gh": "GH-3", "fault": "drone_image_missing", "on": True})
        post("/api/drone/GH-3/mission", {})
        wait_until(lambda s: any(a["type"] == "camera_offline" for a in s["alarms"])
                   and any(a["type"] == "drone_fail" for a in s["alarms"]), 20)
        page.wait_for_timeout(1500)
        shot("07_camera_drone_fault.png")
        shot("s_cam_off.png", "#secCam")
        shot("s_drone_fail.png", "#secDrone")
        cap("【恢复】解除故障 → 摄像头恢复推流、无人机重新巡检识别成功，告警转为已恢复")
        post("/api/fault", {"gh": "GH-3", "fault": "camera_offline", "on": False})
        post("/api/fault", {"gh": "GH-3", "fault": "drone_image_missing", "on": False})
        post("/api/scenario", {"gh": "GH-3", "name": "reset"})
        post("/api/drone/GH-3/mission", {})
        wait_until(lambda s: not any(a["type"] in ("camera_offline", "drone_fail") for a in s["alarms"]), 20)
        page.wait_for_timeout(2000)
        page.evaluate("document.querySelector('details.hist').open = true")
        page.wait_for_timeout(1000)
        shot("08_recovered.png")
        shot("s_cam.png", "#secCam")
        shot("s_alarm.png", ".col-ops .card:nth-child(1)")
        shot("s_link.png", ".col-ops .card:nth-child(2)")
        cap("演示结束：五项功能 · 实时数据 · 统计分析 · 功能联动 · 故障告警与恢复", 3)
        print(f"演示时长 {time.time() - t_start:.0f}s")
        video = page.video.path()
        ctx.close()
        browser.close()

    out = os.path.join(ROOT, "docs", "demo.webm")
    shutil.move(video, out)
    shutil.rmtree(VID, ignore_errors=True)
    try:
        import imageio_ffmpeg
        subprocess.run([imageio_ffmpeg.get_ffmpeg_exe(), "-y", "-i", out, "-c:v", "libx264", "-pix_fmt", "yuv420p",
                        "-crf", "23", os.path.join(ROOT, "docs", "demo.mp4")], check=True,
                       stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        print("已生成 docs/demo.mp4")
    except Exception as e:  # noqa: BLE001
        print("未转换 mp4：", e)


if __name__ == "__main__":
    main()
