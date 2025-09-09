@echo off
echo ========================================
echo    Task Logger - Build and Test Script
echo ========================================
echo.

REM Clean previous builds
echo Cleaning previous builds...
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj
echo.

REM Restore packages
echo Restoring NuGet packages...
dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to restore packages
    pause
    exit /b 1
)
echo.

REM Build the project
echo Building the project...
dotnet build -c Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Build failed
    pause
    exit /b 1
)
echo.

REM Run console test
echo Running console test...
dotnet run --no-build -c Release -- --test-console
echo.

REM Check if exe was created
echo Checking for executable...
set EXE_PATH=bin\Release\net8.0-windows\win-x64\TaskLogger.exe
if exist %EXE_PATH% (
    echo Found executable at: %EXE_PATH%
    echo.
    
    echo Would you like to:
    echo   1. Run the TaskLogger.exe normally
    echo   2. Run with console test mode
    echo   3. Open the log folder
    echo   4. Exit
    echo.
    
    choice /c 1234 /n /m "Select option (1-4): "
    
    if %ERRORLEVEL%==1 (
        echo Starting TaskLogger.exe...
        start "" %EXE_PATH%
    )
    if %ERRORLEVEL%==2 (
        echo Running console test...
        %EXE_PATH% --test-console
    )
    if %ERRORLEVEL%==3 (
        echo Opening log folder...
        set LOG_DIR=%LOCALAPPDATA%\TaskLogger\Logs
        if exist "%LOG_DIR%" (
            explorer "%LOG_DIR%"
        ) else (
            echo Log folder not found: %LOG_DIR%
        )
    )
) else (
    echo ERROR: Executable not found at expected location
    echo Expected: %EXE_PATH%
    echo.
    echo Checking other locations...
    dir /s /b bin\*.exe 2>nul
)

echo.
echo ========================================
echo    Build and Test Complete
echo ========================================
pause