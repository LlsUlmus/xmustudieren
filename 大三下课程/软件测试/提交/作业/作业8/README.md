# 作业8：JMeter 系统性能测试

## 目录结构

```
作业8/
├── test-project/          # 从 GitHub 克隆的被测项目 (gs-rest-service)
├── jmeter/
│   ├── rest-service-test.jmx   # JMeter 测试计划
│   └── results/
│       ├── test-results.jtl    # 原始测试结果
│       └── html-report/        # HTML 可视化报告
├── generate_report.py     # PDF 报告生成脚本
├── 11920232202561_卢思宇_作业8.pdf  # 提交用作业文档
└── README.md
```

## 被测项目

- **仓库**：https://github.com/spring-guides/gs-rest-service
- **使用模块**：`test-project/complete`
- **接口**：`GET http://127.0.0.1:8080/greeting?name=JMeter`

## 启动被测服务

```powershell
cd test-project\complete
.\gradlew.bat bootRun
```

## 运行 JMeter 测试

```powershell
$env:JMETER_HOME = "E:\apache-jmeter-5.6.3\apache-jmeter-5.6.3"
& "$env:JMETER_HOME\bin\jmeter.bat" -n `
  -t jmeter\rest-service-test.jmx `
  -l jmeter\results\test-results.jtl `
  -e -o jmeter\results\html-report
```

## 查看报告

- PDF 作业文档：`11920232202561_卢思宇_作业8.pdf`
- HTML 详细报告：`jmeter/results/html-report/index.html`
