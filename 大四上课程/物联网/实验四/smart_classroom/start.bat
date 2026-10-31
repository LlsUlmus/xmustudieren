@echo off
cd /d %~dp0
start "device_simulator" cmd /k D:\Miniforge3\python.exe device_simulator.py
start "server" cmd /k D:\Miniforge3\python.exe server.py
timeout /t 3 >nul
start http://127.0.0.1:5000
