@echo off
setlocal enabledelayedexpansion
title Indestructible Revival - Installer
color 0B
cd /d "%~dp0"

echo ====================================================================
echo       INDESTRUCTIBLE REVIVAL - PC EMULATOR INSTALLER
echo ====================================================================
echo.
echo  Before you start:
echo   1. Open your Android emulator (LDPlayer 9 recommended) and wait
echo      until the Android home screen is fully loaded.
echo   2. Enable ADB debugging in your emulator settings:
echo      - LDPlayer 9: Menu ^> Settings ^> Other settings ^> ADB debugging
echo                    ^> "Open local connection" ^> Save.
echo      - BlueStacks: Settings ^> Advanced ^> enable "Android Debug Bridge".
echo.
pause

set "ADB=%~dp0tools\adb.exe"
set "APK=%~dp0com.glu.indestructible.standalone.apk"
set "OBB=%~dp0main.300.com.glu.indestructible.obb"
set "PKG=com.glu.indestructible"
set "OBBDIR=/sdcard/Android/obb/%PKG%"

for %%F in ("%ADB%" "%APK%" "%OBB%") do (
    if not exist "%%~F" (
        echo.
        echo [ERROR] Missing file: %%~F
        echo Make sure you extracted ALL files from the ZIP into a folder first.
        goto :Done
    )
)

:Search
echo.
echo [*] Searching for emulator...
"%ADB%" start-server >nul 2>&1

rem Common ADB ports: LDPlayer / BlueStacks (5555, 5557...), MuMu (7555),
rem Nox (62001), MEmu (21503)
for %%P in (5555 5557 5559 5561 5563 7555 62001 21503) do (
    "%ADB%" connect 127.0.0.1:%%P >nul 2>&1
)

set "DEV="
set "COUNT=0"
for /f "skip=1 tokens=1,2" %%A in ('call "%ADB%" devices') do (
    if "%%B"=="device" (
        set /a COUNT+=1
        if not defined DEV set "DEV=%%A"
    )
)

if not defined DEV (
    echo.
    echo [!] No emulator detected.
    echo.
    echo     Current ADB status:
    "%ADB%" devices
    echo.
    echo     Please check that:
    echo      - Your emulator is open and finished booting.
    echo      - ADB debugging is enabled ^(LDPlayer: Settings ^> Other settings
    echo        ^> ADB debugging ^> "Open local connection", then Save^).
    echo      - If a device shows "unauthorized", accept the prompt on screen.
    echo.
    echo     Press any key to try again, or close this window to cancel.
    pause >nul
    goto :Search
)

echo [OK] Emulator detected: !DEV!
if !COUNT! GTR 1 echo [i] !COUNT! devices found: installing on !DEV! only.
echo.

echo [1/3] Installing APK ^(your existing saves are kept^)...
"%ADB%" -s !DEV! install -r -g "%APK%"
if errorlevel 1 (
    echo     Retrying without automatic permission grant...
    "%ADB%" -s !DEV! install -r "%APK%"
    if errorlevel 1 (
        echo.
        echo [ERROR] APK installation failed. See the message above.
        goto :Done
    )
)

echo.
echo [2/3] Copying OBB game data ^(about 100 MB^)...
"%ADB%" -s !DEV! shell "mkdir -p %OBBDIR%"
"%ADB%" -s !DEV! push "%OBB%" "%OBBDIR%/main.300.%PKG%.obb"
if errorlevel 1 (
    echo.
    echo [ERROR] Copying the OBB file failed. See the message above.
    goto :Done
)

echo.
echo [3/3] Granting storage permissions...
"%ADB%" -s !DEV! shell "pm grant %PKG% android.permission.READ_EXTERNAL_STORAGE" >nul 2>&1
"%ADB%" -s !DEV! shell "pm grant %PKG% android.permission.WRITE_EXTERNAL_STORAGE" >nul 2>&1

echo.
echo ====================================================================
echo   [SUCCESS] Indestructible Revival is installed!
echo   Launch it from the "Indestructible" icon inside your emulator.
echo ====================================================================

:Done
echo.
pause
endlocal
