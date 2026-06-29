@echo off
chcp 65001 >nul
cd /d "%~dp0"
if not exist out mkdir out
javac -encoding UTF-8 -d out ^
  src\fs\Entry.java ^
  src\fs\FsFile.java ^
  src\fs\Directory.java ^
  src\fs\FileTreatmentException.java ^
  src\fs\FileSystemLoader.java ^
  src\fs\Main.java ^
  src\gui\EntryTreeModel.java ^
  src\gui\FileSystemGUI.java
if errorlevel 1 (
  echo.
  echo 编译失败。也可在项目根目录执行: powershell -File compile.ps1
  pause
  exit /b 1
)
java -cp out gui.FileSystemGUI
pause
