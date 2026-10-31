"""数据分析方法（全部基于实时模拟数据计算）。"""
import math
import statistics


def linear_trend(ts, ys):
    """最小二乘线性回归，返回斜率（单位/分钟）与拟合优度 R²。"""
    n = len(ys)
    if n < 3:
        return 0.0, 0.0
    t0 = ts[0]
    xs = [(t - t0) / 60.0 for t in ts]
    mx, my = sum(xs) / n, sum(ys) / n
    sxx = sum((x - mx) ** 2 for x in xs)
    if sxx == 0:
        return 0.0, 0.0
    sxy = sum((x - mx) * (y - my) for x, y in zip(xs, ys))
    k = sxy / sxx
    ss_tot = sum((y - my) ** 2 for y in ys)
    ss_res = sum((y - (my + k * (x - mx))) ** 2 for x, y in zip(xs, ys))
    r2 = 1 - ss_res / ss_tot if ss_tot else 0.0
    return k, r2


def moving_avg(ys, w=5):
    out = []
    for i in range(len(ys)):
        seg = ys[max(0, i - w + 1): i + 1]
        out.append(round(sum(seg) / len(seg), 2))
    return out


def vpd(temp, rh):
    """饱和水汽压差 VPD (kPa)，衡量作物蒸腾强度；番茄适宜 0.4~1.2 kPa。"""
    es = 0.6108 * math.exp(17.27 * temp / (temp + 237.3))
    return es * (1 - rh / 100.0)


def soil_analysis(records, low, high, window=60):
    """records: 有效土壤记录列表（dict），取最近 window 条做统计。"""
    recs = records[-window:]
    if not recs:
        return None
    out = {"n": len(recs), "window_s": round(recs[-1]["ts"] - recs[0]["ts"], 0)}
    for k in ("soil_moisture", "soil_temp", "air_temp", "air_humidity", "ec", "light", "co2"):
        vals = [r[k] for r in recs]
        out[k] = {"avg": round(statistics.fmean(vals), 2), "min": round(min(vals), 2),
                  "max": round(max(vals), 2),
                  "std": round(statistics.pstdev(vals), 2) if len(vals) > 1 else 0.0}
    # 湿度趋势：取最近 20 条做线性回归
    tail = recs[-20:]
    k, r2 = linear_trend([r["ts"] for r in tail], [r["soil_moisture"] for r in tail])
    cur = recs[-1]["soil_moisture"]
    out["moisture_slope"] = round(k, 3)            # %/分钟
    out["moisture_r2"] = round(r2, 3)
    if k < -0.05:
        trend = "下降"
        eta = (cur - low) / -k if cur > low else 0
        out["eta_low_min"] = round(eta, 1)
        out["trend_text"] = f"土壤湿度呈下降趋势（{k:.2f}%/min，R²={r2:.2f}），预计 {eta:.1f} 分钟后低于下限 {low}%"
    elif k > 0.05:
        trend = "上升"
        eta = (high - cur) / k if cur < high else 0
        out["eta_high_min"] = round(eta, 1)
        out["trend_text"] = f"土壤湿度呈上升趋势（+{k:.2f}%/min，R²={r2:.2f}），约 {eta:.1f} 分钟达到上限 {high}%"
    else:
        trend = "平稳"
        out["trend_text"] = f"土壤湿度基本平稳（{k:+.2f}%/min）"
    out["moisture_trend"] = trend
    last = recs[-1]
    v = vpd(last["air_temp"], last["air_humidity"])
    out["vpd"] = round(v, 2)
    out["vpd_text"] = "蒸腾偏弱、湿度过高，病害风险上升" if v < 0.4 else ("蒸腾过强，注意补水" if v > 1.6 else "适宜")
    # 病害风险指数：近 window 条记录中高湿（RH>85%）且 15~28℃ 的占比
    risk = sum(1 for r in recs if r["air_humidity"] > 85 and 15 <= r["air_temp"] <= 28) / len(recs)
    out["disease_risk"] = round(risk * 100, 1)
    in_range = sum(1 for r in recs if low <= r["soil_moisture"] <= high) / len(recs)
    out["moisture_in_range"] = round(in_range * 100, 1)
    return out


def pest_analysis(series, window=6):
    """series: [{ts, counts:{sp:n}, flag}]。返回各虫种统计、增长率、异常检测(z-score)。"""
    if not series:
        return None
    out = {"species": {}}
    recent = series[-window:]
    prev = series[-2 * window:-window]
    for sp in ("whitefly", "aphid", "thrips"):
        r = sum(s["counts"].get(sp, 0) for s in recent)
        p = sum(s["counts"].get(sp, 0) for s in prev) if prev else None
        growth = round((r - p) / p * 100, 1) if p else None
        out["species"][sp] = {"recent": r, "prev": p, "growth": growth,
                              "total": sum(s["counts"].get(sp, 0) for s in series)}
    # 白粉虱 z-score 异常检测：最近一次计数 vs 前 12 次（剔除已判定为误报的点）
    hist = [s["counts"]["whitefly"] for s in series[-13:-1] if s.get("flag") != "false_alarm"]
    cur = series[-1]["counts"]["whitefly"]
    if len(hist) >= 4:
        mu = statistics.fmean(hist)
        sd = max(statistics.pstdev(hist), 1.5)
        z = (cur - mu) / sd
    else:
        mu, z = cur, 0.0
    out["latest"] = series[-1]["counts"]
    out["baseline"] = round(mu, 1)
    out["zscore"] = round(z, 2)
    out["anomaly"] = z > 3.0 and cur > mu + 12
    tot = [sum(s["counts"].values()) for s in series]
    out["ma"] = moving_avg(tot, 3)[-1]
    return out


def irrigation_analysis(events, total_liters, base_liters, area):
    used = max(0.0, total_liters - base_liters)
    done = [e for e in events if e.get("end_ts")]
    durs = [e["end_ts"] - e["start_ts"] for e in done]
    gains = [(e["m_end"] - e["m_start"], e["liters"]) for e in done if e.get("liters", 0) > 0]
    eff = None
    if gains:
        eff = round(sum(g for g, _ in gains) / sum(l for _, l in gains) * 100, 2)  # 每100L提升湿度%
    return {
        "water_used_l": round(used, 1),
        "water_per_m2": round(used / area, 3),
        "events": len(events),
        "avg_duration_s": round(statistics.fmean(durs), 1) if durs else 0,
        "avg_liters": round(statistics.fmean([e["liters"] for e in done]), 1) if done else 0,
        "efficiency": eff,
    }
