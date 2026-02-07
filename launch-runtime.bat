@echo off
title FrinkyEngine Runtime
echo ========================================
echo   FrinkyEngine Runtime
echo ========================================
echo.

if "%~1"=="" (
    echo Usage: launch-runtime.bat path\to\Game.fproject
    echo.
    pause
    exit /b 1
)

echo Running project: %~1
echo.
dotnet run --project src\FrinkyEngine.Runtime -- "%~1"

if errorlevel 1 (
    echo.
    echo [ERROR] Runtime exited with errors.
    pause
)
