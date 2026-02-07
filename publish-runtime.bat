@echo off
title Publish FrinkyEngine Runtime
echo ========================================
echo   Publish FrinkyEngine Runtime
echo ========================================
echo.

set RID=win-x64
if not "%~1"=="" set RID=%~1

set PUBLISH_DIR=artifacts\release\runtime\%RID%
if not "%~2"=="" set PUBLISH_DIR=%~2

echo Restoring solution...
echo.
dotnet restore FrinkyEngine.sln -r %RID%

if errorlevel 1 (
    echo.
    echo [ERROR] Restore failed.
    if not defined FRINKY_NO_PAUSE pause
    exit /b 1
)

echo.
echo Publishing Runtime (%RID%) to %PUBLISH_DIR%...
echo.

dotnet publish src\FrinkyEngine.Runtime\FrinkyEngine.Runtime.csproj ^
    -c Release ^
    -r %RID% ^
    -p:FrinkyExport=true ^
    -o "%PUBLISH_DIR%" ^
    --self-contained false ^
    -warnaserror

if errorlevel 1 (
    echo.
    echo [ERROR] Publish failed.
    if not defined FRINKY_NO_PAUSE pause
    exit /b 1
)

echo.
echo Runtime published to: %PUBLISH_DIR%
echo Run with: %PUBLISH_DIR%\FrinkyEngine.Runtime.exe path\to\Game.fproject
if not defined FRINKY_NO_PAUSE pause
