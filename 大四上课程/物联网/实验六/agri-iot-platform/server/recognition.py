"""无人机图像病虫害识别（基于颜色分割 + 连通域分析的轻量算法）。

步骤：
1. RGB → HSV；
2. 叶片掩膜：绿色色调、较高饱和度；
3. 病斑掩膜：褐色（低色调、高饱和度、低亮度）——早疫病典型病斑；
4. 虫害掩膜：近白色小点（低饱和度、高亮度）——白粉虱成虫；
5. 对病斑/白点掩膜做 8 邻域连通域标记，过滤噪声面积，得到目标框；
6. 病斑面积占叶片面积比例 + 白粉虱簇数量 → 严重度分级。
"""
import io
from collections import deque

import numpy as np
from PIL import Image, ImageDraw


class RecognitionError(Exception):
    pass


def _components(mask, min_area, max_area=10 ** 9, link=1):
    """连通域标记，返回 [(x0,y0,x1,y1,area)]。link>1 时把相邻较近的点并为一簇。"""
    h, w = mask.shape
    seen = np.zeros_like(mask, dtype=bool)
    ys, xs = np.nonzero(mask)
    out = []
    for y, x in zip(ys, xs):
        if seen[y, x]:
            continue
        q = deque([(y, x)])
        seen[y, x] = True
        x0 = x1 = x
        y0 = y1 = y
        area = 0
        while q:
            cy, cx = q.popleft()
            area += 1
            x0, x1, y0, y1 = min(x0, cx), max(x1, cx), min(y0, cy), max(y1, cy)
            for dy in range(-link, link + 1):
                for dx in range(-link, link + 1):
                    ny, nx = cy + dy, cx + dx
                    if 0 <= ny < h and 0 <= nx < w and mask[ny, nx] and not seen[ny, nx]:
                        seen[ny, nx] = True
                        q.append((ny, nx))
        if min_area <= area <= max_area:
            out.append((int(x0), int(y0), int(x1), int(y1), int(area)))
    return out


def recognize(jpg_bytes):
    """返回 (result_dict, annotated_jpg_bytes)。图像损坏时抛出 RecognitionError。"""
    if not jpg_bytes:
        raise RecognitionError("图像缺失")
    try:
        img = Image.open(io.BytesIO(jpg_bytes))
        img.load()
        img = img.convert("RGB")
    except Exception as e:  # noqa: BLE001
        raise RecognitionError(f"图像解码失败: {type(e).__name__}")
    if img.size[0] < 100 or img.size[1] < 100:
        raise RecognitionError("图像尺寸异常")

    hsv = np.asarray(img.convert("HSV")).astype(np.int16)
    H, S, V = hsv[..., 0], hsv[..., 1], hsv[..., 2]
    leaf = (H >= 55) & (H <= 110) & (S >= 80) & (V >= 60)
    lesion = (H >= 10) & (H <= 30) & (S >= 140) & (V <= 125) & (V >= 40)
    white = (S <= 45) & (V >= 225)

    leaf_px = int(leaf.sum())
    if leaf_px < img.size[0] * img.size[1] * 0.05:
        raise RecognitionError("未检测到作物冠层（图像无效）")

    # 过滤细长目标（滴灌带 JPEG 压缩后的伪色）与过大区域
    lesions = [c for c in _components(lesion, min_area=12, max_area=300)
               if 3 <= (c[2] - c[0]) <= 22 and 3 <= (c[3] - c[1]) <= 22
               and max(c[2] - c[0], c[3] - c[1]) <= 2.5 * min(c[2] - c[0], c[3] - c[1])]
    flies = _components(white, min_area=8, link=3)   # 白点间距较近，按簇聚合
    lesion_px = sum(c[4] for c in lesions)
    ratio = lesion_px / max(1, leaf_px + lesion_px)

    # 综合评分：病斑数 + 白粉虱簇数 × 0.8（与冠层大小无关，苗期/成株期可比）
    score = len(lesions) + len(flies) * 0.8
    if score < 4:
        grade, level = "健康", 0
    elif score < 10:
        grade, level = "轻度", 1
    elif score < 20:
        grade, level = "中度", 2
    else:
        grade, level = "重度", 3

    # 置信度：目标越明显越高（简单启发式）
    conf = 0.6 + min(0.35, (len(lesions) + len(flies)) * 0.012) if level else 0.9

    ann = img.copy()
    d = ImageDraw.Draw(ann)
    dets = []
    for (x0, y0, x1, y1, a) in lesions:
        d.rectangle([x0 - 3, y0 - 3, x1 + 3, y1 + 3], outline=(255, 40, 40), width=2)
        dets.append({"cls": "早疫病斑", "bbox": [x0, y0, x1, y1], "area": a})
    for (x0, y0, x1, y1, a) in flies:
        d.rectangle([x0 - 3, y0 - 3, x1 + 3, y1 + 3], outline=(0, 200, 255), width=2)
        dets.append({"cls": "白粉虱", "bbox": [x0, y0, x1, y1], "area": a})
    buf = io.BytesIO()
    ann.save(buf, "JPEG", quality=85)

    return {
        "grade": grade, "level": level, "confidence": round(conf, 2),
        "score": round(score, 1), "lesion_count": len(lesions), "whitefly_clusters": len(flies),
        "lesion_ratio": round(ratio * 100, 3),        # %
        "leaf_cover": round(leaf_px / (img.size[0] * img.size[1]) * 100, 1),  # %
        "detections": dets[:200],
        "method": "HSV颜色分割+连通域分析",
    }, buf.getvalue()


class FrameDiff:
    """摄像头画面变化检测：灰度下采样后逐像素差分，统计变化像素比例。"""

    def __init__(self, threshold=28):
        self.prev = None
        self.threshold = threshold

    def update(self, jpg_bytes):
        img = Image.open(io.BytesIO(jpg_bytes)).convert("L").resize((160, 90))
        arr = np.asarray(img).astype(np.int16)[8:, :]   # 去掉顶部 OSD 时间字幕
        brightness = float(arr.mean())
        if self.prev is None:
            self.prev = arr
            return 0.0, brightness
        diff = np.abs(arr - self.prev)
        # 消除全局亮度漂移（昼夜光照变化）
        diff = np.abs(diff - np.median(diff))
        self.prev = arr
        return float((diff > self.threshold).mean() * 100), brightness
