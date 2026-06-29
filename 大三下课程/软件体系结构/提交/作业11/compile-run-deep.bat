@echo off
chcp 65001 >nul
cd /d "%~dp0"
if not exist "out\production\作业11" mkdir "out\production\作业11"
javac -encoding UTF-8 -d "out\production\作业11" src\hw11\EquipmentTag.java src\hw11\ExperimentSlot.java src\hw11\DeepCloneDemo.java
if errorlevel 1 (
  echo 编译失败，请确认已安装 JDK 并已加入 PATH。
  pause
  exit /b 1
)
echo 运行深拷贝演示...
java -cp "out\production\作业11" hw11.DeepCloneDemo
pause
