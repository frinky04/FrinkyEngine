@echo off
setlocal
title Generate API Documentation
echo ========================================
echo   Generate API Documentation
echo ========================================
echo.

echo [1/3] Restoring dotnet tools...
dotnet tool restore
if errorlevel 1 (
    echo [ERROR] Tool restore failed.
    if not defined FRINKY_NO_PAUSE pause
    exit /b 1
)

echo.
echo [2/3] Publishing FrinkyEngine.Core in Release...
dotnet publish src\FrinkyEngine.Core\FrinkyEngine.Core.csproj -c Release -o artifacts\apidoc-stage --no-build
if errorlevel 1 (
    :: Fall back to publish with build if --no-build fails (standalone run)
    dotnet publish src\FrinkyEngine.Core\FrinkyEngine.Core.csproj -c Release -o artifacts\apidoc-stage
    if errorlevel 1 (
        echo [ERROR] Publish failed.
        if not defined FRINKY_NO_PAUSE pause
        exit /b 1
    )
)

echo.
echo [3/3] Generating API docs...
if exist docs\api rmdir /s /q docs\api
dotnet xmldoc2md artifacts\apidoc-stage\FrinkyEngine.Core.dll -o docs\api --github-pages
if errorlevel 1 (
    echo [ERROR] xmldoc2md failed.
    if not defined FRINKY_NO_PAUSE pause
    exit /b 1
)

:: Clean up staging directory
rmdir /s /q artifacts\apidoc-stage >nul 2>&1

echo.
echo API documentation generated in docs\api\
if not defined FRINKY_NO_PAUSE pause
endlocal
