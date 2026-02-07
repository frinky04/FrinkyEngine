@echo off
title FrinkyEngine Build
echo ========================================
echo   FrinkyEngine Build
echo ========================================
echo.

set CONFIG=Release
if /i "%~1"=="debug" set CONFIG=Debug

echo Restoring solution...
echo.
dotnet restore FrinkyEngine.sln

if errorlevel 1 (
    echo.
    echo [ERROR] Restore failed.
    if not defined FRINKY_NO_PAUSE pause
    exit /b 1
)

echo.
echo Building solution (%CONFIG%)...
echo.
dotnet build FrinkyEngine.sln -c %CONFIG% -warnaserror --no-restore

if errorlevel 1 (
    echo.
    echo [ERROR] Build failed.
    if not defined FRINKY_NO_PAUSE pause
    exit /b 1
)

echo.
echo Build succeeded.
if not defined FRINKY_NO_PAUSE pause
