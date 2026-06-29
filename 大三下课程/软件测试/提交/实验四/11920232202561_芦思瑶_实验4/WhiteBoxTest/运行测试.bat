@echo off
chcp 65001 >nul
cd /d "%~dp0"
echo 运行 JUnit 白盒测试...
mvn test
pause
