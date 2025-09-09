@echo off
echo ========================================
echo    Task Logger - Installation Script
echo ========================================
echo.

set INSTALL_DIR=%USERPROFILE%\TaskLogger
set DESKTOP_DIR=%USERPROFILE%\Desktop
set STARTMENU_DIR=%APPDATA%\Microsoft\Windows\Start Menu\Programs

echo Installing Task Logger to: %INSTALL_DIR%
echo.

REM Create installation directory
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"

REM Copy application files
echo Copying application files...
xcopy /E /I /Y "%~dp0*" "%INSTALL_DIR%\"

REM Create desktop shortcut
echo Creating desktop shortcut...
set SHORTCUT_TARGET=%INSTALL_DIR%\TaskLogger.exe
set SHORTCUT_PATH=%DESKTOP_DIR%\Task Logger.lnk

powershell -Command "$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%SHORTCUT_PATH%'); $Shortcut.TargetPath = '%SHORTCUT_TARGET%'; $Shortcut.WorkingDirectory = '%INSTALL_DIR%'; $Shortcut.Description = 'Task Logger - Daily Task Management'; $Shortcut.Save()"

REM Create start menu shortcut
echo Creating start menu shortcut...
if not exist "%STARTMENU_DIR%" mkdir "%STARTMENU_DIR%"
set STARTMENU_SHORTCUT=%STARTMENU_DIR%\Task Logger.lnk

powershell -Command "$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%STARTMENU_SHORTCUT%'); $Shortcut.TargetPath = '%SHORTCUT_TARGET%'; $Shortcut.WorkingDirectory = '%INSTALL_DIR%'; $Shortcut.Description = 'Task Logger - Daily Task Management'; $Shortcut.Save()"

echo.
echo ========================================
echo    Installation Complete!
echo ========================================
echo.
echo Task Logger has been installed to:
echo   %INSTALL_DIR%
echo.
echo Shortcuts created:
echo   - Desktop: Task Logger
echo   - Start Menu: Task Logger
echo.
echo You can now:
echo   1. Run Task Logger from the desktop shortcut
echo   2. Or run it directly from: %INSTALL_DIR%\TaskLogger.exe
echo.
echo The application will:
echo   - Ask you to configure database location on first run
echo   - Prompt to start with Windows
echo   - Run in system tray for background monitoring
echo.
pause
