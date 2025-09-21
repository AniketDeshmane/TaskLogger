@echo off
echo ==========================================
echo    Task Logger - Complete Build Script
echo ==========================================
echo.

REM Set variables
set PROJECT_DIR=%~dp0
set OUTPUT_DIR=%PROJECT_DIR%build\output
set PUBLISH_DIR=%OUTPUT_DIR%\publish
set INSTALLER_DIR=%PROJECT_DIR%installer\WixInstaller

REM Create output directories
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"
if not exist "%PUBLISH_DIR%" mkdir "%PUBLISH_DIR%"

echo [1/7] Cleaning previous builds...
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj
if exist "%INSTALLER_DIR%\bin" rmdir /s /q "%INSTALLER_DIR%\bin"
if exist "%INSTALLER_DIR%\obj" rmdir /s /q "%INSTALLER_DIR%\obj"
echo.

echo [2/7] Restoring NuGet packages...
dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to restore packages
    pause
    exit /b 1
)
echo.

echo [3/7] Building Release configuration...
dotnet build -c Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Build failed
    pause
    exit /b 1
)
echo.

echo [4/7] Publishing self-contained application...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o "%PUBLISH_DIR%"
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Publish failed
    pause
    exit /b 1
)
echo.

echo [5/7] Creating portable ZIP package...
set ZIP_FILE=%OUTPUT_DIR%\TaskLogger-Portable.zip
if exist "%ZIP_FILE%" del "%ZIP_FILE%"
powershell -Command "Compress-Archive -Path '%PUBLISH_DIR%\*' -DestinationPath '%ZIP_FILE%' -Force"
if %ERRORLEVEL% NEQ 0 (
    echo WARNING: Failed to create ZIP package
) else (
    echo Portable package created: %ZIP_FILE%
)
echo.

echo [6/7] Building MSI Installer...
echo ===============================

REM Try WiX installer first if the directory exists
if exist "%INSTALLER_DIR%" (
    cd "%INSTALLER_DIR%"
    
    REM Check if WiX SDK is available
    dotnet --version >nul 2>&1
    if %ERRORLEVEL% NEQ 0 (
        echo ERROR: .NET SDK not found. Please install .NET SDK.
        cd "%PROJECT_DIR%"
        goto :FALLBACK_INSTALLER
    )
    
    REM Restore WiX packages
    echo - Restoring WiX packages...
    dotnet restore >nul 2>&1
    if %ERRORLEVEL% NEQ 0 (
        echo WARNING: WiX package restore failed. Using fallback installer...
        cd "%PROJECT_DIR%"
        goto :FALLBACK_INSTALLER
    )
    
    REM Build the installer
    echo - Building installer...
    dotnet build -c Release >nul 2>&1
    if %ERRORLEVEL% NEQ 0 (
        echo WARNING: WiX MSI build failed. Using fallback installer...
        cd "%PROJECT_DIR%"
        goto :FALLBACK_INSTALLER
    ) else (
        REM Copy MSI to output directory
        if exist "%INSTALLER_DIR%\bin\Release\*.msi" (
            copy "%INSTALLER_DIR%\bin\Release\*.msi" "%OUTPUT_DIR%\" >nul
            echo MSI installer created successfully
            cd "%PROJECT_DIR%"
            goto :INSTALLER_DONE
        )
    )
)

:FALLBACK_INSTALLER
echo - WiX not available, using PowerShell installer creator...
powershell.exe -ExecutionPolicy Bypass -File "%PROJECT_DIR%\create-installer.ps1" -PublishPath "%PUBLISH_DIR%" -OutputPath "%OUTPUT_DIR%" -Version "1.0.0"
if %ERRORLEVEL% NEQ 0 (
    echo WARNING: PowerShell installer creation failed.
    echo          Manual installation from ZIP package will be required.
) else (
    echo Alternative installer created successfully
)

:INSTALLER_DONE
echo.

echo [7/7] Build Summary
echo ==========================================
echo    Build Complete!
echo ==========================================
echo.
echo Output files:
if exist "%PUBLISH_DIR%\TaskLogger.exe" (
    echo   [OK] Executable: %PUBLISH_DIR%\TaskLogger.exe
) else (
    echo   [MISSING] Executable not found
)

if exist "%ZIP_FILE%" (
    echo   [OK] Portable: %ZIP_FILE%
) else (
    echo   [MISSING] Portable package not created
)

if exist "%OUTPUT_DIR%\TaskLoggerSetup.msi" (
    echo   [OK] Installer: %OUTPUT_DIR%\TaskLoggerSetup.msi
) else (
    echo   [INFO] MSI installer not created (WiX may not be installed)
)

echo.
echo To distribute the application, use either:
echo   1. The portable ZIP package (no installation required)
echo   2. The MSI installer (if available)
echo.
pause