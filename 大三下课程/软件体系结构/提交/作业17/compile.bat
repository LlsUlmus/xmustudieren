@echo off
chcp 65001 >nul
cd /d "%~dp0src"
echo 正在编译...
javac model/RandomNumberModel.java observer/DigitDisplayObserver.java observer/BarChartObserver.java observer/StatisticsObserver.java controller/RandomAppController.java Main.java
if errorlevel 1 exit /b 1
echo 编译成功。运行: run.bat  或  cd src ^&^& java Main
exit /b 0
