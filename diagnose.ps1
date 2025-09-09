# Task Logger Diagnostic Script
Write-Host "========================================"
Write-Host "   Task Logger - Diagnostic Script"
Write-Host "========================================"
Write-Host ""

# Function to test and report
function Test-Component {
    param(
        [string]$Name,
        [scriptblock]$Test
    )
    
    Write-Host -NoNewline "Testing $Name... "
    try {
        $result = & $Test
        if ($result) {
            Write-Host "✓ PASS" -ForegroundColor Green
            return $true
        } else {
            Write-Host "✗ FAIL" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "✗ ERROR: $_" -ForegroundColor Red
        return $false
    }
}

# Check .NET SDK
Write-Host "1. Checking .NET Environment" -ForegroundColor Cyan
Write-Host "   .NET SDK versions installed:"
dotnet --list-sdks | ForEach-Object { Write-Host "     $_" }
Write-Host "   .NET Runtime versions installed:"
dotnet --list-runtimes | ForEach-Object { Write-Host "     $_" }
Write-Host ""

# Check project file
Write-Host "2. Checking Project Configuration" -ForegroundColor Cyan
if (Test-Path "TaskLogger.csproj") {
    Write-Host "   Project file found"
    $csproj = Get-Content "TaskLogger.csproj" -Raw
    if ($csproj -match '<TargetFramework>(.*?)</TargetFramework>') {
        Write-Host "   Target Framework: $($Matches[1])"
    }
    if ($csproj -match '<RuntimeIdentifier>(.*?)</RuntimeIdentifier>') {
        Write-Host "   Runtime Identifier: $($Matches[1])"
    }
} else {
    Write-Host "   ERROR: TaskLogger.csproj not found!" -ForegroundColor Red
}
Write-Host ""

# Try to build
Write-Host "3. Attempting Build" -ForegroundColor Cyan
$buildResult = dotnet build -c Release 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "   Build succeeded" -ForegroundColor Green
} else {
    Write-Host "   Build failed with exit code: $LASTEXITCODE" -ForegroundColor Red
    Write-Host "   Build output:"
    $buildResult | ForEach-Object { Write-Host "     $_" }
}
Write-Host ""

# Check for output files
Write-Host "4. Checking Output Files" -ForegroundColor Cyan
$possiblePaths = @(
    "bin\Release\net8.0-windows\TaskLogger.exe",
    "bin\Release\net8.0-windows\win-x64\TaskLogger.exe",
    "bin\Release\net8.0-windows\win-x64\publish\TaskLogger.exe"
)

$exeFound = $false
foreach ($path in $possiblePaths) {
    if (Test-Path $path) {
        Write-Host "   Found exe at: $path" -ForegroundColor Green
        $exeInfo = Get-Item $path
        Write-Host "     Size: $($exeInfo.Length) bytes"
        Write-Host "     Created: $($exeInfo.CreationTime)"
        $exeFound = $true
        $exePath = $path
    }
}

if (-not $exeFound) {
    Write-Host "   No executable found in expected locations" -ForegroundColor Red
    Write-Host "   Searching for any .exe files in bin folder..."
    Get-ChildItem -Path "bin" -Filter "*.exe" -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
        Write-Host "     Found: $($_.FullName)"
    }
}
Write-Host ""

# Check log files
Write-Host "5. Checking Log Files" -ForegroundColor Cyan
$logPath = "$env:LOCALAPPDATA\TaskLogger\Logs"
if (Test-Path $logPath) {
    Write-Host "   Log directory exists: $logPath"
    $logFiles = Get-ChildItem -Path $logPath -Filter "*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 3
    if ($logFiles) {
        Write-Host "   Recent log files:"
        foreach ($file in $logFiles) {
            Write-Host "     $($file.Name) - $($file.LastWriteTime) - $($file.Length) bytes"
        }
        
        # Show last few lines of most recent log
        $mostRecent = $logFiles[0]
        if ($mostRecent) {
            Write-Host ""
            Write-Host "   Last 10 lines from most recent log:" -ForegroundColor Yellow
            Get-Content $mostRecent.FullName -Tail 10 | ForEach-Object { Write-Host "     $_" }
        }
    } else {
        Write-Host "   No log files found"
    }
} else {
    Write-Host "   Log directory does not exist: $logPath"
}
Write-Host ""

# Try to run the exe with test mode
if ($exeFound) {
    Write-Host "6. Testing Executable" -ForegroundColor Cyan
    Write-Host "   Running exe with --test-console flag..."
    
    $process = Start-Process -FilePath $exePath -ArgumentList "--test-console" -NoNewWindow -PassThru -Wait
    if ($process.ExitCode -eq 0) {
        Write-Host "   Console test completed successfully" -ForegroundColor Green
    } else {
        Write-Host "   Console test failed with exit code: $($process.ExitCode)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "========================================"
Write-Host "   Diagnostic Complete"
Write-Host "========================================"
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Check the log files for any error messages"
Write-Host "2. Try running the exe directly from the command line"
Write-Host "3. Check Windows Event Viewer for application crashes"
Write-Host "4. Ensure all required .NET runtimes are installed"
Write-Host ""

# Offer to open log folder
$openLogs = Read-Host "Would you like to open the log folder? (Y/N)"
if ($openLogs -eq 'Y' -or $openLogs -eq 'y') {
    if (Test-Path $logPath) {
        Start-Process explorer.exe $logPath
    } else {
        Write-Host "Log folder does not exist yet" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")