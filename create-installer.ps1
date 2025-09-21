# Task Logger Installer Creation Script
# This script creates an MSI installer without requiring WiX SDK

param(
    [string]$PublishPath = ".\build\output\publish",
    [string]$OutputPath = ".\build\output",
    [string]$Version = "1.0.0"
)

Write-Host "===========================================" -ForegroundColor Cyan
Write-Host "    Task Logger - MSI Installer Creator" -ForegroundColor Cyan
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host ""

# Check if publish directory exists
if (-not (Test-Path $PublishPath)) {
    Write-Host "ERROR: Published application not found at $PublishPath" -ForegroundColor Red
    Write-Host "Please run: dotnet publish -c Release -r win-x64 --self-contained" -ForegroundColor Yellow
    exit 1
}

# Create output directory if it doesn't exist
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath | Out-Null
}

# Alternative 1: Create a simple setup using IExpress (built into Windows)
Write-Host "[1/3] Creating self-extracting installer using IExpress..." -ForegroundColor Green

$sedFile = @"
[Version]
Class=IEXPRESS
SEDVersion=3
[Options]
PackagePurpose=InstallApp
ShowInstallProgramWindow=0
HideExtractAnimation=1
UseLongFileName=1
InsideCompressed=0
CAB_FixedSize=0
CAB_ResvCodeSigning=0
RebootMode=N
InstallPrompt=%InstallPrompt%
DisplayLicense=%DisplayLicense%
FinishMessage=%FinishMessage%
TargetName=%TargetName%
FriendlyName=%FriendlyName%
AppLaunched=%AppLaunched%
PostInstallCmd=%PostInstallCmd%
AdminQuietInstCmd=%AdminQuietInstCmd%
UserQuietInstCmd=%UserQuietInstCmd%
SourceFiles=SourceFiles
[Strings]
InstallPrompt=Do you want to install Task Logger?
DisplayLicense=
FinishMessage=Task Logger has been installed successfully!
TargetName=$OutputPath\TaskLoggerSetup.exe
FriendlyName=Task Logger Setup
AppLaunched=cmd /c xcopy /E /I /Y "%TEMP%\IXP000.TMP" "%ProgramFiles%\TaskLogger"
PostInstallCmd=<None>
AdminQuietInstCmd=
UserQuietInstCmd=
FILE0="TaskLogger.exe"
[SourceFiles]
SourceFiles0=$PublishPath\
[SourceFiles0]
%FILE0%=
"@

$sedFilePath = "$env:TEMP\tasklogger_setup.sed"
$sedFile | Out-File -FilePath $sedFilePath -Encoding ASCII

# Run IExpress
$iexpressPath = "$env:WINDIR\System32\iexpress.exe"
if (Test-Path $iexpressPath) {
    Start-Process -FilePath $iexpressPath -ArgumentList "/N", $sedFilePath -Wait -NoNewWindow
    Write-Host "Self-extracting installer created: $OutputPath\TaskLoggerSetup.exe" -ForegroundColor Green
} else {
    Write-Host "WARNING: IExpress not found. Skipping self-extracting installer." -ForegroundColor Yellow
}

# Alternative 2: Create a ZIP-based installer with PowerShell script
Write-Host ""
Write-Host "[2/3] Creating ZIP-based installer..." -ForegroundColor Green

$zipPath = "$OutputPath\TaskLogger-$Version.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

# Create ZIP file
Compress-Archive -Path "$PublishPath\*" -DestinationPath $zipPath -Force
Write-Host "ZIP package created: $zipPath" -ForegroundColor Green

# Create installer script
$installerScript = @'
# Task Logger Installation Script
param(
    [string]$InstallPath = "$env:ProgramFiles\TaskLogger"
)

Write-Host "Installing Task Logger..." -ForegroundColor Cyan

# Check for admin rights
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "This installer requires Administrator privileges." -ForegroundColor Red
    Write-Host "Please run as Administrator." -ForegroundColor Yellow
    pause
    exit 1
}

# Extract files
$zipFile = Join-Path $PSScriptRoot "TaskLogger-1.0.0.zip"
if (-not (Test-Path $zipFile)) {
    Write-Host "ERROR: Installation files not found." -ForegroundColor Red
    pause
    exit 1
}

# Create installation directory
if (Test-Path $InstallPath) {
    Write-Host "Removing existing installation..." -ForegroundColor Yellow
    Remove-Item -Path $InstallPath -Recurse -Force
}

New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null

# Extract files
Write-Host "Extracting files..." -ForegroundColor Green
Expand-Archive -Path $zipFile -DestinationPath $InstallPath -Force

# Create Start Menu shortcut
$startMenuPath = [Environment]::GetFolderPath("CommonStartMenu")
$shortcutPath = Join-Path $startMenuPath "Programs\Task Logger.lnk"

$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = Join-Path $InstallPath "TaskLogger.exe"
$shortcut.WorkingDirectory = $InstallPath
$shortcut.Description = "Task Logger Application"
$shortcut.IconLocation = Join-Path $InstallPath "TaskLogger.exe"
$shortcut.Save()

Write-Host "Start Menu shortcut created." -ForegroundColor Green

