@echo off
cd /d "%~dp0"
where node >nul 2>nul
if errorlevel 1 (
 echo Install Node.js 22 or newer from https://nodejs.org then run this again.
 pause
 exit /b 1
)
start "Arena browser" cmd /c "timeout /t 2 /nobreak >nul & start http://localhost:8080"
node server.js
pause
