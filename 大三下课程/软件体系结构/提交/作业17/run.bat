@echo off
chcp 65001 >nul
cd /d "%~dp0src"
if not exist Main.class (
    echo 请先运行 compile.bat 编译
    exit /b 1
)
java Main %*
