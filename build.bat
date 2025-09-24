@echo off
setlocal EnableDelayedExpansion

:: ============================================================================
:: Task Logger Build and Installer Creation Script
:: ============================================================================

echo.
echo ============================================================================
echo                    TASK LOGGER BUILD SYSTEM v1.0
echo ============================================================================
echo.

:: Check for administrator privileges (needed for some operations)
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo Warning: Not running as administrator. Some features may be limited.
    echo.
)

:: Variables
set PROJECT_DIR=%~dp0
set SRC_DIR=%PROJECT_DIR%src\TaskLogger
set INSTALLER_DIR=%PROJECT_DIR%installer\WixInstaller
set BUILD_DIR=%PROJECT_DIR%build
set OUTPUT_DIR=%BUILD_DIR%\output
set INSTALLER_OUTPUT=%BUILD_DIR%\installer
set LOG_FILE=%BUILD_DIR%\build.log

:: Check if running in GitHub Actions
if defined GITHUB_ACTIONS (
    set GITHUB_CI=1
    echo Running in GitHub Actions environment
) else (
    set GITHUB_CI=0
)

:: Parse command line arguments
set BUILD_TYPE=Release
set CLEAN_BUILD=1
set BUILD_PROJECT=1
set BUILD_INSTALLER=1
set CREATE_PACKAGE=1

:parse_args
if "%~1"=="" goto :args_done
if /i "%~1"=="--debug" (
    set BUILD_TYPE=Debug
    shift
    goto :parse_args
)
if /i "%~1"=="--no-clean" (
    set CLEAN_BUILD=0
    shift
    goto :parse_args
)
if /i "%~1"=="--project-only" (
    set BUILD_INSTALLER=0
    set CREATE_PACKAGE=0
    shift
    goto :parse_args
)
if /i "%~1"=="--installer-only" (
    set BUILD_PROJECT=0
    shift
    goto :parse_args
)
if /i "%~1"=="--help" (
    goto :show_help
)
shift
goto :parse_args
:args_done

:: Create build directory
if not exist "%BUILD_DIR%" mkdir "%BUILD_DIR%"
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"
if not exist "%INSTALLER_OUTPUT%" mkdir "%INSTALLER_OUTPUT%"

:: Start logging
echo Build started at %date% %time% > "%LOG_FILE%"
echo. >> "%LOG_FILE%"

:: Display build configuration
echo Build Configuration:
echo   - Build Type: %BUILD_TYPE%
echo   - Clean Build: %CLEAN_BUILD%
echo   - Build Project: %BUILD_PROJECT%
echo   - Build Installer: %BUILD_INSTALLER%
echo   - Create Package: %CREATE_PACKAGE%
echo.
echo Build Configuration: >> "%LOG_FILE%"
echo   - Build Type: %BUILD_TYPE% >> "%LOG_FILE%"
echo   - Clean Build: %CLEAN_BUILD% >> "%LOG_FILE%"
echo   - Build Project: %BUILD_PROJECT% >> "%LOG_FILE%"
echo   - Build Installer: %BUILD_INSTALLER% >> "%LOG_FILE%"
echo   - Create Package: %CREATE_PACKAGE% >> "%LOG_FILE%"
echo. >> "%LOG_FILE%"

:: ============================================================================
:: STEP 1: Check Prerequisites
:: ============================================================================
echo [1/7] Checking Prerequisites...
echo ==============================
echo.

:: Check for .NET SDK
echo Checking .NET SDK...
dotnet --version >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: .NET SDK is not installed or not in PATH
    echo Please install .NET 8.0 SDK from: https://dotnet.microsoft.com/download
    echo. >> "%LOG_FILE%"
    echo ERROR: .NET SDK not found >> "%LOG_FILE%"
    goto :error
)
for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo   - .NET SDK Version: %DOTNET_VERSION%
echo   - .NET SDK Version: %DOTNET_VERSION% >> "%LOG_FILE%"

:: Check for WiX Toolset (if building installer)
if %BUILD_INSTALLER%==1 (
    echo Checking WiX Toolset...
    dotnet tool list -g | findstr /i "wix" >nul 2>&1
    if %errorLevel% neq 0 (
        echo   - WiX not found. Installing WiX Toolset...
        dotnet tool install --global wix --version 4.0.5
        if %errorLevel% neq 0 (
            echo ERROR: Failed to install WiX Toolset
            echo. >> "%LOG_FILE%"
            echo ERROR: WiX Toolset installation failed >> "%LOG_FILE%"
            goto :error
        )
    )
    echo   - WiX Toolset: Installed
    echo   - WiX Toolset: Installed >> "%LOG_FILE%"
)

