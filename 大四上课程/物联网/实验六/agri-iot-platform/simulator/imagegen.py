"""用 Pillow 程序化生成模拟图像：
- 无人机俯拍图：番茄冠层 + 按病害程度生成的病斑（褐色病斑+黄色晕圈）+ 白粉虱聚集白点
- 摄像头画面：棚内透视画面，亮度随模拟昼夜变化，偶尔有人员/巡检车移动
"""
import io
import math
import random

from PIL import Image, ImageDraw, ImageFont

SOIL_COLOR = (152, 136, 112)       # 低饱和度土壤底色（与病斑区分）
LESION_COLOR = (92, 52, 18)        # 早疫病褐色病斑
HALO_COLOR = (214, 196, 52)        # 病斑黄色晕圈
WHITEFLY_COLOR = (246, 246, 240)   # 白粉虱


def _font(size):
    for f in ("C:/Windows/Fonts/msyh.ttc", "C:/Windows/Fonts/simhei.ttf",
              "/usr/share/fonts/truetype/wqy/wqy-microhei.ttc"):
        try:
            return ImageFont.truetype(f, size)
        except OSError:
            continue
    return ImageFont.load_default()


def _plant_layout(gh_index):
    """每个大棚固定的植株布局（种子固定，保证多次巡检图像可比）。"""
    rnd = random.Random(1000 + gh_index)
    plants = []
    for r in range(5):
        for c in range(11):
            x = 28 + c * 42 + rnd.randint(-5, 5)
            y = 36 + r * 62 + rnd.randint(-5, 5)
            rad = rnd.randint(15, 20)
            plants.append((x, y, rad))
    return plants


