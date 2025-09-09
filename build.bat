@echo off
echo ==========================================
echo    Task Logger - Build Script
echo ==========================================
echo.

echo Cleaning previous builds...
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj
echo.

echo Restoring NuGet packages...
dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to restore packages
    pause
    exit /b 1
)
echo.

echo Building Release configuration...
dotnet build -c Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Build failed
    pause
    exit /b 1
)
echo.

echo ==========================================
echo    Build Successful!
echo ==========================================
echo.
echo Executable location:
echo   bin\Release\net8.0-windows\win-x64\TaskLogger.exe
echo.
echo To create a distribution package, run:
echo   dotnet publish -c Release -r win-x64 --self-contained
echo.
pause