echo.
echo Prerequisites check completed successfully.
echo.

:: ============================================================================
:: STEP 2: Clean Previous Builds
:: ============================================================================
if %CLEAN_BUILD%==1 (
    echo [2/7] Cleaning Previous Builds...
    echo =================================
    echo.
    
    if exist "%SRC_DIR%\bin" (
        echo   - Cleaning bin directory...
        rmdir /s /q "%SRC_DIR%\bin" 2>nul
    )
    if exist "%SRC_DIR%\obj" (
        echo   - Cleaning obj directory...
        rmdir /s /q "%SRC_DIR%\obj" 2>nul
    )
    if exist "%INSTALLER_DIR%\bin" (
        echo   - Cleaning installer bin directory...
        rmdir /s /q "%INSTALLER_DIR%\bin" 2>nul
    )
    if exist "%INSTALLER_DIR%\obj" (
        echo   - Cleaning installer obj directory...
        rmdir /s /q "%INSTALLER_DIR%\obj" 2>nul
    )
    if exist "%OUTPUT_DIR%" (
        echo   - Cleaning output directory...
        rmdir /s /q "%OUTPUT_DIR%" 2>nul
        mkdir "%OUTPUT_DIR%"
    )
    
    echo.
    echo Clean completed.
    echo.
) else (
    echo [2/7] Skipping Clean (--no-clean specified^)
    echo.
)

:: ============================================================================
:: STEP 3: Restore NuGet Packages
:: ============================================================================
if %BUILD_PROJECT%==1 (
    echo [3/7] Restoring NuGet Packages...
    echo =================================
    echo.
    
    cd /d "%SRC_DIR%"
    dotnet restore --verbosity quiet
    if %errorLevel% neq 0 (
        echo ERROR: Failed to restore NuGet packages
        echo Check the log file for details: %LOG_FILE%
        echo. >> "%LOG_FILE%"
        echo ERROR: NuGet restore failed >> "%LOG_FILE%"
        dotnet restore >> "%LOG_FILE%" 2>&1
        goto :error
    )
    
    echo Package restore completed successfully.
    echo.
) else (
    echo [3/7] Skipping Package Restore (--installer-only specified^)
    echo.
)

:: ============================================================================
:: STEP 4: Build Task Logger Application
:: ============================================================================
if %BUILD_PROJECT%==1 (
    echo [4/7] Building Task Logger Application...
    echo =========================================
    echo.
    
    cd /d "%SRC_DIR%"
    echo Building %BUILD_TYPE% configuration...
    dotnet build -c %BUILD_TYPE% --verbosity minimal
    if %errorLevel% neq 0 (
        echo ERROR: Build failed
        echo Check the log file for details: %LOG_FILE%
        echo. >> "%LOG_FILE%"
        echo ERROR: Application build failed >> "%LOG_FILE%"
        dotnet build -c %BUILD_TYPE% >> "%LOG_FILE%" 2>&1
        goto :error
    )
    
    echo.
    echo Publishing self-contained application...
    dotnet publish -c %BUILD_TYPE% -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --output "%OUTPUT_DIR%\publish"
    if %errorLevel% neq 0 (
        echo ERROR: Publish failed
        echo Check the log file for details: %LOG_FILE%
        echo. >> "%LOG_FILE%"
        echo ERROR: Application publish failed >> "%LOG_FILE%"
        dotnet publish -c %BUILD_TYPE% -r win-x64 --self-contained >> "%LOG_FILE%" 2>&1
        goto :error
    )
    
    echo.
    echo Application build completed successfully.
    echo.
) else (
    echo [4/7] Skipping Application Build (--installer-only specified^)
    echo.
)

