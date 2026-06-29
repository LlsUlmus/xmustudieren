@echo off
setlocal
set MINGW=C:\msys64\ucrt64
set PATH=%MINGW%\bin;%PATH%

echo Building exp07_task1...
g++ -O2 -std=c++17 exp07_task1.cpp -o exp07_task1.exe -I%MINGW%\include -L%MINGW%\lib -lfreeglut -lopengl32 -lglu32
if errorlevel 1 goto :fail

echo Building exp07_task2...
g++ -O2 -std=c++17 exp07_task2.cpp -o exp07_task2.exe -I%MINGW%\include -L%MINGW%\lib -lfreeglut -lopengl32 -lglu32
if errorlevel 1 goto :fail

copy /Y "%MINGW%\bin\libfreeglut.dll" .
echo.
echo Build succeeded.
echo   exp07_task1.exe - Bezier curve
echo   exp07_task2.exe - Bezier surface
goto :end

:fail
echo Build failed.
exit /b 1

:end
endlocal
