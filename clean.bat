@echo off
title FrinkyEngine Clean
echo ========================================
echo   FrinkyEngine Clean
echo ========================================
echo.

echo Cleaning solution...
dotnet clean FrinkyEngine.sln -c Debug >nul 2>&1
dotnet clean FrinkyEngine.sln -c Release >nul 2>&1

echo Removing bin/obj folders...
for /d /r src %%d in (bin obj) do (
    if exist "%%d" rd /s /q "%%d"
)

if exist artifacts (
    echo Removing artifacts folder...
    rd /s /q artifacts
)

echo.
echo Clean complete.
pause
