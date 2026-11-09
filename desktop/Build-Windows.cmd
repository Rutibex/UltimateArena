@echo off
cd /d "%~dp0"
where dotnet >nul 2>nul
if errorlevel 1 (
 echo Install the .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0
 pause
 exit /b 1
)
dotnet publish UltimateArena.csproj -c Release -r win-x64 --self-contained true -o artifacts\windows
if errorlevel 1 (
 pause
 exit /b 1
)
echo.
echo Built: artifacts\windows\UltimateArena.exe
pause
