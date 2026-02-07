@echo off
title FrinkyEngine Editor
echo ========================================
echo   FrinkyEngine Editor
echo ========================================
echo.

:: Optional: pass a .fproject path as argument
if "%~1"=="" (
    echo Launching editor (no project)...
    echo.
    dotnet run --project src\FrinkyEngine.Editor
) else (
    echo Launching editor with project: %~1
    echo.
    dotnet run --project src\FrinkyEngine.Editor -- "%~1"
)

if errorlevel 1 (
    echo.
    echo [ERROR] Editor exited with errors.
    pause
)
