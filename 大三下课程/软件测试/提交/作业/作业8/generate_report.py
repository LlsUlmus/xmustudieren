# -*- coding: utf-8 -*-
"""Generate homework PDF report for Assignment 8."""
from fpdf import FPDF
from pathlib import Path

BASE = Path(__file__).parent
FONT = Path(r"C:\Windows\Fonts\simhei.ttf")
OUTPUT = BASE / "11920232202561_卢思宇_作业8.pdf"


class ReportPDF(FPDF):
    def header(self):
        self.set_font("SimHei", "", 10)
        self.cell(0, 8, "软件测试 - 作业8：JMeter 系统性能测试", align="C", new_x="LMARGIN", new_y="NEXT")
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


def main():
    pdf = ReportPDF()
    pdf.add_font("SimHei", "", str(FONT))
    pdf.add_font("SimHei", "B", str(FONT))
    pdf.set_auto_page_break(auto=True, margin=15)
    pdf.add_page()

    pdf.section_title("一、作业目的")
    pdf.body_text(
        "本实验选用 Apache JMeter 对从 GitHub 拉取的 Spring Boot REST 示例项目进行系统性能测试，"
        "了解 JMeter 的主要功能与使用流程，分析被测接口在并发访问下的响应时间、吞吐量与错误率等指标。"
    )

    pdf.section_title("二、JMeter 主要功能介绍")
    pdf.sub_title("1. 工具概述")
    pdf.body_text(
        "Apache JMeter 是 Apache 基金会开源的性能测试工具，主要用于 Web 应用、REST API、数据库等系统的"
        "负载测试与压力测试，支持 Windows/Linux 多平台，采用 Java 开发。"
    )

    pdf.sub_title("2. 主要功能模块")
    pdf.bullet("测试计划管理：以 .jmx 文件组织测试场景，支持线程组、定时器、断言、监听器等组件。")
    pdf.bullet("HTTP/HTTPS 协议测试：模拟浏览器或客户端对 Web 接口发起 GET/POST 等请求。")
    pdf.bullet("并发模拟：通过线程组设置虚拟用户数、 ramp-up 时间、循环次数，模拟多用户并发访问。")
    pdf.bullet("断言与校验：对响应码、响应内容、响应时间等进行自动校验，判断测试是否通过。")
    pdf.bullet("结果监听与报告：提供聚合报告、察看结果树、图形结果等；支持命令行生成 HTML Dashboard 报告。")
    pdf.bullet("扩展能力：支持 JDBC、FTP、JMS 等协议；可通过插件扩展功能；支持分布式压测。")

    pdf.sub_title("3. 与 LoadRunner 对比（简述）")
    pdf.body_text(
        "LoadRunner 是 Micro Focus 商业性能测试工具，功能全面，支持多种协议与深入性能分析，但价格昂贵、"
        "部署复杂。JMeter 免费开源、上手快，适合教学实验与中小型项目性能验证。本实验选用 JMeter。"
    )

    pdf.add_page()
    pdf.section_title("三、被测系统说明")
    pdf.sub_title("1. 项目来源")
    pdf.body_text(
        "项目名称：gs-rest-service（Spring 官方指南示例）\n"
        "GitHub 地址：https://github.com/spring-guides/gs-rest-service\n"
        "本地路径：作业8/test-project/complete\n"
        "克隆命令：git clone --depth 1 https://github.com/spring-guides/gs-rest-service.git test-project"
    )

    pdf.sub_title("2. 系统功能")
    pdf.body_text(
        "该项目是一个基于 Spring Boot 的 REST 服务，核心接口为：\n"
        "  GET http://127.0.0.1:8080/greeting?name={name}\n"
        "返回 JSON 格式问候语，例如：{\"id\":1,\"content\":\"Hello, JMeter!\"}"
    )

    pdf.sub_title("3. 启动方式")
    pdf.body_text(
        "进入 test-project/complete 目录，执行：\n"
        "  Windows: gradlew.bat bootRun\n"
        "服务默认监听 8080 端口。"
    )

    pdf.section_title("四、测试方案设计")
    pdf.sub_title("1. 测试类型")
    pdf.body_text("系统测试中的性能/负载测试：验证 greeting 接口在并发条件下的稳定性与响应能力。")

    pdf.sub_title("2. 测试环境")
    pdf.body_text(
        "操作系统：Windows 10\n"
        "JDK：Java 17+\n"
        "JMeter 版本：5.6.3\n"
        "被测服务：Spring Boot 4.0.6 + Tomcat 11\n"
        "测试地址：http://127.0.0.1:8080/greeting?name=JMeter"
    )

    pdf.sub_title("3. 测试参数")
    pdf.body_text(
        "线程组配置：\n"
        "  - 并发用户数（线程数）：20\n"
        "  - Ramp-Up 时间：5 秒\n"
        "  - 循环次数：10\n"
        "  - 总请求数：20 x 10 = 200\n"
        "断言：响应体包含 \"Hello\"\n"
        "测试脚本：作业8/jmeter/rest-service-test.jmx"
    )

    pdf.sub_title("4. 执行命令")
    pdf.body_text(
        "set JMETER_HOME=E:\\apache-jmeter-5.6.3\\apache-jmeter-5.6.3\n"
        "jmeter -n -t rest-service-test.jmx -l results/test-results.jtl "
        "-e -o results/html-report"
    )

    pdf.add_page()
    pdf.section_title("五、测试结果与分析")
    pdf.sub_title("1. 汇总结果")
    pdf.body_text(
        "测试时间：2026年6月5日\n"
        "测试结论：全部 200 次请求成功，错误率 0%，系统在当前负载下运行稳定。"
    )

    pdf.set_font("SimHei", "B", 11)
    pdf.cell(45, 8, "指标", border=1)
    pdf.cell(45, 8, "数值", border=1, new_x="LMARGIN", new_y="NEXT")
    pdf.set_font("SimHei", "", 11)
    rows = [
        ("样本数", "200"),
        ("错误数", "0"),
        ("错误率", "0.00%"),
        ("平均响应时间", "2.97 ms"),
        ("中位数响应时间", "2 ms"),
        ("最小响应时间", "0 ms"),
        ("最大响应时间", "33 ms"),
        ("90% 响应时间", "5 ms"),
        ("95% 响应时间", "6 ms"),
        ("99% 响应时间", "12.96 ms"),
        ("吞吐量", "42.95 事务/秒"),
    ]
    for k, v in rows:
        pdf.cell(45, 8, k, border=1)
        pdf.cell(45, 8, v, border=1, new_x="LMARGIN", new_y="NEXT")
    pdf.ln(4)

    pdf.sub_title("2. 结果分析")
    pdf.bullet("平均响应时间约 3ms，99% 请求在 13ms 内完成，响应速度良好。")
    pdf.bullet("最大响应时间 33ms，出现在并发启动阶段，属于正常现象。")
    pdf.bullet("吞吐量约 43 TPS，对于简单 JSON 查询接口表现正常。")
    pdf.bullet("错误率为 0%，说明接口功能正确、服务稳定，满足本次系统测试预期。")
    pdf.bullet("详细 HTML 报告见：作业8/jmeter/results/html-report/index.html")

    pdf.section_title("六、测试步骤总结")
    pdf.body_text(
        "1. 从 GitHub 克隆 gs-rest-service 项目到作业8/test-project。\n"
        "2. 使用 Gradle 启动 complete 模块的 Spring Boot 服务。\n"
        "3. 编写 JMeter 测试计划，配置线程组、HTTP 请求与响应断言。\n"
        "4. 使用 JMeter 非 GUI 模式执行压测并生成 JTL 与 HTML 报告。\n"
        "5. 根据聚合数据与 Dashboard 报告分析性能指标并撰写实验报告。"
    )

    pdf.section_title("七、实验结论")
    pdf.body_text(
        "本次实验使用 JMeter 对 Spring Boot REST 服务完成了系统性能测试。被测 greeting 接口在 "
        "20 并发用户、共 200 次请求的场景下，响应时间短、吞吐稳定、无错误发生，系统功能与性能均达到预期。"
        "通过实验掌握了 JMeter 测试计划配置、命令行执行与结果分析方法，为后续 Web 系统性能测试打下基础。"
    )

    pdf.output(str(OUTPUT))
    print(f"PDF generated: {OUTPUT}")


if __name__ == "__main__":
    main()