# Create Desktop shortcut (optional)
$desktopPath = [Environment]::GetFolderPath("CommonDesktopDirectory")
$desktopShortcut = Join-Path $desktopPath "Task Logger.lnk"
$shortcut2 = $shell.CreateShortcut($desktopShortcut)
$shortcut2.TargetPath = Join-Path $InstallPath "TaskLogger.exe"
$shortcut2.WorkingDirectory = $InstallPath
$shortcut2.Description = "Task Logger Application"
$shortcut2.IconLocation = Join-Path $InstallPath "TaskLogger.exe"
$shortcut2.Save()

Write-Host "Desktop shortcut created." -ForegroundColor Green

# Add to PATH (optional)
$currentPath = [Environment]::GetEnvironmentVariable("Path", "Machine")
if ($currentPath -notlike "*$InstallPath*") {
    [Environment]::SetEnvironmentVariable("Path", "$currentPath;$InstallPath", "Machine")
    Write-Host "Added to system PATH." -ForegroundColor Green
}

# Register uninstaller
$uninstallPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\TaskLogger"
New-Item -Path $uninstallPath -Force | Out-Null
New-ItemProperty -Path $uninstallPath -Name "DisplayName" -Value "Task Logger" -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallPath -Name "DisplayVersion" -Value "1.0.0" -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallPath -Name "Publisher" -Value "Your Company" -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallPath -Name "InstallLocation" -Value $InstallPath -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallPath -Name "UninstallString" -Value "powershell.exe -ExecutionPolicy Bypass -File `"$InstallPath\uninstall.ps1`"" -PropertyType String -Force | Out-Null

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Task Logger installed successfully!" -ForegroundColor Green
Write-Host "Installation path: $InstallPath" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
pause
'@

$installerScript | Out-File -FilePath "$OutputPath\install.ps1" -Encoding UTF8

# Create uninstaller script
$uninstallerScript = @'
# Task Logger Uninstallation Script

Write-Host "Uninstalling Task Logger..." -ForegroundColor Cyan

# Check for admin rights
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "This uninstaller requires Administrator privileges." -ForegroundColor Red
    Write-Host "Please run as Administrator." -ForegroundColor Yellow
    pause
    exit 1
}

$InstallPath = "$env:ProgramFiles\TaskLogger"

# Remove shortcuts
$startMenuPath = [Environment]::GetFolderPath("CommonStartMenu")
$shortcutPath = Join-Path $startMenuPath "Programs\Task Logger.lnk"
if (Test-Path $shortcutPath) {
    Remove-Item $shortcutPath -Force
    Write-Host "Start Menu shortcut removed." -ForegroundColor Green
}

$desktopPath = [Environment]::GetFolderPath("CommonDesktopDirectory")
$desktopShortcut = Join-Path $desktopPath "Task Logger.lnk"
if (Test-Path $desktopShortcut) {
    Remove-Item $desktopShortcut -Force
    Write-Host "Desktop shortcut removed." -ForegroundColor Green
}

# Remove from PATH
$currentPath = [Environment]::GetEnvironmentVariable("Path", "Machine")
$newPath = ($currentPath.Split(';') | Where-Object { $_ -ne $InstallPath }) -join ';'
[Environment]::SetEnvironmentVariable("Path", $newPath, "Machine")
Write-Host "Removed from system PATH." -ForegroundColor Green

# Remove uninstaller registry entry
$uninstallPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\TaskLogger"
if (Test-Path $uninstallPath) {
    Remove-Item -Path $uninstallPath -Recurse -Force
    Write-Host "Registry entry removed." -ForegroundColor Green
}

# Remove installation directory
if (Test-Path $InstallPath) {
    Remove-Item -Path $InstallPath -Recurse -Force
    Write-Host "Installation directory removed." -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Task Logger uninstalled successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
pause
'@

$uninstallerScript | Out-File -FilePath "$OutputPath\uninstall.ps1" -Encoding UTF8

# Copy uninstaller to publish directory for inclusion in package
Copy-Item "$OutputPath\uninstall.ps1" -Destination "$PublishPath\uninstall.ps1" -Force

# Recreate ZIP with uninstaller included
Remove-Item $zipPath -Force
Compress-Archive -Path "$PublishPath\*" -DestinationPath $zipPath -Force

Write-Host ""
Write-Host "[3/3] Creating batch installer wrapper..." -ForegroundColor Green

# Create a batch file to run the PowerShell installer
$batchInstaller = @"
@echo off
echo ==========================================
echo    Task Logger - Installation
echo ==========================================
echo.
echo This installer requires Administrator privileges.
echo Please ensure you are running as Administrator.
echo.
pause

powershell.exe -ExecutionPolicy Bypass -File "%~dp0install.ps1"
"@

$batchInstaller | Out-File -FilePath "$OutputPath\setup.bat" -Encoding ASCII

Write-Host ""
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host "    Installer Creation Complete!" -ForegroundColor Cyan
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Created installers:" -ForegroundColor Green
Write-Host "  1. Self-extracting: $OutputPath\TaskLoggerSetup.exe (if IExpress available)" -ForegroundColor White
Write-Host "  2. ZIP package: $zipPath" -ForegroundColor White
Write-Host "  3. PowerShell installer: $OutputPath\install.ps1" -ForegroundColor White
Write-Host "  4. Batch installer: $OutputPath\setup.bat" -ForegroundColor White
Write-Host ""
Write-Host "To install, users can run setup.bat as Administrator" -ForegroundColor Yellow
Write-Host ""