@echo off
chcp 65001 >nul
cd /d "%~dp0"
echo ========================================
echo  spring-chat 后端（端口 8080）
echo ========================================
echo.
if "%MINIMAX_API_KEY%"=="" (
  echo [提示] 未设置环境变量 MINIMAX_API_KEY。
  echo        若 yml 中未写密钥，请在命令行先执行：set MINIMAX_API_KEY=你的密钥
  echo.
)
java -version >nul 2>&1
if errorlevel 1 (
  echo [错误] 未找到 java，请先安装 JDK 17+ 并配置 PATH。
  pause
  exit /b 1
)
echo 正在启动：java -jar spring-chat-1.0.0.jar
echo 浏览器可访问 http://127.0.0.1:8080/api/health 检查服务
echo.
java -jar "%~dp0spring-chat-1.0.0.jar"
pause
