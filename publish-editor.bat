@echo off
title Publish FrinkyEngine Editor
echo ========================================
echo   Publish FrinkyEngine Editor
echo ========================================
echo.

set OUTDIR=artifacts\Editor
echo Publishing Editor to %OUTDIR%...
echo.

dotnet publish src\FrinkyEngine.Editor\FrinkyEngine.Editor.csproj ^
    -c Release ^
    -o "%OUTDIR%" ^
    --self-contained false

if errorlevel 1 (
    echo.
    echo [ERROR] Publish failed.
    pause
    exit /b 1
)

echo.
echo Editor published to: %OUTDIR%
echo Run with: %OUTDIR%\FrinkyEngine.Editor.exe [optional .fproject path]
pause
