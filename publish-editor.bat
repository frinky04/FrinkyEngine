@echo off
title Publish FrinkyEngine Editor
echo ========================================
echo   Publish FrinkyEngine Editor
echo ========================================
echo.

set RID=win-x64
if not "%~1"=="" set RID=%~1

set PUBLISH_DIR=artifacts\release\editor\%RID%
if not "%~2"=="" set PUBLISH_DIR=%~2

echo Restoring Editor project...
echo.
dotnet restore src\FrinkyEngine.Editor\FrinkyEngine.Editor.csproj -r %RID%

if errorlevel 1 (
    echo.
    echo [ERROR] Restore failed.
    if not defined FRINKY_NO_PAUSE pause
    exit /b 1
)

echo.
echo Publishing Editor (%RID%) to %PUBLISH_DIR%...
echo.

dotnet publish src\FrinkyEngine.Editor\FrinkyEngine.Editor.csproj ^
    -c Release ^
    -r %RID% ^
    -o "%PUBLISH_DIR%" ^
    --self-contained false ^
    -warnaserror ^
    --no-restore

if errorlevel 1 (
    echo.
    echo [ERROR] Publish failed.
    if not defined FRINKY_NO_PAUSE pause
    exit /b 1
)

echo.
echo Editor published to: %PUBLISH_DIR%
echo Run with: %PUBLISH_DIR%\FrinkyEngine.Editor.exe [optional .fproject path]
if not defined FRINKY_NO_PAUSE pause
