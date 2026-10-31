@echo off
chcp 65001 >nul
cd /d %~dp0
echo 启动服务器端 ...
start "能源监测-服务器" python server.py
timeout /t 2 >nul
echo 启动模拟数据端 ...
start "能源监测-模拟器" python simulator.py --start 07:00
timeout /t 3 >nul
start http://127.0.0.1:5000/
