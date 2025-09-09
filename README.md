# Task Logger 📝

A beautiful and fast C# .NET WPF application for logging daily tasks and accomplishments. The app automatically triggers on system shutdown to capture your final thoughts for the day.

## Features ✨

- **Beautiful Modern UI**: Clean, intuitive interface with modern design principles
- **System Shutdown Detection**: Automatically prompts for task logging when shutting down
- **Weekend Mode**: Automatically disables logging on weekends to encourage rest
- **Task History**: View, search, and export your complete task history
- **Startup Integration**: Option to start with Windows for seamless experience
- **Data Persistence**: All tasks saved to SQLite database with advanced querying capabilities
- **Export Functionality**: Export your history as TXT or CSV files

## Screenshots 🖼️

The application features a modern, card-based UI with:
- Clean typography and spacing
- Intuitive color scheme
- Responsive design elements
- Status indicators and feedback

## Installation 🚀

### Option 1: Portable (No Installation Required)
1. Download `TaskLogger-Windows-x64.zip` from [Releases](https://github.com/AniketDeshmane/TaskLogger/releases)
2. Extract to any folder
3. Run `TaskLogger.exe` directly
4. The app will prompt you to configure database location on first run

### Option 2: Installer (Recommended)
1. Download `TaskLogger-Installer.zip` from [Releases](https://github.com/AniketDeshmane/TaskLogger/releases)
2. Extract and run `install.bat` as administrator
3. Creates desktop and start menu shortcuts
4. Installs to `%USERPROFILE%\TaskLogger\`

### Option 3: Build from Source
```bash
git clone https://github.com/AniketDeshmane/TaskLogger.git
cd TaskLogger
dotnet restore
dotnet build --configuration Release
dotnet publish --configuration Release --self-contained true --runtime win-x64
```

## Usage 📖

1. **Daily Logging**: Enter your tasks and accomplishments in the main window
2. **Automatic Shutdown Detection**: The app will prompt you when shutting down the system
3. **View History**: Click "View History" to see all your logged tasks
4. **Search & Filter**: Use the search box in history to find specific tasks
5. **Export Data**: Export your history as TXT or CSV for backup or analysis
6. **Settings**: Configure startup behavior and other preferences

## Configuration ⚙️

### First Run Setup
- **Database Location**: Choose where to store your task data
- **Startup Prompt**: Option to start with Windows automatically
- **System Tray**: App runs in background with minimal resources

### Settings
- Enable/disable automatic startup with Windows
- Configure notification preferences
- View and change database file location
- Automatic migration from old file-based system

### Weekend Mode
- Automatically disables logging on Saturdays and Sundays
- Shows friendly weekend messages
- Encourages work-life balance

## Technical Details 🔧

- **Framework**: .NET 8.0 WPF
- **Architecture**: MVVM pattern with proper separation of concerns
- **Data Storage**: SQLite database in Documents/TaskLogger/TaskLogger.db
- **System Integration**: Windows Registry for startup, SystemEvents for shutdown detection
- **UI Framework**: WPF with modern styling and data binding
- **Project Structure**: Organized into Views, ViewModels, Services, Models, Utils, and Converters folders

## Development 🛠️

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Windows 10/11

### Building
```bash
dotnet restore
dotnet build
dotnet run
```

### Testing
```bash
dotnet test
```

## CI/CD Pipeline 🔄

The project includes a GitHub Actions workflow that:
- Builds the application on every push
- Runs tests (if any)
- Creates release artifacts
- Publishes to GitHub Releases
- Generates installer packages

## Contributing 🤝

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License 📄

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.


## Support 💬

If you encounter any issues or have suggestions:
1. Check the [Issues](https://github.com/AniketDeshmane/TaskLogger/issues) page
2. Create a new issue with detailed information
3. Include system information and error messages

---

**Happy Task Logging!** 🎉
