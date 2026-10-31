"""裁剪 report/screenshots/*.png：去掉窗口标题栏、含主机名与时间的结束行、含本机路径的提示符行。"""
import os
import sys

import numpy as np
from PIL import Image

d = sys.argv[1] if len(sys.argv) > 1 else os.path.join(os.path.dirname(__file__), "..", "..", "report", "screenshots")
for f in sorted(os.listdir(d)):
    if not f.endswith(".png"):
        continue
    path = os.path.join(d, f)
    im = Image.open(path).convert("RGB")
    a = np.asarray(im).astype(int)
    bg = (a[..., 2] > 100) & (a[..., 0] < 30) & (a[..., 1] < 30)      # 控制台背景色
    green = (a[..., 1] > 120) & (a[..., 0] < 90) & (a[..., 2] < 90)   # 结束行为绿色
    rows = np.where(green.sum(1) > 20)[0]
    if not len(rows):
        continue                                                      # 已裁剪过
    body = np.where(bg.sum(1) > a.shape[1] * 0.5)[0]
    top, bottom = int(body.min()), int(rows.min())
    blank = (~bg).sum(1) <= 2
    y = bottom
    while y > top and not blank[y]:
        y -= 1
    while y > top and blank[y]:
        y -= 1
    cols = np.where(bg[top:y].sum(0) > (y - top) * 0.5)[0]
    im.crop((int(cols.min()), top, int(cols.max()) + 1, y + 12)).save(path)
    print("cropped", f)