def drone_image(gh_index, disease_level, whitefly_density, seed=None, stage="结果期"):
    """生成无人机俯拍图，返回 JPEG bytes。

    disease_level: 0~1 病害严重度；whitefly_density: 0~1 白粉虱密度。
    """
    rnd = random.Random(seed)
    w, h = 480, 320
    img = Image.new("RGB", (w, h), SOIL_COLOR)
    d = ImageDraw.Draw(img)
    # 垄沟与滴灌带
    for r in range(5):
        y = 36 + r * 62
        d.rectangle([0, y - 3, w, y + 3], fill=(140, 124, 100))
        d.line([0, y + 24, w, y + 24], fill=(40, 40, 40), width=1)
    plants = _plant_layout(gh_index)
    seedling = stage == "育苗期"
    for (x, y, rad) in plants:
        if seedling:
            rad = int(rad * 0.65)
        base = (46 + rnd.randint(-8, 8), 128 + rnd.randint(-12, 12), 52 + rnd.randint(-8, 8))
        d.ellipse([x - rad, y - rad, x + rad, y + rad], fill=base)
        for _ in range(7):  # 叶片纹理
            a = rnd.random() * math.tau
            rr = rad * 0.55
            lx, ly = x + math.cos(a) * rr, y + math.sin(a) * rr
            lr = rad * 0.45
            shade = (base[0] + rnd.randint(-10, 18), base[1] + rnd.randint(-10, 25), base[2] + rnd.randint(-5, 12))
            d.ellipse([lx - lr, ly - lr * 0.7, lx + lr, ly + lr * 0.7], fill=shade)
        # 少量果实（结果期）
        if stage == "结果期" and rnd.random() < 0.5:
            fx, fy = x + rnd.randint(-6, 6), y + rnd.randint(-6, 6)
            d.ellipse([fx - 3, fy - 3, fx + 3, fy + 3], fill=(200, 40, 35))

    # 病斑：数量与病害等级成正比
    n_lesion = int(disease_level * 45 + (rnd.random() < disease_level * 2) * rnd.randint(0, 3))
    infected = rnd.sample(plants, k=min(len(plants), max(1, int(len(plants) * min(1.0, disease_level * 1.6)))))
    for _ in range(n_lesion):
        x, y, rad = rnd.choice(infected)
        if seedling:
            rad = int(rad * 0.65)
        a = rnd.random() * math.tau
        rr = rnd.random() * rad * 0.6
        cx, cy = x + math.cos(a) * rr, y + math.sin(a) * rr
        s = rnd.randint(3, 5)
        d.ellipse([cx - s - 2, cy - s - 2, cx + s + 2, cy + s + 2], fill=HALO_COLOR)
        d.ellipse([cx - s, cy - s, cx + s, cy + s], fill=LESION_COLOR)

    # 白粉虱聚集：成簇白点
    n_cluster = int(whitefly_density * 25)
    for _ in range(n_cluster):
        x, y, rad = rnd.choice(plants)
        if seedling:
            rad = int(rad * 0.65)
        cx, cy = x + rnd.randint(-rad // 2, rad // 2), y + rnd.randint(-rad // 2, rad // 2)
        for _ in range(rnd.randint(6, 12)):
            px, py = cx + rnd.randint(-5, 5), cy + rnd.randint(-5, 5)
            d.rectangle([px, py, px + 1, py + 1], fill=WHITEFLY_COLOR)

    buf = io.BytesIO()
    img.save(buf, "JPEG", quality=88)
    return buf.getvalue()


def camera_frame(gh_index, daylight, person_x=None, label="", seed=None):
    """生成摄像头画面。daylight: 0~1；person_x: 画面中移动目标横坐标（None 表示无人）。"""
    rnd = random.Random(seed)
    w, h = 480, 270
    b = 0.22 + 0.78 * daylight
    img = Image.new("RGB", (w, h))
    d = ImageDraw.Draw(img)

    def c(rgb):
        return tuple(int(v * b) for v in rgb)

    # 棚顶（薄膜）渐变
    for y in range(0, 110):
        t = y / 110
        d.line([0, y, w, y], fill=c((int(205 - 40 * t), int(222 - 30 * t), int(232 - 30 * t))))
    # 地面
    d.rectangle([0, 110, w, h], fill=c((126, 104, 80)))
    vx, vy = 240, 96
    # 拱架
    for i in range(6):
        y = 8 + i * 17
        d.arc([20 + i * 30, y - 40, w - 20 - i * 30, y + 120], 200, 340, fill=c((235, 235, 235)), width=2)
    # 作物垄（透视）
    rows = [-260, -120, 20, 160, 300]
    for idx, bx in enumerate(rows):
        x0 = 240 + bx
        poly = [(x0 - 45, h), (x0 + 45, h), (vx + bx * 0.08 + 4, vy + 18), (vx + bx * 0.08 - 4, vy + 18)]
        d.polygon(poly, fill=c((52, 120 + idx * 4, 50)))
        for k in range(10):  # 植株块
            t = k / 10
            px = x0 + (vx + bx * 0.08 - x0) * t
            py = h + (vy + 18 - h) * t
            r = 22 * (1 - t) + 3
            sway = rnd.uniform(-1.0, 1.0)
            d.ellipse([px - r + sway, py - r * 1.8, px + r + sway, py], fill=c((60, 140, 58)))
    # 移动目标（农事人员/巡检车）
    if person_x is not None:
        px = int(person_x)
        d.rectangle([px - 12, 150, px + 12, 215], fill=c((235, 120, 30)))
        d.ellipse([px - 9, 128, px + 9, 148], fill=c((240, 200, 160)))
        d.rectangle([px - 12, 215, px - 3, 245], fill=c((50, 60, 90)))
        d.rectangle([px + 3, 215, px + 12, 245], fill=c((50, 60, 90)))
    # OSD 叠加
    d.rectangle([0, 0, w, 22], fill=(0, 0, 0))
    d.text((6, 3), label, fill=(255, 255, 255), font=_font(13))
    buf = io.BytesIO()
    img.save(buf, "JPEG", quality=70)
    return buf.getvalue()
