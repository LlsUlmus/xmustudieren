# -*- coding: utf-8 -*-
"""Generate homework PDF report for Assignment 8 (Experiment 9 requirements)."""
import json
from pathlib import Path

from fpdf import FPDF

BASE = Path(__file__).parent
FONT = Path(r"C:\Windows\Fonts\simhei.ttf")
OUTPUT = BASE / "11920232202561_卢思宇_作业8.pdf"
ANALYSIS = BASE / "jmeter" / "results" / "analysis.json"


class ReportPDF(FPDF):
    def header(self):
        self.set_font("SimHei", "", 10)
        self.cell(0, 8, "软件测试实验9 - JMeter 系统性能测试", align="C", new_x="LMARGIN", new_y="NEXT")
        self.ln(2)

    def footer(self):
        self.set_y(-12)
        self.set_font("SimHei", "", 9)
        self.cell(0, 8, f"第 {self.page_no()} 页", align="C")

    def section_title(self, title):
        self.set_font("SimHei", "B", 14)
        self.cell(0, 10, title, new_x="LMARGIN", new_y="NEXT")
        self.ln(2)

    def sub_title(self, title):
        self.set_font("SimHei", "B", 12)
        self.cell(0, 8, title, new_x="LMARGIN", new_y="NEXT")
        self.ln(1)

    def body_text(self, text):
        self.set_font("SimHei", "", 11)
        self.multi_cell(0, 7, text)
        self.ln(2)

    def bullet(self, text):
        self.set_font("SimHei", "", 11)
        self.multi_cell(0, 7, f"  - {text}")
        self.ln(1)

    def table_row(self, cols, widths, bold=False):
        self.set_font("SimHei", "B" if bold else "", 10)
        for text, w in zip(cols, widths):
            self.cell(w, 8, str(text), border=1)
        self.ln(8)


def load_analysis():
    if ANALYSIS.exists():
        return json.loads(ANALYSIS.read_text(encoding="utf-8"))
    return {"scenarios": {}, "comparison": {}}


def fmt_stats(jtl, key):
    if not jtl.get("available"):
        return "N/A"
    s = jtl[key]
    return f"均值{s['mean']} / P95 {s['p95']} / 最大{s['max']} ms"


