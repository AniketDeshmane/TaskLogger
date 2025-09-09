# Task Logger - Troubleshooting Guide

## Issue: Application doesn't show any window when exe is run

### Diagnostic Steps

1. **Run the Diagnostic Script**
   ```powershell
   .\diagnose.ps1
   ```
   This will check your .NET installation, build the project, and look for any issues.

2. **Check the Log Files**
   - Logs are stored in: `%LOCALAPPDATA%\TaskLogger\Logs\`
   - Look for files named `TaskLogger_YYYY-MM-DD.log`
   - The application now has comprehensive logging that will show:
     - Application startup sequence
     - Service initialization
     - Any errors or exceptions
     - Database connection attempts

3. **Use the Debug Log Viewer**
   - If the application starts but minimizes to tray, click the "Debug Logs" button
   - This will show real-time application logs with filtering capabilities

4. **Run Console Test Mode**
   ```cmd
   TaskLogger.exe --test-console
   ```
   This runs a console-based diagnostic that tests all major components.

### Common Issues and Solutions

#### 1. Application Crashes on Startup
**Symptoms:** Process appears briefly in Task Manager then disappears
**Check:** 
- Windows Event Viewer > Windows Logs > Application
- Look for .NET Runtime errors
- Check the log file for FATAL entries

**Solutions:**
- Ensure .NET 8.0 Desktop Runtime is installed
- Run as Administrator (first time only)
- Check antivirus isn't blocking the app

#### 2. Window Opens but Immediately Closes
**Symptoms:** Window flashes briefly
**Check:** Log file for database initialization errors

**Solutions:**
- Delete the database file: `%USERPROFILE%\Documents\TaskLogger\TaskLogger.db`
- Check write permissions to Documents folder
- Try running from a different location

#### 3. Application Runs but No Window Visible
**Symptoms:** Process runs in Task Manager but no window
**Check:** System tray (notification area) for TaskLogger icon

**Solutions:**
- Look for the app icon in the system tray
- Right-click the tray icon and select "Show"
- Check if window is off-screen (Win+Tab to see all windows)

#### 4. Database Errors
**Symptoms:** Errors about database in logs
**Check:** Log entries containing "database" or "sqlite"

**Solutions:**
- Ensure the database directory exists and is writable
- Delete corrupted database file and let app recreate it
- Check disk space availability

### Build Issues

1. **Clean Build**
   ```cmd
   dotnet clean
   dotnet restore
   dotnet build -c Release
   ```

2. **Publish Self-Contained**
   ```cmd
   dotnet publish -c Release -r win-x64 --self-contained
   ```

3. **Run from Build Output**
   ```cmd
   bin\Release\net8.0-windows\win-x64\TaskLogger.exe
   ```

### Required Prerequisites

- **Windows 10/11 (64-bit)**
- **.NET 8.0 Desktop Runtime** or **.NET 8.0 SDK**
  - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
- **Visual C++ Redistributables** (usually already installed)

### Getting Help

1. **Check Logs First**
   - The new logging system captures detailed information
   - Look for ERROR or FATAL entries
   - Note the timestamp of when the issue occurred

2. **Run Diagnostic Tools**
   - Use `diagnose.ps1` for comprehensive system check
   - Use `build-and-test.bat` for build verification
   - Run with `--test-console` flag for component testing

3. **Collect Information for Support**
   When reporting issues, include:
   - The log file from the day the issue occurred
   - Output from `diagnose.ps1`
   - Windows version (run `winver`)
   - .NET versions (run `dotnet --info`)

### Log File Locations

- **Application Logs:** `%LOCALAPPDATA%\TaskLogger\Logs\TaskLogger_YYYY-MM-DD.log`
- **Database:** `%USERPROFILE%\Documents\TaskLogger\TaskLogger.db`
- **Configuration:** `%LOCALAPPDATA%\TaskLogger\config.json`

### Emergency Recovery

If the application won't start at all:

1. **Safe Mode Start**
   - Hold Shift while starting the application
   - This skips auto-start services

2. **Reset Configuration**
   - Delete: `%LOCALAPPDATA%\TaskLogger\config.json`
   - Application will use defaults

3. **Fresh Install**
   - Backup your database from Documents\TaskLogger
   - Uninstall using `uninstall.bat`
   - Delete `%LOCALAPPDATA%\TaskLogger` folder
   - Reinstall using `install.bat`
   - Restore your database file

### Debug Mode Features

The application now includes:
- Comprehensive logging to file
- Debug log viewer window
- Console test mode
- Detailed error messages with stack traces
- Automatic log cleanup (keeps 30 days)

### Contact Support

If you continue to experience issues after following this guide:
1. Collect all log files from the issue date
2. Run `diagnose.ps1` and save the output
3. Note the exact steps to reproduce the issue
4. Include screenshots if applicable