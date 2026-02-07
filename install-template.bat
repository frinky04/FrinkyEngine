@echo off
title Install FrinkyEngine Template
echo ========================================
echo   Install FrinkyEngine Game Template
echo ========================================
echo.

echo Installing template from templates\FrinkyEngine.Templates...
echo.
dotnet new install .\templates\FrinkyEngine.Templates --force

if errorlevel 1 (
    echo.
    echo [ERROR] Template installation failed.
    if not defined FRINKY_NO_PAUSE pause
    exit /b 1
)

echo.
echo Template installed. Create a new game project with:
echo   dotnet new frinky-game -n MyGame
if not defined FRINKY_NO_PAUSE pause
