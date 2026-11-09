@echo off
cd /d "%~dp0"
echo Host address: http://YOUR-LAN-IP:8080
echo Use JOIN HOST on that address. Close this window to stop hosting.
node server.js --bind 0.0.0.0
pause
