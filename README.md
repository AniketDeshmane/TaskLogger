# Task Logger

A beautiful Windows desktop application for tracking daily tasks with automatic logging on system events.

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-blue)
![License](https://img.shields.io/badge/License-MIT-green)

## Features

- 📝 **Task Tracking**: Log your daily tasks with descriptions and categories
- 🎨 **Modern UI**: Beautiful WPF interface with a clean, professional design
- 🔄 **System Integration**: Automatically prompts for task logging on system events
- 💾 **Database Storage**: All tasks stored in a local SQLite database
- 📊 **Task History**: View, search, and export your task history
- 🔔 **System Tray**: Runs quietly in the background with system tray integration
- ⚙️ **Configurable**: Customize settings to match your workflow

## Installation

### Option 1: Using the Installer (Recommended)

1. Download the latest `TaskLogger-Windows-x64-Installer.zip` from [Releases](https://github.com/yourusername/TaskLogger/releases)
2. Extract the ZIP file
3. Run `install.bat`
4. Follow the installation prompts

### Option 2: Portable Version

1. Download `TaskLogger-Windows-x64-SelfContained.zip` from [Releases](https://github.com/yourusername/TaskLogger/releases)
2. Extract to any folder
3. Run `TaskLogger.exe`

### Option 3: Build from Source

#### Prerequisites
- Windows 10/11 (64-bit)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 or Visual Studio Code (optional)

#### Build Steps
```bash
# Clone the repository
git clone https://github.com/yourusername/TaskLogger.git
cd TaskLogger

# Build the application
dotnet build -c Release

# Run the application
dotnet run -c Release

# Or create a self-contained executable
dotnet publish -c Release -r win-x64 --self-contained
```

## Usage

### First Launch
1. On first launch, you'll be prompted to configure the database location
2. The application will minimize to the system tray
3. Click the tray icon to open the main window

### Logging Tasks
1. Click "Add Task" or use the keyboard shortcut
2. Enter task description and select a category
3. Tasks are automatically timestamped
4. View your task history in the History tab

### System Events
The application can automatically prompt for task logging on:
- System shutdown
- System restart
- User logoff
- End of work day (configurable time)

### Settings
Access settings through the gear icon to configure:
- Database location
- Startup with Windows
- System event triggers
- UI preferences
- Export options

## Keyboard Shortcuts

- `Ctrl+N` - Add new task
- `Ctrl+H` - View history
- `Ctrl+S` - Save current task
- `Ctrl+,` - Open settings
- `Esc` - Minimize to tray

## Data Storage

- **Database**: `%USERPROFILE%\Documents\TaskLogger\TaskLogger.db`
- **Configuration**: `%LOCALAPPDATA%\TaskLogger\config.json`
- **Logs**: `%LOCALAPPDATA%\TaskLogger\Logs\`

## Uninstallation

### If Installed with Installer
1. Run `uninstall.bat` from the installation directory
2. Or manually delete `%LOCALAPPDATA%\TaskLogger`

### Preserve Your Data
Your task database is stored separately in Documents and won't be deleted during uninstallation.

## Development

### Project Structure
```
TaskLogger/
├── App.xaml.cs           # Application entry point
├── Models/               # Data models
├── Views/                # WPF windows and controls
├── ViewModels/           # MVVM view models
├── Services/             # Business logic and services
├── Converters/           # WPF value converters
└── Utils/                # Utility classes
```

### Technologies Used
- **Framework**: .NET 8.0, WPF
- **Database**: Entity Framework Core with SQLite
- **UI Components**: Hardcodet.NotifyIcon.Wpf
- **Pattern**: MVVM (Model-View-ViewModel)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

If you encounter any issues or have questions:
1. Check the [Issues](https://github.com/yourusername/TaskLogger/issues) page
2. Create a new issue with:
   - Description of the problem
   - Steps to reproduce
   - System information (Windows version, .NET version)
   - Log files if applicable

## Acknowledgments

- Built with ❤️ using WPF and .NET 8.0
- Icons and UI inspired by modern Windows 11 design
- Thanks to all contributors and users

---

**Note**: Remember to update the GitHub repository URLs in this README with your actual repository information.