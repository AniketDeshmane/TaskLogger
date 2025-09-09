@echo off
echo ========================================
echo    Task Logger - Testing Fixes
echo ========================================
echo.

echo The following issues have been fixed:
echo.
echo 1. FIXED: InvalidOperationException when showing DatabaseConfigWindow
echo    - Removed Owner property assignment before MainWindow is shown
echo    - Set WindowStartupLocation to CenterScreen instead
echo.
echo 2. FIXED: XamlParseException with Effect property
echo    - Removed invalid Effect binding in HistoryWindow.xaml
echo    - Added proper DropShadowEffect directly to the Border
echo.
echo ========================================
echo    Building the application...
echo ========================================
echo.

dotnet build -c Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Build failed
    pause
    exit /b 1
)

echo.
echo ========================================
echo    Build Successful!
echo ========================================
echo.
echo The application should now:
echo   1. Start without crashing
echo   2. Show database configuration dialog properly
echo   3. Save tasks without XAML errors
echo   4. Display the main window correctly
echo.
echo Run the exe from: bin\Release\net8.0-windows\win-x64\TaskLogger.exe
echo.
echo Check the log file for any remaining issues:
echo   %LOCALAPPDATA%\TaskLogger\Logs\
echo.
pause