:: ============================================================================
:: STEP 5: Create Installer Resources
:: ============================================================================
if %BUILD_INSTALLER%==1 (
    echo [5/7] Creating Installer Resources...
    echo =====================================
    echo.
    
    :: Create banner and dialog bitmaps if they don't exist
    if not exist "%INSTALLER_DIR%\banner.bmp" (
        echo   - Creating default banner bitmap...
        :: Create a simple banner bitmap (493x58 pixels)
        powershell -Command "$bitmap = New-Object System.Drawing.Bitmap 493, 58; $graphics = [System.Drawing.Graphics]::FromImage($bitmap); $graphics.Clear([System.Drawing.Color]::FromArgb(0, 120, 215)); $font = New-Object System.Drawing.Font('Segoe UI', 20, [System.Drawing.FontStyle]::Bold); $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White); $graphics.DrawString('Task Logger', $font, $brush, 10, 10); $bitmap.Save('%INSTALLER_DIR%\banner.bmp', [System.Drawing.Imaging.ImageFormat]::Bmp); $graphics.Dispose(); $bitmap.Dispose()" 2>nul
        if not exist "%INSTALLER_DIR%\banner.bmp" (
            echo     Warning: Could not create banner bitmap. Using default.
        )
    )
    
    if not exist "%INSTALLER_DIR%\dialog.bmp" (
        echo   - Creating default dialog bitmap...
        :: Create a simple dialog bitmap (493x312 pixels)
        powershell -Command "$bitmap = New-Object System.Drawing.Bitmap 493, 312; $graphics = [System.Drawing.Graphics]::FromImage($bitmap); $gradient = New-Object System.Drawing.Drawing2D.LinearGradientBrush([System.Drawing.Point]::new(0,0), [System.Drawing.Point]::new(493,312), [System.Drawing.Color]::FromArgb(0, 120, 215), [System.Drawing.Color]::FromArgb(0, 84, 153)); $graphics.FillRectangle($gradient, 0, 0, 493, 312); $font = New-Object System.Drawing.Font('Segoe UI', 32, [System.Drawing.FontStyle]::Bold); $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White); $graphics.DrawString('Task Logger', $font, $brush, 120, 100); $font2 = New-Object System.Drawing.Font('Segoe UI', 14); $graphics.DrawString('Professional Task Management', $font2, $brush, 120, 160); $bitmap.Save('%INSTALLER_DIR%\dialog.bmp', [System.Drawing.Imaging.ImageFormat]::Bmp); $graphics.Dispose(); $bitmap.Dispose()" 2>nul
        if not exist "%INSTALLER_DIR%\dialog.bmp" (
            echo     Warning: Could not create dialog bitmap. Using default.
        )
    )
    
    echo.
    echo Installer resources created.
    echo.
) else (
    echo [5/7] Skipping Installer Resources (--project-only specified^)
    echo.
)

:: ============================================================================
:: STEP 6: Build MSI Installer
:: ============================================================================
if %BUILD_INSTALLER%==1 (
    echo [6/7] Building MSI Installer...
    echo ===============================
    echo.
    
    cd /d "%INSTALLER_DIR%"
    
    :: Restore WiX packages
    echo   - Restoring WiX packages...
    dotnet restore --verbosity quiet
    if %errorLevel% neq 0 (
        echo ERROR: Failed to restore WiX packages
        echo. >> "%LOG_FILE%"
        echo ERROR: WiX restore failed >> "%LOG_FILE%"
        goto :error
    )
    
    :: Build the installer
    echo   - Building installer...
    dotnet build -c %BUILD_TYPE% --verbosity minimal
    if %errorLevel% neq 0 (
        echo ERROR: Installer build failed
        echo Check the log file for details: %LOG_FILE%
        echo. >> "%LOG_FILE%"
        echo ERROR: Installer build failed >> "%LOG_FILE%"
        dotnet build -c %BUILD_TYPE% >> "%LOG_FILE%" 2>&1
        goto :error
    )
    
    :: Copy installer to output directory
    if exist "%INSTALLER_OUTPUT%\TaskLoggerSetup.msi" (
        copy /Y "%INSTALLER_OUTPUT%\TaskLoggerSetup.msi" "%OUTPUT_DIR%\TaskLoggerSetup.msi" >nul
        echo.
        echo Installer created successfully: %OUTPUT_DIR%\TaskLoggerSetup.msi
    )
    echo.
) else (
    echo [6/7] Skipping Installer Build (--project-only specified^)
    echo.
)

