# -*- coding: utf-8 -*-
"""用 Playwright 对看板各页面截图（用于实验报告）。用法：python tools/shot.py 输出目录"""
import sys
import time

from playwright.sync_api import sync_playwright

out = sys.argv[1] if len(sys.argv) > 1 else "screenshots"
URL = "http://127.0.0.1:5000/"


def launch(p):
    for ch in ("msedge", "chrome", None):
        try:
            return p.chromium.launch(channel=ch) if ch else p.chromium.launch()
        except Exception:
            continue
    raise RuntimeError("未找到可用浏览器")


with sync_playwright() as p:
    b = launch(p)
    pg = b.new_page(viewport={"width": 1920, "height": 1080})
    errs = []
    pg.on("console", lambda m: errs.append(m.text) if m.type == "error" else None)
    pg.on("pageerror", lambda e: errs.append(str(e)))
    pg.goto(URL)
    time.sleep(3)
    steps = sys.argv[2].split(",") if len(sys.argv) > 2 else ["board", "analysis", "alarm", "detail", "console"]
    for s in steps:
        if s == "board":
            pg.click("text=实时看板"); time.sleep(1.5)
        elif s == "analysis":
            pg.click("button[data-view=analysis]"); time.sleep(1.5)
        elif s == "alarm":
            pg.click("button[data-view=alarm]"); time.sleep(1.5)
        elif s == "detail":
            pg.click("button[data-view=board]"); time.sleep(0.5)
            pg.click(".dev[data-id=ws_a]"); time.sleep(2)
        elif s == "console":
            pg.click("[data-close]") if pg.is_visible("#detailMask") else None
            pg.click("#btnConsole"); time.sleep(1)
        pg.screenshot(path=f"{out}/{s}.png", full_page=True)
        if s == "detail":
            pg.click("[data-close]")
    print("errors:", errs)
    b.close()
