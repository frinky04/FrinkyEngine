@echo off
setlocal
title FrinkyEngine Local Release
echo ========================================
echo   FrinkyEngine Local Release
echo ========================================
echo.

:: Usage: release-local.bat vX.Y.Z
if "%~1"=="" (
    echo Usage: release-local.bat vX.Y.Z ^| --patch
    echo.
    echo Example:
    echo   release-local.bat v0.1.0
    echo   release-local.bat --patch
    echo.
    if not defined FRINKY_NO_PAUSE pause
    exit /b 1
)

if /i "%~1"=="--patch" (
    echo Resolving next patch version from git tags...
    git fetch --tags --quiet >nul 2>&1

    for /f %%i in ('powershell -NoProfile -ExecutionPolicy Bypass -Command ^
      "$tags = git tag --list 'v*';" ^
      "$versions = @();" ^
      "foreach ($t in $tags) {" ^
      "  if ($t -match '^v(?<maj>\d+)\.(?<min>\d+)\.(?<pat>\d+)$') {" ^
      "    $versions += [pscustomobject]@{ Maj = [int]$Matches['maj']; Min = [int]$Matches['min']; Pat = [int]$Matches['pat'] };" ^
      "  }" ^
      "}" ^
      "if ($versions.Count -eq 0) { Write-Output 'v0.1.0'; exit 0 }" ^
      "$latest = $versions | Sort-Object Maj, Min, Pat | Select-Object -Last 1;" ^
      "Write-Output ('v{0}.{1}.{2}' -f $latest.Maj, $latest.Min, ($latest.Pat + 1));"') do (
        set TAG=%%i
    )

    if not defined TAG (
        echo.
        echo [ERROR] Could not resolve next patch version from tags.
        if not defined FRINKY_NO_PAUSE pause
        exit /b 1
    )

    call echo Resolved tag: %%TAG%%
) else (
    set TAG=%~1
)

set VERSION=%TAG:v=%
set RID=win-x64
set FRINKY_NO_PAUSE_PREV=%FRINKY_NO_PAUSE%

echo Validating tag format...
powershell -NoProfile -ExecutionPolicy Bypass -Command "if ('%TAG%' -match '^v\d+\.\d+\.\d+$') { exit 0 } else { exit 1 }"
if errorlevel 1 (
    echo.
    echo [ERROR] Tag must match vMAJOR.MINOR.PATCH ^(for example v1.2.3^)
    if not defined FRINKY_NO_PAUSE pause
    exit /b 1
)

echo.
echo [1/4] Building solution with warnings as errors...
dotnet restore FrinkyEngine.sln
if errorlevel 1 (
    echo [ERROR] Restore failed.
    if not defined FRINKY_NO_PAUSE pause
    exit /b 1
)

dotnet build FrinkyEngine.sln -c Release -warnaserror --no-restore
if errorlevel 1 (
    echo [ERROR] Build failed.
    if not defined FRINKY_NO_PAUSE pause
    exit /b 1
)

echo.
echo [2/4] Publishing Editor and Runtime...
set FRINKY_NO_PAUSE=1
call publish-editor.bat %RID% artifacts\release\editor\%RID%
if errorlevel 1 exit /b 1

call publish-runtime.bat %RID% artifacts\release\runtime\%RID%
if errorlevel 1 exit /b 1
set FRINKY_NO_PAUSE=%FRINKY_NO_PAUSE_PREV%

echo.
echo [3/4] Packing template...
dotnet pack templates\FrinkyEngine.Templates\FrinkyEngine.Templates.csproj -c Release -o artifacts\templates
if errorlevel 1 (
    echo [ERROR] Template pack failed.
    if not defined FRINKY_NO_PAUSE pause
    exit /b 1
)

echo.
echo [4/4] Creating release zip files...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$ErrorActionPreference='Stop';" ^
  "$version='%VERSION%';" ^
  "$editorDir='artifacts/release/editor/%RID%';" ^
  "$runtimeDir='artifacts/release/runtime/%RID%';" ^
  "$editorStage='artifacts/release/FrinkyEngine-Editor-%RID%-v' + $version;" ^
  "$runtimeStage='artifacts/release/FrinkyEngine-Runtime-%RID%-v' + $version;" ^
  "New-Item -ItemType Directory -Force -Path $editorStage | Out-Null;" ^
  "New-Item -ItemType Directory -Force -Path $runtimeStage | Out-Null;" ^
  "Copy-Item -Path ($editorDir + '/*') -Destination $editorStage -Recurse -Force;" ^
  "Copy-Item -Path ($runtimeDir + '/*') -Destination $runtimeStage -Recurse -Force;" ^
  "$editorZip='artifacts/release/FrinkyEngine-Editor-%RID%-v' + $version + '.zip';" ^
  "$runtimeZip='artifacts/release/FrinkyEngine-Runtime-%RID%-v' + $version + '.zip';" ^
  "if (Test-Path $editorZip) { Remove-Item $editorZip -Force };" ^
  "if (Test-Path $runtimeZip) { Remove-Item $runtimeZip -Force };" ^
  "Compress-Archive -Path ($editorStage + '/*') -DestinationPath $editorZip -CompressionLevel Optimal;" ^
  "Compress-Archive -Path ($runtimeStage + '/*') -DestinationPath $runtimeZip -CompressionLevel Optimal;"

if errorlevel 1 (
    echo [ERROR] Failed to create zip files.
    if not defined FRINKY_NO_PAUSE pause
    exit /b 1
)

echo.
echo Local release artifacts ready:
echo   artifacts\release\FrinkyEngine-Editor-%RID%-v%VERSION%.zip
echo   artifacts\release\FrinkyEngine-Runtime-%RID%-v%VERSION%.zip
echo.
echo Next step:
echo   git tag -a %TAG% -m "FrinkyEngine %TAG%"
echo   git push origin %TAG%
if not defined FRINKY_NO_PAUSE pause
endlocal