:: ============================================================================
:: STEP 7: Create Distribution Package
:: ============================================================================
if %CREATE_PACKAGE%==1 (
    echo [7/7] Creating Distribution Package...
    echo ======================================
    echo.
    
    :: Create ZIP package with installer
    if exist "%OUTPUT_DIR%\TaskLoggerSetup.msi" (
        echo   - Creating installer package...
        powershell -Command "Compress-Archive -Path '%OUTPUT_DIR%\TaskLoggerSetup.msi' -DestinationPath '%OUTPUT_DIR%\TaskLogger-Installer.zip' -Force" 2>nul
        if exist "%OUTPUT_DIR%\TaskLogger-Installer.zip" (
            echo     Created: TaskLogger-Installer.zip
        )
    )
    
    :: Create ZIP package with portable version
    if exist "%OUTPUT_DIR%\publish" (
        echo   - Creating portable package...
        powershell -Command "Compress-Archive -Path '%OUTPUT_DIR%\publish\*' -DestinationPath '%OUTPUT_DIR%\TaskLogger-Portable.zip' -Force" 2>nul
        if exist "%OUTPUT_DIR%\TaskLogger-Portable.zip" (
            echo     Created: TaskLogger-Portable.zip
        )
    )
    
    echo.
    echo Distribution packages created.
    echo.
) else (
    echo [7/7] Skipping Package Creation
    echo.
)

:: ============================================================================
:: Build Complete
:: ============================================================================
:success
cd /d "%PROJECT_DIR%"
echo.
echo ============================================================================
echo                         BUILD COMPLETED SUCCESSFULLY
echo ============================================================================
echo.
echo Build Outputs:
echo --------------
if exist "%OUTPUT_DIR%\publish\TaskLogger.exe" (
    echo   - Application: %OUTPUT_DIR%\publish\TaskLogger.exe
    for %%A in ("%OUTPUT_DIR%\publish\TaskLogger.exe") do echo     Size: %%~zA bytes
)
if exist "%OUTPUT_DIR%\TaskLoggerSetup.msi" (
    echo   - Installer: %OUTPUT_DIR%\TaskLoggerSetup.msi
    for %%A in ("%OUTPUT_DIR%\TaskLoggerSetup.msi") do echo     Size: %%~zA bytes
)
if exist "%OUTPUT_DIR%\TaskLogger-Installer.zip" (
    echo   - Installer Package: %OUTPUT_DIR%\TaskLogger-Installer.zip
    for %%A in ("%OUTPUT_DIR%\TaskLogger-Installer.zip") do echo     Size: %%~zA bytes
)
if exist "%OUTPUT_DIR%\TaskLogger-Portable.zip" (
    echo   - Portable Package: %OUTPUT_DIR%\TaskLogger-Portable.zip
    for %%A in ("%OUTPUT_DIR%\TaskLogger-Portable.zip") do echo     Size: %%~zA bytes
)
echo.
echo Build Log: %LOG_FILE%
echo.
echo Build completed at %date% %time% >> "%LOG_FILE%"
echo.

::: Ask if user wants to run the installer (skip in CI)
if "%GITHUB_CI%"=="0" goto maybe_install
echo Skipping installer prompt in CI environment
goto end

:maybe_install
if not exist "%OUTPUT_DIR%\TaskLoggerSetup.msi" goto end
choice /C YN /N /M "Would you like to run the installer now? [Y/N]: "
if errorlevel 2 goto end
if errorlevel 1 (
    echo.
    echo Starting installer...
    start "" "%OUTPUT_DIR%\TaskLoggerSetup.msi"
)

::: ============================================================================
::: Error Handler
::: ============================================================================
:::
::error
cd /d "%PROJECT_DIR%"
echo.
echo ============================================================================
echo                            BUILD FAILED
echo ============================================================================
echo.
echo Please check the log file for details: %LOG_FILE%
echo.
echo Build failed at %date% %time% >> "%LOG_FILE%"
exit /b 1

::: ============================================================================
::: Show Help
::: ============================================================================
:::
::show_help
echo.
echo Task Logger Build Script
echo ========================
echo.
echo Usage: build.bat [options]
echo.
echo Options:
echo   --debug           Build in Debug configuration (default: Release)
echo   --no-clean        Skip cleaning previous builds
echo   --project-only    Build only the application (skip installer)
echo   --installer-only  Build only the installer (skip application)
echo   --help            Show this help message
echo.
echo Examples:
echo   build.bat                    Full build with installer
echo   build.bat --debug            Debug build
echo   build.bat --project-only     Build application only
echo   build.bat --installer-only   Build installer only
echo.
goto :end

::: ============================================================================
::: End
::: ============================================================================
:::
:end
endlocal
echo.
if not "%GITHUB_CI%"=="0" goto exit_now
pause
:exit_now
exit /b 0