# -*- coding: utf-8 -*-
"""Parse JMeter JTL results and server monitor logs for bottleneck analysis."""
import csv
import json
import statistics
from pathlib import Path

BASE = Path(__file__).parent
RESULTS = BASE / "jmeter" / "results"
OUTPUT = RESULTS / "analysis.json"

SCENARIOS = {
    "scenario1": {
        "name": "场景1 - 性能测试",
        "type": "性能测试（基准负载）",
        "jtl": RESULTS / "scenario1-results.jtl",
        "stats": RESULTS / "scenario1-report" / "statistics.json",
        "monitor": RESULTS / "scenario1-server.csv",
        "threads": 15,
        "loops": 20,
        "ramp_up": 10,
    },
    "scenario2": {
        "name": "场景2 - 压力测试",
        "type": "压力测试（高并发）",
        "jtl": RESULTS / "scenario2-results.jtl",
        "stats": RESULTS / "scenario2-report" / "statistics.json",
        "monitor": RESULTS / "scenario2-server.csv",
        "threads": 50,
        "loops": 30,
        "ramp_up": 5,
    },
}


def percentile(values, pct):
    if not values:
        return 0.0
    sorted_vals = sorted(values)
    k = (len(sorted_vals) - 1) * pct / 100.0
    f = int(k)
    c = min(f + 1, len(sorted_vals) - 1)
    if f == c:
        return float(sorted_vals[f])
    return sorted_vals[f] + (sorted_vals[c] - sorted_vals[f]) * (k - f)


def parse_jtl(jtl_path: Path) -> dict:
    if not jtl_path.exists():
        return {"available": False}

    elapsed_list, latency_list, connect_list, download_list = [], [], [], []
    network_list, errors = [], 0
    http_count = 0
    first_ts, last_ts = None, None

    with jtl_path.open(encoding="utf-8", newline="") as f:
        reader = csv.DictReader(f)
        for row in reader:
            if row.get("label") != "GET /greeting":
                continue
            ts = int(row.get("timeStamp", 0))
            if first_ts is None:
                first_ts = ts
            last_ts = ts
            http_count += 1
            if row.get("success", "true").lower() != "true":
                errors += 1
            elapsed = float(row.get("elapsed", 0))
            latency = float(row.get("Latency", elapsed))
            connect = float(row.get("Connect", 0))
            download = max(elapsed - latency, 0)
            network = max(latency - connect, 0)

            elapsed_list.append(elapsed)
            latency_list.append(latency)
            connect_list.append(connect)
            download_list.append(download)
            network_list.append(network)

    if not elapsed_list:
        return {"available": False}

    duration_sec = max((last_ts - first_ts) / 1000.0, 0.001) if first_ts and last_ts else 0.001
    throughput = http_count / duration_sec if http_count else 0

    def stats(values):
        return {
            "mean": round(statistics.mean(values), 2),
            "median": round(statistics.median(values), 2),
            "min": round(min(values), 2),
            "max": round(max(values), 2),
            "p90": round(percentile(values, 90), 2),
            "p95": round(percentile(values, 95), 2),
            "p99": round(percentile(values, 99), 2),
        }

    return {
        "available": True,
        "sample_count": http_count,
        "error_count": errors,
        "error_rate": round(errors / http_count * 100, 2) if http_count else 0,
        "throughput_tps": round(throughput, 2),
        "elapsed_ms": stats(elapsed_list),
        "latency_ms": stats(latency_list),
        "connect_ms": stats(connect_list),
        "network_ms": stats(network_list),
        "download_ms": stats(download_list),
    }


def parse_monitor(monitor_path: Path) -> dict:
    if not monitor_path.exists():
        return {"available": False}

    cpu_vals, mem_vals = [], []
    with monitor_path.open(encoding="utf-8", newline="") as f:
        reader = csv.DictReader(f)
        for row in reader:
            cpu_vals.append(float(row.get("cpu_percent", 0)))
            mem_vals.append(float(row.get("memory_mb", 0)))

    if not cpu_vals:
        return {"available": False}

    return {
        "available": True,
        "cpu_percent": {
            "mean": round(statistics.mean(cpu_vals), 2),
            "max": round(max(cpu_vals), 2),
        },
        "memory_mb": {
            "mean": round(statistics.mean(mem_vals), 2),
            "max": round(max(mem_vals), 2),
            "start": round(mem_vals[0], 2),
            "end": round(mem_vals[-1], 2),
            "growth_mb": round(mem_vals[-1] - mem_vals[0], 2),
        },
    }


