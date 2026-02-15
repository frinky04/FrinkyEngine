@echo off
title FrinkyEngine — Update & Launch
echo ========================================
echo   FrinkyEngine — Update ^& Launch
echo ========================================
echo.

echo Pulling latest changes...
git pull
if errorlevel 1 (
    echo.
    echo [ERROR] Git pull failed.
    if not defined FRINKY_NO_PAUSE pause
    exit /b 1
)
echo.

echo Building...
dotnet build FrinkyEngine.sln --verbosity quiet
if errorlevel 1 (
    echo.
    echo [ERROR] Build failed.
    if not defined FRINKY_NO_PAUSE pause
    exit /b 1
)
echo.

:: Optional: pass a .fproject path as argument
if "%~1"=="" (
    echo Launching editor ^(no project^)...
    echo.
    dotnet run --project src\FrinkyEngine.Editor --no-build
) else (
    echo Launching editor with project: %~1
    echo.
    dotnet run --project src\FrinkyEngine.Editor --no-build -- "%~1"
)

if errorlevel 1 (
    echo.
    echo [ERROR] Editor exited with errors.
    if not defined FRINKY_NO_PAUSE pause
)
