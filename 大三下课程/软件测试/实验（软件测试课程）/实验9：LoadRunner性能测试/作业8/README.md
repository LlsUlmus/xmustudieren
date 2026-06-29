# 实验9：JMeter 系统性能测试

## 目录结构

```
作业8/
├── test-project/              # 被测项目 gs-rest-service
├── jmeter/
│   ├── data/names.csv         # 参数化数据
│   ├── scenario1-performance.jmx  # 场景1：性能测试
│   ├── scenario2-stress.jmx       # 场景2：压力测试
│   └── results/
│       ├── scenario1-results.jtl / scenario1-report/
│       ├── scenario2-results.jtl / scenario2-report/
│       └── analysis.json      # 瓶颈分析数据
├── run_tests.ps1              # 一键运行两场景 + 分析 + 生成 PDF
├── analyze_results.py         # JTL 与时间/资源瓶颈分析
├── generate_report.py         # 生成 PDF 实验报告
└── README.md
```

## 实验要求对照

| 要求 | 实现 |
|------|------|
| 事务设置 | Transaction Controller `TX_Greeting_Complete` |
| 参数化 | CSV Data Set Config + `data/names.csv` |
| 检查点 | 响应内容、HTTP 200、响应时间 Duration Assertion |
| 两个用户场景 | 场景1 性能测试(15×20)、场景2 压力测试(50×30) |
| 性能监测指标 | TPS、响应时间、错误率、CPU、内存、时间分解 |
| 瓶颈分类 | `analyze_results.py` 输出连接/网络/下载/CPU/内存/稳定性分类 |

## 启动被测服务

```powershell
cd test-project\complete
.\gradlew.bat bootRun
```

## 运行全部测试（推荐）

```powershell
cd 作业8
.\run_tests.ps1
```

## 手动运行单个场景

```powershell
$env:JMETER_HOME = "E:\apache-jmeter-5.6.3\apache-jmeter-5.6.3"
cd jmeter
& "$env:JMETER_HOME\bin\jmeter.bat" -n -t scenario1-performance.jmx `
  -l results\scenario1-results.jtl -e -o results\scenario1-report
```

## 查看报告

- PDF：`11920232202561_卢思宇_作业8.pdf`（运行 `python generate_report.py` 生成）
- HTML：`jmeter/results/scenario1-report/index.html`、`scenario2-report/index.html`
