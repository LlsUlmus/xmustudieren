@echo off
chcp 65001 >nul
cd /d "%~dp0"
echo ========================================
echo  BlackBox 手工测试演示（5组典型用例）
echo ========================================
echo.

call :run_case "2021 7 22"   "正常用例"
call :run_case "1899 1 1"    "年份非法"
call :run_case "2021 4 31"   "日期不存在"
call :run_case "2021 12 31"  "跨年"
call :run_case "2020 2 29"   "闰年2月"

echo ========================================
echo  演示结束，请对上述输出截图
echo ========================================
pause
goto :eof

:run_case
echo ---------- %~2 : 输入 %~1 ----------
(echo %~1) | java blackbox.BlackBox 2>nul | findstr /v "请输入"
echo.
goto :eof