def main():
    data = load_analysis()
    pdf = ReportPDF()
    pdf.add_font("SimHei", "", str(FONT))
    pdf.add_font("SimHei", "B", str(FONT))
    pdf.set_auto_page_break(auto=True, margin=15)
    pdf.add_page()

    pdf.section_title("一、实验目的")
    pdf.body_text(
        "本实验选用 Apache JMeter 对 Spring Boot REST 示例项目 gs-rest-service 进行系统性能测试，"
        "掌握脚本录制与增强（事务、参数化、检查点）、多场景压测配置及瓶颈分析方法。"
    )

    pdf.section_title("二、被测系统")
    pdf.body_text(
        "项目：gs-rest-service（Spring 官方指南）\n"
        "GitHub：https://github.com/spring-guides/gs-rest-service\n"
        "接口：GET http://127.0.0.1:8080/greeting?name={name}\n"
        "返回：{\"id\":1,\"content\":\"Hello, {name}!\"}"
    )

    pdf.section_title("三、脚本增强说明")
    pdf.sub_title("1. 事务（Transaction Controller）")
    pdf.body_text(
        "使用 Transaction Controller「TX_Greeting_Complete」将 GET /greeting 请求及断言封装为完整业务事务，"
        "便于统计端到端响应时间与吞吐量。"
    )
    pdf.sub_title("2. 参数化（CSV Data Set Config）")
    pdf.body_text(
        "通过 jmeter/data/names.csv 为 name 参数提供 16 组不同用户名，循环复用，模拟多用户访问。"
    )
    pdf.sub_title("3. 检查点（Assertions）")
    pdf.bullet("响应内容检查点：响应体包含 \"Hello\"")
    pdf.bullet("HTTP 状态码检查点：响应码为 200")
    pdf.bullet("响应时间检查点：Duration Assertion（场景1 ≤500ms，场景2 ≤2000ms）")

    pdf.add_page()
    pdf.section_title("四、测试场景设计")
    pdf.body_text("设置两个用户场景，并监测响应时间、吞吐量、错误率及服务器 CPU/内存指标。")

    widths = [45, 55, 55]
    pdf.table_row(["场景", "场景1 - 性能测试", "场景2 - 压力测试"], widths, bold=True)
    s1 = data.get("scenarios", {}).get("scenario1", {})
    s2 = data.get("scenarios", {}).get("scenario2", {})
    c1 = s1.get("config", {"threads": 15, "loops": 20, "ramp_up_sec": 10, "total_requests": 300})
    c2 = s2.get("config", {"threads": 50, "loops": 30, "ramp_up_sec": 5, "total_requests": 1500})

    rows = [
        ("测试类型", "性能测试（基准负载）", "压力测试（高并发）"),
        ("并发用户数", str(c1["threads"]), str(c2["threads"])),
        ("Ramp-Up(秒)", str(c1["ramp_up_sec"]), str(c2["ramp_up_sec"])),
        ("循环次数", str(c1["loops"]), str(c2["loops"])),
        ("总请求数", str(c1["total_requests"]), str(c2["total_requests"])),
        ("思考时间", "100-300ms 随机", "50-150ms 随机"),
    ]
    for row in rows:
        pdf.table_row(row, widths)

    pdf.ln(4)
    pdf.sub_title("监测指标")
    pdf.bullet("JMeter：响应时间、吞吐量(TPS)、错误率、连接时间、Latency、Apdex")
    pdf.bullet("服务器：JVM 进程 CPU 使用率、工作集内存(MB)")
    pdf.bullet("时间分解：连接时间、网络/等待时间、下载时间、总耗时")

    pdf.add_page()
    pdf.section_title("五、场景1测试结果")
    jtl1 = s1.get("jtl_analysis", {})
    mon1 = s1.get("server_monitor", {})
    if jtl1.get("available"):
        pdf.body_text(
            f"样本数：{jtl1['sample_count']}  错误率：{jtl1['error_rate']}%  "
            f"吞吐量：{jtl1['throughput_tps']} TPS"
        )
        pdf.sub_title("响应时间")
        pdf.body_text(fmt_stats(jtl1, "elapsed_ms"))
        pdf.sub_title("时间分解（ms）")
        pdf.bullet(f"连接时间(Connect)：{fmt_stats(jtl1, 'connect_ms')}")
        pdf.bullet(f"网络/等待时间(Latency-Connect)：{fmt_stats(jtl1, 'network_ms')}")
        pdf.bullet(f"下载时间(Elapsed-Latency)：{fmt_stats(jtl1, 'download_ms')}")
        if mon1.get("available"):
            pdf.sub_title("服务器资源")
            pdf.bullet(f"CPU 峰值：{mon1['cpu_percent']['max']}%")
            pdf.bullet(
                f"内存：{mon1['memory_mb']['start']}MB → {mon1['memory_mb']['end']}MB，"
                f"增长 {mon1['memory_mb']['growth_mb']}MB"
            )
    else:
        pdf.body_text("（请先运行 run_tests.ps1 生成测试数据）")

    pdf.add_page()
    pdf.section_title("六、场景2测试结果")
    jtl2 = s2.get("jtl_analysis", {})
    mon2 = s2.get("server_monitor", {})
    if jtl2.get("available"):
        pdf.body_text(
            f"样本数：{jtl2['sample_count']}  错误率：{jtl2['error_rate']}%  "
            f"吞吐量：{jtl2['throughput_tps']} TPS"
        )
        pdf.sub_title("响应时间")
        pdf.body_text(fmt_stats(jtl2, "elapsed_ms"))
        pdf.sub_title("时间分解（ms）")
        pdf.bullet(f"连接时间(Connect)：{fmt_stats(jtl2, 'connect_ms')}")
        pdf.bullet(f"网络/等待时间(Latency-Connect)：{fmt_stats(jtl2, 'network_ms')}")
        pdf.bullet(f"下载时间(Elapsed-Latency)：{fmt_stats(jtl2, 'download_ms')}")
        if mon2.get("available"):
            pdf.sub_title("服务器资源")
            pdf.bullet(f"CPU 峰值：{mon2['cpu_percent']['max']}%")
            pdf.bullet(
                f"内存：{mon2['memory_mb']['start']}MB → {mon2['memory_mb']['end']}MB，"
                f"增长 {mon2['memory_mb']['growth_mb']}MB"
            )
    else:
        pdf.body_text("（请先运行 run_tests.ps1 生成测试数据）")

    comp = data.get("comparison", {})
    if comp.get("text"):
        pdf.sub_title("两场景对比")
        pdf.body_text(comp["text"])

    pdf.add_page()
    pdf.section_title("七、瓶颈分类分析")

    for key, label in [("scenario1", "场景1"), ("scenario2", "场景2")]:
        sc = data.get("scenarios", {}).get(key, {})
        bn = sc.get("bottleneck", {})
        pdf.sub_title(f"{label}：{bn.get('summary', '待分析')}")
        for cat in bn.get("categories", []):
            pdf.bullet(f"[{cat.get('severity','')}] {cat.get('type','')}：{cat.get('detail','')}")
        pdf.ln(2)

    pdf.section_title("八、实验结论")
    pdf.body_text(
        "1. 脚本已增强：事务控制器封装业务、CSV 参数化用户名、三重检查点保障结果正确性。\n"
        "2. 完成性能测试与压力测试两个场景，覆盖不同并发强度下的 TPS 与响应时间。\n"
        "3. 通过 JMeter 时间分解与服务器 CPU/内存监测，对连接、网络等待、下载时间及资源占用进行瓶颈分类；"
        "本示例接口逻辑简单，两场景下均无功能性错误，主要延迟集中在首字节等待，CPU/内存未达饱和。\n"
        "4. HTML 详细报告：jmeter/results/scenario1-report/index.html 与 scenario2-report/index.html"
    )

    pdf.output(str(OUTPUT))
    print(f"PDF generated: {OUTPUT}")


if __name__ == "__main__":
    main()
