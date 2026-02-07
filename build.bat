@echo off
title FrinkyEngine Build
echo ========================================
echo   FrinkyEngine Build
echo ========================================
echo.

set CONFIG=Release
if /i "%~1"=="debug" set CONFIG=Debug

echo Building solution (%CONFIG%)...
echo.
dotnet build FrinkyEngine.sln -c %CONFIG%

if errorlevel 1 (
    echo.
    echo [ERROR] Build failed.
    pause
    exit /b 1
)

echo.
echo Build succeeded.
pause
