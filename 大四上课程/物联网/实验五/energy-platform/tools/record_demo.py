# -*- coding: utf-8 -*-
"""
自动录制系统运行演示视频（≤2 分钟）。
前提：server.py 与 simulator.py 已启动（建议 simulator --start 07:00）。
用法：python tools/record_demo.py 输出目录
"""
import glob
import os
import shutil
import subprocess
import sys
import time

import imageio_ffmpeg
import requests
from playwright.sync_api import sync_playwright

OUT = os.path.abspath(sys.argv[1] if len(sys.argv) > 1 else "demo")
URL = "http://127.0.0.1:5000/"
W, H = 1600, 900
os.makedirs(OUT, exist_ok=True)

CAPTION_JS = """(t) => {
  let el = document.getElementById('__cap');
  if (!el) { el = document.createElement('div'); el.id='__cap';
    el.style.cssText='position:fixed;left:50%;bottom:26px;transform:translateX(-50%);z-index:999;padding:10px 26px;border-radius:24px;background:rgba(0,0,0,.72);color:#fff;font-size:20px;border:1px solid #3fd0ff;box-shadow:0 0 20px rgba(63,208,255,.5);pointer-events:none';
    document.body.appendChild(el); }
  el.textContent = t; }"""


def fault(device, ftype, ticks):
    requests.post(URL + "api/fault", json={"device": device, "type": ftype, "ticks": ticks}, timeout=3)


with sync_playwright() as p:
    browser = None
    for ch in ("msedge", "chrome", None):
        try:
            browser = p.chromium.launch(channel=ch) if ch else p.chromium.launch()
            break
        except Exception:
            pass
    ctx = browser.new_context(viewport={"width": W, "height": H}, record_video_dir=OUT,
                              record_video_size={"width": W, "height": H})
    pg = ctx.new_page()
    cap = lambda t: pg.evaluate(CAPTION_JS, t)
    t0 = time.time()
    pg.goto(URL)
    time.sleep(2)

    cap("① 能源监测数字看板：整体能耗概览 / 仪表盘 / 实时曲线 / 监测对象 / 能流 / 占比排名")
    time.sleep(7)
    cap("② 模拟数据每秒上报（仿真 5 分钟/周期），曲线、仪表、统计实时联动更新")
    pg.mouse.move(1000, 250); time.sleep(3)
    pg.click("#rtSeg button[data-k=dev]"); time.sleep(3)
    pg.click("#rtSeg button[data-k=park]"); time.sleep(1)
    pg.click("#pieSeg button[data-k=group]"); time.sleep(2.5)
    pg.click("#pieSeg button[data-k=dev]"); time.sleep(1)

    cap("③ 点击监测对象卡片，查看详细信息：实时功率、阈值、逐时电量、告警记录")
    pg.click(".dev[data-id=ws_a]"); time.sleep(6)
    pg.click("[data-close]"); time.sleep(1)

    cap("④ 模拟控制台：注入异常（功率超限 / 数据中断 / 制冷故障）")
    pg.click("#btnConsole"); time.sleep(1.5)
    pg.select_option("#fDev", "ws_b"); pg.select_option("#fType", "overload"); pg.fill("#fTicks", "25")
    time.sleep(1); pg.click("#fGo"); time.sleep(1.5)
    pg.click("button[data-q='ev,offline,10']"); time.sleep(1.5)
    pg.click("button[data-q='dc_cool,cooling,10']"); time.sleep(1.5)
    pg.click("[data-close-console]")
    cap("⑤ 系统识别异常：告警横幅、卡片变红/置灰、实时告警列表、PUE 仪表超限")
    time.sleep(10)

    cap("⑥ 告警中心：告警统计、规则、记录（发生/恢复/确认）")
    pg.click("button[data-view=alarm]"); time.sleep(5)
    btn = pg.query_selector("#alarmTbl button[data-ack]")
    if btn:
        btn.click()
    time.sleep(3)

    cap("⑦ 统计分析：分时段趋势、今昨对比、区域堆叠、峰谷平电费")
    pg.click("button[data-view=analysis]"); time.sleep(7)
    cap("⑦ 统计分析：电费估算排名、光伏与用电对比、碳排放追踪、数据中心 PUE")
    pg.mouse.wheel(0, 900); time.sleep(8)
    pg.mouse.wheel(0, -900); time.sleep(1)

    cap("⑧ 异常恢复后告警自动消除，看板恢复正常运行")
    fault("", "clear", 0)
    pg.click("button[data-view=board]"); time.sleep(10)
    cap("绿源零碳产业园 · 能源互联网监测平台 演示结束")
    time.sleep(3)
    print("录制时长 %.1f 秒" % (time.time() - t0))
    video = pg.video.path()
    ctx.close(); browser.close()

mp4 = os.path.join(OUT, "系统运行演示.mp4")
ff = imageio_ffmpeg.get_ffmpeg_exe()
subprocess.run([ff, "-y", "-i", video, "-c:v", "libx264", "-pix_fmt", "yuv420p", "-crf", "23",
                "-preset", "medium", "-movflags", "+faststart", mp4], check=True,
               stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
os.remove(video)
print("视频已保存：", mp4)
