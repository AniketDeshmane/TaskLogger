@echo off
echo ==========================================
echo    Task Logger - Installation
echo ==========================================
echo.

set INSTALL_DIR=%LOCALAPPDATA%\TaskLogger

echo This will install Task Logger to:
echo   %INSTALL_DIR%
echo.
echo Press any key to continue or close this window to cancel...
pause >nul

echo.
if exist "%INSTALL_DIR%" (
    echo Removing previous installation...
    taskkill /F /IM TaskLogger.exe >nul 2>&1
    timeout /t 2 /nobreak >nul
    rmdir /s /q "%INSTALL_DIR%" 2>nul
)

echo Creating installation directory...
mkdir "%INSTALL_DIR%" 2>nul

echo Copying application files...
xcopy /E /I /Y "%~dp0*" "%INSTALL_DIR%\" >nul

echo Creating desktop shortcut...
powershell -ExecutionPolicy Bypass -Command ^
    "$WshShell = New-Object -comObject WScript.Shell; ^
     $Shortcut = $WshShell.CreateShortcut('%USERPROFILE%\Desktop\Task Logger.lnk'); ^
     $Shortcut.TargetPath = '%INSTALL_DIR%\TaskLogger.exe'; ^
     $Shortcut.WorkingDirectory = '%INSTALL_DIR%'; ^
     $Shortcut.IconLocation = '%INSTALL_DIR%\TaskLogger.exe'; ^
     $Shortcut.Description = 'Task Logger - Track your daily tasks'; ^
     $Shortcut.Save()"

echo Creating Start Menu shortcut...
set START_MENU=%APPDATA%\Microsoft\Windows\Start Menu\Programs
powershell -ExecutionPolicy Bypass -Command ^
    "$WshShell = New-Object -comObject WScript.Shell; ^
     $Shortcut = $WshShell.CreateShortcut('%START_MENU%\Task Logger.lnk'); ^
     $Shortcut.TargetPath = '%INSTALL_DIR%\TaskLogger.exe'; ^
     $Shortcut.WorkingDirectory = '%INSTALL_DIR%'; ^
     $Shortcut.IconLocation = '%INSTALL_DIR%\TaskLogger.exe'; ^
     $Shortcut.Description = 'Task Logger - Track your daily tasks'; ^
     $Shortcut.Save()"

echo.
echo ==========================================
echo    Installation Complete!
echo ==========================================
echo.
echo Task Logger has been installed successfully.
echo.
echo Shortcuts created:
echo   - Desktop
echo   - Start Menu
echo.
echo Would you like to start Task Logger now? (Y/N)
choice /c YN /n /m "Select [Y/N]: "
if errorlevel 2 goto end
if errorlevel 1 goto start

:start
echo.
echo Starting Task Logger...
start "" "%INSTALL_DIR%\TaskLogger.exe"
goto end

:end
echo.
echo Installation complete. Press any key to exit...
pause >nul
exit