def classify_bottleneck(jtl: dict, monitor: dict, scenario_type: str) -> dict:
    if not jtl.get("available"):
        return {"summary": "无测试数据", "categories": []}

    categories = []
    elapsed_mean = jtl["elapsed_ms"]["mean"]
    connect_mean = jtl["connect_ms"]["mean"]
    network_mean = jtl["network_ms"]["mean"]
    download_mean = jtl["download_ms"]["mean"]
    error_rate = jtl["error_rate"]

    if connect_mean > elapsed_mean * 0.3:
        categories.append({
            "type": "网络瓶颈",
            "detail": f"TCP 连接时间均值 {connect_mean}ms，占总响应时间比例偏高，可能存在连接池或端口耗尽问题。",
            "severity": "中",
        })
    elif connect_mean < 2:
        categories.append({
            "type": "网络连接",
            "detail": f"连接时间均值 {connect_mean}ms，Keep-Alive 生效，连接建立不是瓶颈。",
            "severity": "低",
        })

    if network_mean > download_mean * 2 and network_mean > 5:
        categories.append({
            "type": "服务器/网络等待",
            "detail": f"首字节等待（Latency-Connect）均值 {network_mean}ms，服务器处理或网络往返占主导。",
            "severity": "中" if network_mean < 50 else "高",
        })
    else:
        categories.append({
            "type": "服务器处理",
            "detail": f"首字节等待均值 {network_mean}ms，Spring Boot 接口处理较快。",
            "severity": "低",
        })

    if download_mean > 1:
        categories.append({
            "type": "下载/传输时间",
            "detail": f"响应体下载时间均值 {download_mean}ms（Elapsed-Latency），JSON 体量小，传输开销可忽略。",
            "severity": "低",
        })

    if monitor.get("available"):
        cpu_max = monitor["cpu_percent"]["max"]
        mem_growth = monitor["memory_mb"]["growth_mb"]
        if cpu_max > 80:
            categories.append({
                "type": "CPU 资源",
                "detail": f"压测期间 JVM 进程 CPU 峰值 {cpu_max}%，CPU 可能成为瓶颈。",
                "severity": "高",
            })
        else:
            categories.append({
                "type": "CPU 资源",
                "detail": f"压测期间 JVM 进程 CPU 峰值 {cpu_max}%，CPU 资源充足。",
                "severity": "低",
            })

        if mem_growth > 50:
            categories.append({
                "type": "内存泄漏风险",
                "detail": f"测试期间内存增长 {mem_growth}MB，需关注是否存在内存泄漏。",
                "severity": "中",
            })
        else:
            categories.append({
                "type": "内存分析",
                "detail": f"内存从 {monitor['memory_mb']['start']}MB 到 {monitor['memory_mb']['end']}MB，"
                          f"增长 {mem_growth}MB，无明显泄漏迹象。",
                "severity": "低",
            })

    if error_rate > 0:
        categories.append({
            "type": "功能/稳定性",
            "detail": f"错误率 {error_rate}%，存在断言失败或超时，{scenario_type}下系统稳定性下降。",
            "severity": "高" if error_rate > 5 else "中",
        })
    else:
        categories.append({
            "type": "功能/稳定性",
            "detail": "错误率 0%，检查点（状态码、响应内容、响应时间）全部通过。",
            "severity": "低",
        })

    if elapsed_mean > 100:
        summary = f"平均响应 {elapsed_mean}ms 偏高，需优化服务器或降低并发。"
    elif scenario_type.startswith("压力") and jtl["elapsed_ms"]["p95"] > jtl["elapsed_ms"]["mean"] * 3:
        summary = "压力场景下尾部延迟明显增大，系统接近性能拐点。"
    else:
        summary = "整体性能良好，无明显硬件或架构瓶颈。"

    return {"summary": summary, "categories": categories}


def load_jmeter_stats(stats_path: Path) -> dict:
    if stats_path.exists():
        return json.loads(stats_path.read_text(encoding="utf-8"))
    return {}


def main():
    analysis = {"scenarios": {}, "comparison": {}}
    throughputs, p95s = [], []

    for key, cfg in SCENARIOS.items():
        jtl = parse_jtl(cfg["jtl"])
        monitor = parse_monitor(cfg["monitor"])
        jmeter_stats = load_jmeter_stats(cfg["stats"])
        bottleneck = classify_bottleneck(jtl, monitor, cfg["type"])

        analysis["scenarios"][key] = {
            "name": cfg["name"],
            "type": cfg["type"],
            "config": {
                "threads": cfg["threads"],
                "loops": cfg["loops"],
                "ramp_up_sec": cfg["ramp_up"],
                "total_requests": cfg["threads"] * cfg["loops"],
            },
            "jmeter_stats": jmeter_stats,
            "jtl_analysis": jtl,
            "server_monitor": monitor,
            "bottleneck": bottleneck,
        }

        if jtl.get("available"):
            throughputs.append((cfg["name"], jtl["throughput_tps"]))
            p95s.append((cfg["name"], jtl["elapsed_ms"]["p95"]))

    if len(throughputs) == 2:
        t1, t2 = throughputs[0][1], throughputs[1][1]
        p1, p2 = p95s[0][1], p95s[1][1]
        analysis["comparison"] = {
            "throughput_change": round((t2 - t1) / t1 * 100, 1) if t1 else 0,
            "p95_change": round((p2 - p1) / p1 * 100, 1) if p1 else 0,
            "text": (
                f"压力测试相比性能测试：吞吐量变化 {round((t2-t1)/t1*100,1) if t1 else 0}%，"
                f"95% 响应时间变化 {round((p2-p1)/p1*100,1) if p1 else 0}%。"
            ),
        }

    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT.write_text(json.dumps(analysis, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"Analysis saved: {OUTPUT}")


if __name__ == "__main__":
    main()
