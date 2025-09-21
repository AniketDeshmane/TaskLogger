@echo off
echo ==========================================
echo    Task Logger - MSI Installer Build
echo ==========================================
echo.

REM Set paths
set PROJECT_DIR=%~dp0
set INSTALLER_DIR=%PROJECT_DIR%installer\WixInstaller
set OUTPUT_DIR=%PROJECT_DIR%build\output

REM Check if application is built
if not exist "%PROJECT_DIR%bin\Release\net8.0-windows\win-x64\publish\TaskLogger.exe" (
    echo ERROR: Application not published. Please run the following command first:
    echo        dotnet publish -c Release -r win-x64 --self-contained
    pause
    exit /b 1
)

echo [1/3] Cleaning previous installer builds...
if exist "%INSTALLER_DIR%\bin" rmdir /s /q "%INSTALLER_DIR%\bin"
if exist "%INSTALLER_DIR%\obj" rmdir /s /q "%INSTALLER_DIR%\obj"
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"
echo.

echo [2/3] Building MSI installer...
cd "%INSTALLER_DIR%"

REM Build the WiX project using dotnet
dotnet build -c Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Installer build failed
    cd "%PROJECT_DIR%"
    pause
    exit /b 1
)
echo.

echo [3/3] Copying installer to output directory...
if exist "%INSTALLER_DIR%\bin\Release\*.msi" (
    copy "%INSTALLER_DIR%\bin\Release\*.msi" "%OUTPUT_DIR%\" >nul
    echo.
    echo ==========================================
    echo    MSI Installer Build Successful!
    echo ==========================================
    echo.
    echo Installer location:
    echo   %OUTPUT_DIR%\TaskLoggerSetup.msi
) else (
    echo ERROR: MSI file not found in output
    cd "%PROJECT_DIR%"
    pause
    exit /b 1
)

cd "%PROJECT_DIR%"
echo.
pause