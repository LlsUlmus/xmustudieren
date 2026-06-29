@echo off
chcp 65001 >nul
cd /d "%~dp0"
echo Compiling...
javac Singleton.java SingletonTest.java
if errorlevel 1 (
  echo javac failed. Install JDK and add bin to PATH.
  pause
  exit /b 1
)
echo Running SingletonTest...
echo.
java SingletonTest
echo.
pause
