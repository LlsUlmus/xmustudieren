@echo off
chcp 65001 >nul
cd /d "%~dp0"
if not exist out mkdir out
javac -encoding UTF-8 -d out src\ui\Button.java src\ui\TextField.java src\ui\GUIFactory.java src\ui\Application.java src\ui\windows\*.java src\ui\mac\*.java src\ui\linux\*.java src\factory\GUIFactoryProvider.java src\Client.java
copy /Y resources\gui.properties out\ >nul
java -Dfile.encoding=UTF-8 -cp out Client
pause
