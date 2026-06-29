# 实验9：JMeter 系统性能测试（exp9）

## 目录结构

```
exp9/
├── service/                   # 被测 Spring Boot REST 服务
├── jmeter/
│   ├── data/names.csv         # 参数化数据
│   ├── scenario1-performance.jmx
│   ├── scenario2-stress.jmx
│   └── results/               # JTL、HTML 报告、analysis.json
├── run_tests.ps1              # 一键运行
├── analyze_results.py
├── generate_report.py
├── 实验报告.md
└── 实验报告.pdf               # python generate_report.py 生成
```

## 启动被测服务

```powershell
cd service
.\gradlew.bat bootRun
```

## 运行测试

```powershell
cd exp9
.\run_tests.ps1
```
