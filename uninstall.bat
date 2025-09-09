@echo off
echo ==========================================
echo    Task Logger - Uninstallation
echo ==========================================
echo.

set INSTALL_DIR=%LOCALAPPDATA%\TaskLogger

echo This will completely remove Task Logger from your system.
echo.
echo Installation directory: %INSTALL_DIR%
echo.
echo WARNING: This will also remove:
echo   - All application settings
echo   - Desktop and Start Menu shortcuts
echo   - Startup registry entry (if exists)
echo.
echo Your task database will be preserved in:
echo   %USERPROFILE%\Documents\TaskLogger\
echo.
echo Are you sure you want to uninstall? (Y/N)
choice /c YN /n /m "Select [Y/N]: "
if errorlevel 2 goto cancel
if errorlevel 1 goto uninstall

:uninstall
echo.
echo Stopping Task Logger if running...
taskkill /F /IM TaskLogger.exe >nul 2>&1
timeout /t 2 /nobreak >nul

echo Removing installation directory...
if exist "%INSTALL_DIR%" (
    rmdir /s /q "%INSTALL_DIR%" 2>nul
    if exist "%INSTALL_DIR%" (
        echo.
        echo ERROR: Could not remove installation directory.
        echo Please close any programs using files in this directory and try again.
        pause
        exit /b 1
    )
)

echo Removing desktop shortcut...
if exist "%USERPROFILE%\Desktop\Task Logger.lnk" (
    del "%USERPROFILE%\Desktop\Task Logger.lnk" 2>nul
)

echo Removing Start Menu shortcut...
set START_MENU=%APPDATA%\Microsoft\Windows\Start Menu\Programs
if exist "%START_MENU%\Task Logger.lnk" (
    del "%START_MENU%\Task Logger.lnk" 2>nul
)

echo Removing startup registry entry if exists...
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "TaskLogger" /f >nul 2>&1

echo.
echo ==========================================
echo    Uninstallation Complete!
echo ==========================================
echo.
echo Task Logger has been removed from your system.
echo.
echo Note: Your task database has been preserved in:
echo   %USERPROFILE%\Documents\TaskLogger\
echo.
echo If you want to remove the database as well, delete that folder manually.
goto end

:cancel
echo.
echo Uninstallation cancelled.
goto end

:end
echo.
echo Press any key to exit...
pause >nul
exit