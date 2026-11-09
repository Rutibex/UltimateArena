@echo off
cd /d "%~dp0"
dotnet run --project UltimateArena.csproj -c Release -- --self-test
pause
