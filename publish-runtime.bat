@echo off
title Publish FrinkyEngine Runtime
echo ========================================
echo   Publish FrinkyEngine Runtime
echo ========================================
echo.

set OUTDIR=artifacts\Runtime
echo Publishing Runtime to %OUTDIR%...
echo.

dotnet publish src\FrinkyEngine.Runtime\FrinkyEngine.Runtime.csproj ^
    -c Release ^
    -p:FrinkyExport=true ^
    -o "%OUTDIR%" ^
    --self-contained false

if errorlevel 1 (
    echo.
    echo [ERROR] Publish failed.
    pause
    exit /b 1
)

echo.
echo Runtime published to: %OUTDIR%
echo Run with: %OUTDIR%\FrinkyEngine.Runtime.exe path\to\Game.fproject
pause
