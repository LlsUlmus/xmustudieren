@echo off
setlocal

cd /d "%~dp0"
echo [INFO] Start ChatServer...

where java >nul 2>nul
if %errorlevel% neq 0 goto no_java

if not exist "deps\activemq-all-5.19.2.jar" goto dep_missing
if not exist "deps\log4j-api-2.20.0.jar" goto dep_missing
if not exist "deps\log4j-core-2.20.0.jar" goto dep_missing

set "PORT_PID="
for /f "tokens=5" %%i in ('netstat -ano ^| findstr ":8080" ^| findstr "LISTENING"') do (
  set "PORT_PID=%%i"
  goto port_checked
)

:port_checked
if defined PORT_PID goto port_busy

echo [1/2] Compile...
javac -proc:none -encoding UTF-8 -cp "deps/*" ChatServer.java
if %errorlevel% neq 0 goto compile_fail

echo [2/2] Run server at http://localhost:8080
java -cp ".;deps/*" ChatServer
echo.
echo Server exited. Press any key to close...
pause >nul
exit /b 0

:no_java
echo [ERROR] Java not found. Please install Java and set PATH.
pause
exit /b 1

:dep_missing
echo [ERROR] Missing jars in deps folder.
echo Required:
echo   deps\activemq-all-5.19.2.jar
echo   deps\log4j-api-2.20.0.jar
echo   deps\log4j-core-2.20.0.jar
pause
exit /b 1

:port_busy
echo [WARN] Port 8080 is busy (PID %PORT_PID%).
echo [TIP ] Server may already be running.
echo [TIP ] Use restart.bat to stop old process then start again.
pause
exit /b 0

:compile_fail
echo [ERROR] Compile failed.
pause
exit /b 1
