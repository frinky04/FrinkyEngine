@echo off
title Package FrinkyEngine Game
echo ========================================
echo   Package FrinkyEngine Game
echo ========================================
echo.

:: Usage: package-game.bat path\to\Game.fproject [output-folder]

if "%~1"=="" (
    echo Usage: package-game.bat path\to\Game.fproject [output-folder]
    echo.
    echo   Packages a game project into a standalone folder with the runtime,
    echo   game assembly, scenes, and assets ready to distribute.
    echo.
    pause
    exit /b 1
)

set FPROJECT=%~1
set FPROJECT_DIR=%~dp1
set FPROJECT_NAME=%~n1

:: Output folder defaults to artifacts\GameName
if "%~2"=="" (
    set OUTDIR=artifacts\%FPROJECT_NAME%
) else (
    set OUTDIR=%~2
)

echo Project file:  %FPROJECT%
echo Project dir:   %FPROJECT_DIR%
echo Output folder: %OUTDIR%
echo.

:: Step 1: Build the game assembly
echo [1/4] Building game project...
pushd "%FPROJECT_DIR%"
dotnet build -c Release
if errorlevel 1 (
    popd
    echo [ERROR] Game project build failed.
    pause
    exit /b 1
)
popd
echo       Done.
echo.

:: Step 2: Publish the runtime (WinExe mode, no console window)
echo [2/4] Publishing runtime...
dotnet publish src\FrinkyEngine.Runtime\FrinkyEngine.Runtime.csproj ^
    -c Release ^
    -p:FrinkyExport=true ^
    -o "%OUTDIR%" ^
    --self-contained false
if errorlevel 1 (
    echo [ERROR] Runtime publish failed.
    pause
    exit /b 1
)
echo       Done.
echo.

:: Step 3: Copy game files (assembly, assets, scenes, .fproject)
echo [3/4] Copying game files...

:: Copy the .fproject file
copy /y "%FPROJECT%" "%OUTDIR%\" >nul

:: Copy game assembly (Release build)
set GAME_BIN=%FPROJECT_DIR%bin\Release\net8.0
if exist "%GAME_BIN%\%FPROJECT_NAME%.dll" (
    copy /y "%GAME_BIN%\%FPROJECT_NAME%.dll" "%OUTDIR%\" >nul
    if exist "%GAME_BIN%\%FPROJECT_NAME%.pdb" copy /y "%GAME_BIN%\%FPROJECT_NAME%.pdb" "%OUTDIR%\" >nul
    echo       Copied game assembly.
) else (
    echo       [WARN] Game assembly not found at %GAME_BIN%\%FPROJECT_NAME%.dll
)

:: Copy Assets folder
if exist "%FPROJECT_DIR%Assets" (
    xcopy /e /i /y /q "%FPROJECT_DIR%Assets" "%OUTDIR%\Assets" >nul
    echo       Copied Assets.
)

:: Copy Scenes folder
if exist "%FPROJECT_DIR%Scenes" (
    xcopy /e /i /y /q "%FPROJECT_DIR%Scenes" "%OUTDIR%\Scenes" >nul
    echo       Copied Scenes.
) else (
    :: Some projects might store scenes in Assets
    echo       No Scenes folder found (scenes may be in Assets).
)

echo       Done.
echo.

:: Step 4: Create a launcher bat
echo [4/4] Creating launcher...
set LAUNCHER=%OUTDIR%\Play.bat
echo @echo off > "%LAUNCHER%"
echo start "" "FrinkyEngine.Runtime.exe" "%FPROJECT_NAME%.fproject" >> "%LAUNCHER%"
echo       Created Play.bat
echo.

echo ========================================
echo   Package complete: %OUTDIR%
echo ========================================
echo.
echo To play: run %OUTDIR%\Play.bat
echo   or: %OUTDIR%\FrinkyEngine.Runtime.exe %FPROJECT_NAME%.fproject
echo.
echo NOTE: The target machine needs .NET 8 runtime installed.
echo       For a fully self-contained build, re-run with --self-contained.
pause
