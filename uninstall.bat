@echo off
echo ========================================
echo    Task Logger - Uninstallation Script
echo ========================================
echo.

set INSTALL_DIR=%USERPROFILE%\TaskLogger
set DESKTOP_DIR=%USERPROFILE%\Desktop
set STARTMENU_DIR=%APPDATA%\Microsoft\Windows\Start Menu\Programs

echo This will remove Task Logger from your system.
echo.
echo Files to be removed:
echo   - Installation directory: %INSTALL_DIR%
echo   - Desktop shortcut: Task Logger
echo   - Start menu shortcut: Task Logger
echo.
echo NOTE: Your task data will be preserved in the database file.
echo.
set /p CONFIRM="Are you sure you want to uninstall? (Y/N): "

if /i not "%CONFIRM%"=="Y" (
    echo Uninstallation cancelled.
    pause
    exit /b
)

echo.
echo Uninstalling Task Logger...

REM Remove desktop shortcut
if exist "%DESKTOP_DIR%\Task Logger.lnk" (
    echo Removing desktop shortcut...
    del "%DESKTOP_DIR%\Task Logger.lnk"
)

REM Remove start menu shortcut
if exist "%STARTMENU_DIR%\Task Logger.lnk" (
    echo Removing start menu shortcut...
    del "%STARTMENU_DIR%\Task Logger.lnk"
)

REM Remove installation directory
if exist "%INSTALL_DIR%" (
    echo Removing installation directory...
    rmdir /S /Q "%INSTALL_DIR%"
)

echo.
echo ========================================
echo    Uninstallation Complete!
echo ========================================
echo.
echo Task Logger has been removed from your system.
echo.
echo Your task data is still available at:
echo   %USERPROFILE%\Documents\TaskLogger\TaskLogger.db
echo.
echo You can reinstall Task Logger anytime by running the installer again.
echo.
pause
