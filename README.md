# Task Logger

<div align="center">
  <img src="src/TaskLogger/TaskLogger.png" alt="Task Logger Logo" width="128" height="128">
  
  **Professional Task Management for Windows**
  
  [![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/8.0)
  [![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D6)](https://www.microsoft.com/windows)
  [![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)
  [![Build Status](https://img.shields.io/badge/Build-Passing-success)]()
  [![Version](https://img.shields.io/badge/Version-1.0.0-blue)]()
</div>

---

## 📋 Overview

Task Logger is a professional Windows desktop application designed to streamline task management and productivity tracking. Built with modern WPF and .NET 8.0, it offers seamless integration with Windows system events, automatic task logging, and comprehensive reporting features.

## ✨ Key Features

### Core Functionality
- **📝 Smart Task Tracking** - Intelligent task categorization with customizable tags and priorities
- **🔄 System Event Integration** - Automatic prompts on shutdown, restart, and logoff events
- **💾 Secure Local Storage** - SQLite database with customizable storage location
- **📊 Advanced Analytics** - Detailed task history with search, filter, and export capabilities
- **🎨 Modern UI/UX** - Clean, professional interface following Windows 11 design guidelines

### Professional Features
- **🔐 Data Security** - All data stored locally with optional encryption
- **⚡ High Performance** - Optimized for minimal resource usage
- **🔔 System Tray Integration** - Unobtrusive background operation
- **⌨️ Keyboard Shortcuts** - Comprehensive hotkey support for power users
- **🌙 Theme Support** - Light and dark themes with automatic switching
- **📤 Export Options** - Export data to CSV, JSON, or Excel formats

## 🚀 Installation

### System Requirements
- **Operating System**: Windows 10 version 1809+ or Windows 11
- **Architecture**: x64 (64-bit)
- **RAM**: Minimum 2GB (4GB recommended)
- **Disk Space**: 200MB for installation
- **.NET Runtime**: Included (self-contained deployment)

### Installation Methods

#### Method 1: MSI Installer (Recommended)
1. Download `TaskLoggerSetup.msi` from the latest release
2. Double-click to run the installer
3. Follow the installation wizard:
   - Choose installation directory
   - Select database storage location
   - Configure startup options
4. Launch Task Logger from Start Menu or Desktop

#### Method 2: Portable Version
1. Download `TaskLogger-Portable.zip`
2. Extract to your preferred location
3. Run `TaskLogger.exe`
4. No installation required - runs directly

#### Method 3: Build from Source
```bash
# Clone repository
git clone https://github.com/yourusername/TaskLogger.git
cd TaskLogger

# Run build script
build.bat

# Find installer in build/output/TaskLoggerSetup.msi
```

## 🛠️ Building from Source

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (optional)
- Windows 10/11 SDK
- [WiX Toolset](https://wixtoolset.org/) (for MSI installer)

### Build Instructions

#### Quick Build
```cmd
# Full build with installer
build.bat

# Debug build
build.bat --debug

# Application only
build.bat --project-only

# Installer only
build.bat --installer-only
```

#### Manual Build
```bash
# Navigate to source directory
cd src/TaskLogger

# Restore packages
dotnet restore

# Build application
dotnet build -c Release

# Create self-contained executable
dotnet publish -c Release -r win-x64 --self-contained

# Build installer (requires WiX Toolset)
cd ../../installer/WixInstaller
dotnet build -c Release
```

## 🚀 Automated Releases

This project uses GitHub Actions for automated building and releasing.

### Release Workflow (`release.yml`)
Creates releases when you push a version tag or manually trigger it:

```bash
# Create a new release
git tag v1.0.0
git push origin v1.0.0

# Or manually trigger from GitHub Actions tab
```

**Release Assets:**
- `TaskLogger-{version}-Installer.zip` - Full installer with batch scripts
- `TaskLogger-{version}-Portable.zip` - Portable executable

**Features:**
- Uses your proven `build.bat` script
- Creates professional release packages
- Generates detailed release notes
- Automatically creates GitHub releases with download links

## 📖 User Guide

### First Launch
1. **Database Configuration**: On first launch, select where to store your task database
2. **System Tray**: Application minimizes to system tray by default
3. **Quick Access**: Click tray icon or use `Win+Shift+T` to open main window

### Task Management
- **Add Task**: `Ctrl+N` or click "Add Task" button
- **Quick Entry**: Type task and press `Enter` for rapid logging
- **Categories**: Organize tasks with customizable categories
- **Tags**: Add multiple tags for better organization
- **Priority Levels**: High, Medium, Low priority settings

### Keyboard Shortcuts
| Shortcut | Action |
|----------|--------|
| `Ctrl+N` | New task |
| `Ctrl+S` | Save current task |
| `Ctrl+H` | View history |
| `Ctrl+F` | Search tasks |
| `Ctrl+E` | Export data |
| `Ctrl+,` | Open settings |
| `Esc` | Minimize to tray |
| `Win+Shift+T` | Show/Hide window |

### System Integration
- **Shutdown Prompt**: Automatically prompts to log tasks on system shutdown
- **Daily Summary**: Optional end-of-day task summary
- **Startup Options**: Configure to start with Windows
- **Notification Settings**: Customize notification preferences

## 🔧 Configuration

### Settings Location
- **Application**: `%LOCALAPPDATA%\TaskLogger\`
- **Database**: User-defined (default: `%USERPROFILE%\Documents\TaskLogger\`)
- **Logs**: `%LOCALAPPDATA%\TaskLogger\Logs\`
- **Configuration**: `%LOCALAPPDATA%\TaskLogger\config.json`

### Configuration Options
```json
{
  "General": {
    "StartWithWindows": true,
    "MinimizeToTray": true,
    "ShowInTaskbar": false
  },
  "Database": {
    "Path": "C:\\Users\\[Username]\\Documents\\TaskLogger\\",
    "BackupEnabled": true,
    "BackupInterval": "Daily"
  },
  "SystemEvents": {
    "PromptOnShutdown": true,
    "PromptOnLogoff": true,
    "DailySummary": true,
    "SummaryTime": "17:00"
  },
  "UI": {
    "Theme": "Auto",
    "Language": "en-US",
    "DateFormat": "MM/dd/yyyy"
  }
}
```

## 🏗️ Project Structure

```
TaskLogger/
├── src/
│   └── TaskLogger/           # Main application source
│       ├── Models/           # Data models and entities
│       ├── Views/            # WPF views (XAML)
│       ├── ViewModels/       # MVVM view models
│       ├── Services/         # Business logic services
│       ├── Converters/       # WPF value converters
│       ├── Utils/            # Utility classes
│       └── Themes/           # Application themes
├── installer/
│   └── WixInstaller/         # MSI installer project
├── build/                    # Build outputs
├── tools/                    # Build tools and scripts
├── build.bat                 # Main build script
└── README.md                 # This file
```

## 🔒 Security & Privacy

### Data Protection
- **Local Storage Only**: All data stored locally on your machine
- **No Cloud Sync**: No external servers or cloud storage
- **Optional Encryption**: AES-256 encryption available for sensitive data
- **Secure Deletion**: Proper data wiping when deleting tasks

### Privacy Policy
- No telemetry or usage tracking
- No personal data collection
- No internet connection required
- Complete user control over all data

## 🐛 Troubleshooting

### Common Issues

#### Application won't start
- Ensure .NET 8.0 runtime is installed
- Check Windows Event Viewer for errors
- Run as Administrator if needed

#### Database errors
- Verify database path has write permissions
- Check available disk space
- Use Settings to relocate database if needed

#### System tray icon missing
- Check notification area settings
- Restart Windows Explorer
- Reinstall application

### Getting Help
1. Check [Issues](https://github.com/yourusername/TaskLogger/issues) page
2. Review [Wiki](https://github.com/yourusername/TaskLogger/wiki) documentation
3. Contact support via GitHub

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### Development Setup
1. Fork the repository
2. Create feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open Pull Request

### Code Standards
- Follow C# coding conventions
- Maintain MVVM pattern
- Include XML documentation
- Write unit tests for new features
- Ensure all tests pass before PR

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with [.NET 8.0](https://dotnet.microsoft.com/) and [WPF](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
- Database powered by [SQLite](https://www.sqlite.org/) and [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- System tray integration via [Hardcodet.NotifyIcon.Wpf](https://github.com/hardcodet/wpf-notifyicon)
- Installer created with [WiX Toolset](https://wixtoolset.org/)

## 📊 Project Status

- **Current Version**: 1.0.0
- **Release Date**: January 2024
- **Development Status**: Active
- **Next Release**: 1.1.0 (Q2 2024)

## 📮 Contact

- **GitHub**: [Task Logger Repository](https://github.com/yourusername/TaskLogger)
- **Issues**: [Report Issues](https://github.com/yourusername/TaskLogger/issues)
- **Discussions**: [Community Forum](https://github.com/yourusername/TaskLogger/discussions)

---

<div align="center">
  Made with ❤️ by the Task Logger Team
  
  Copyright © 2024 Task Logger. All rights reserved.
</div>