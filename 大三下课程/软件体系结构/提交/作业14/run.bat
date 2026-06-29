@echo off
chcp 65001 >nul
cd /d "%~dp0"
if not exist out mkdir out
javac -encoding UTF-8 -d out src\bridge\*.java src\Client.java
if errorlevel 1 (
    echo 编译失败
    pause
    exit /b 1
)
java -cp out Client
pause
