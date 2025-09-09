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
- **System Tray Integration**: Runs in background with minimal resources
- **Database Configuration**: Choose where to store your task data

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

## Project Structure 📁

```
TaskLogger/
├── 📁 Views/                    # WPF User Interface Views
│   ├── MainWindow.xaml          # Main application window
│   ├── MainWindow.xaml.cs       # Main window code-behind
│   ├── HistoryWindow.xaml       # Task history window
│   ├── HistoryWindow.xaml.cs    # History window code-behind
│   ├── SettingsWindow.xaml      # Settings configuration window
│   ├── SettingsWindow.xaml.cs   # Settings window code-behind
│   ├── DatabaseConfigWindow.xaml # Database configuration window
│   └── DatabaseConfigWindow.xaml.cs # Database config code-behind
│
├── 📁 ViewModels/               # MVVM ViewModels
│   ├── MainViewModel.cs         # Main window business logic
│   ├── HistoryViewModel.cs      # History window business logic
│   └── SettingsViewModel.cs     # Settings window business logic
│
├── 📁 Services/                 # Business Logic Services
│   ├── ITaskService.cs          # Task service interface
│   ├── TaskService.cs           # Task data operations
│   ├── ISystemEventService.cs   # System events interface
│   ├── SystemEventService.cs    # System shutdown/suspend detection
│   ├── IStartupService.cs       # Startup management interface
│   ├── StartupService.cs        # Windows startup integration
│   ├── ISystemTrayService.cs    # System tray interface
│   ├── SystemTrayService.cs     # System tray management
│   ├── IBackgroundService.cs    # Background operations interface
│   ├── BackgroundService.cs     # Background monitoring
│   ├── IDatabaseConfigService.cs # Database configuration interface
│   ├── DatabaseConfigService.cs # Database path management
│   ├── IStartupPromptService.cs # Startup prompt interface
│   ├── StartupPromptService.cs  # First-launch startup prompt
│   ├── TaskLoggerDbContext.cs   # Entity Framework context
│   └── DatabaseService.cs       # Database initialization and migration
│
├── 📁 Models/                   # Data Models
│   └── TaskEntry.cs             # Task data model with Entity Framework
│
├── 📁 Utils/                    # Utility Classes
│   └── DateTimeHelper.cs        # Date/time utility functions
│
├── 📁 Converters/               # WPF Value Converters
│   ├── BoolToColorConverter.cs  # Boolean to color converter
│   └── StringToVisibilityConverter.cs # String to visibility converter
│
├── 📁 .github/                  # GitHub Actions CI/CD
│   └── 📁 workflows/
│       └── build-and-publish.yml # Build and publish workflow
│
├── App.xaml                     # Application resources and styling
├── App.xaml.cs                  # Application entry point
├── TaskLogger.csproj            # Project configuration
├── install.bat                  # Installation script
├── uninstall.bat                # Uninstallation script
├── README.md                    # This file
└── .gitignore                   # Git ignore rules
```

## Architecture Overview 🏗️

### **MVVM Pattern**
- **Views**: WPF XAML files with minimal code-behind
- **ViewModels**: Business logic and data binding
- **Models**: Data structures and entities
- **Services**: Business logic and data access

### **Separation of Concerns**
- **Views**: Pure UI presentation
- **ViewModels**: UI logic and data binding
- **Services**: Business logic and external integrations
- **Models**: Data structures
- **Utils**: Helper functions
- **Converters**: WPF value converters

### **Dependency Injection Ready**
All services implement interfaces, making the application ready for dependency injection containers.

## Database Schema 🗄️

### TaskEntry Table
```sql
CREATE TABLE Tasks (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Task TEXT NOT NULL,                    -- The task description (max 1000 chars)
    CreatedAt DATETIME NOT NULL,           -- When the task was created
    EventType TEXT,                        -- Event type (e.g., "System Shutdown", "System Suspend")
    Notes TEXT                             -- Additional notes (max 100 chars)
);

-- Indexes for better performance
CREATE INDEX IX_Tasks_CreatedAt ON Tasks(CreatedAt);
CREATE INDEX IX_Tasks_EventType ON Tasks(EventType);
```

## Enhanced Features 🚀

### **System Tray Integration**
- **Tray Icon**: App minimizes to system tray instead of closing
- **Context Menu**: Right-click for Show/Exit options
- **Balloon Tips**: Notifications for important events
- **Click to Show**: Left-click tray icon to restore window
- **Graceful Exit**: Proper cleanup when exiting

### **Background Operation**
- **Minimal Resources**: Runs efficiently in background
- **Event Monitoring**: Continues monitoring system events when hidden
- **Periodic Checks**: Reminds users to log tasks every 4 hours during work hours
- **Smart Notifications**: Different urgency levels for different events

### **Enhanced System Event Detection**
- **Shutdown Detection**: System Shutdown, Reboot, User Logoff
- **Sleep/Hibernate**: Detects when system goes to sleep
- **Resume Detection**: Logs when system resumes from sleep
- **Background Monitoring**: Works even when main window is hidden

### **Database Configuration**
- **First Run Setup**: Choose database location on startup
- **Path Validation**: Ensures chosen location is writable
- **Migration Support**: Automatically migrates existing data
- **Backup Creation**: Creates backup of existing database
- **Settings Integration**: View and change database path

## Installation Options 🔧

### **Portable Mode**
- **Self-contained**: All dependencies included
- **No Registry**: Uses file-based configuration
- **Portable Database**: Can be moved with the application
- **Zero Installation**: Runs from any location

### **Installed Mode**
- **System Integration**: Desktop and start menu shortcuts
- **Registry Settings**: Stores configuration in Windows registry
- **User Profile**: Installs to `%USERPROFILE%\TaskLogger\`
- **Proper Uninstall**: Clean removal with uninstall script

### **Database Management**
- **SQLite Database**: Professional database with indexing
- **Automatic Migration**: Migrates from old file-based system
- **Backup Creation**: Automatic backups when changing location
- **Path Validation**: Ensures database location is accessible

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

## Dependencies 📦

### **Core Dependencies**
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="Hardcodet.NotifyIcon.Wpf" Version="1.1.0" />
<PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
<PackageReference Include="System.Management" Version="8.0.0" />
```

## Benefits 🎉

### **User Experience**
- ✅ **Seamless Integration**: Works like a professional system utility
- ✅ **Never Miss Events**: Monitors system events even when hidden
- ✅ **Smart Reminders**: Periodic prompts during work hours
- ✅ **Easy Access**: One-click restore from tray
- ✅ **Professional Installation**: Proper Windows app experience

### **System Integration**
- ✅ **Startup Integration**: Optional automatic startup
- ✅ **Tray Integration**: Professional system tray behavior
- ✅ **Event Monitoring**: Comprehensive system event detection
- ✅ **Resource Efficient**: Minimal impact when running in background
- ✅ **Registry Integration**: Proper Windows application behavior

### **Data Management**
- ✅ **SQLite Database**: Professional database with indexing
- ✅ **Automatic Migration**: Migrates from old file-based system
- ✅ **Backup Creation**: Automatic backups when changing location
- ✅ **Path Validation**: Ensures database location is accessible
- ✅ **Advanced Querying**: Search, filter, and export capabilities

### **Reliability**
- ✅ **Persistent Monitoring**: Works even when window is closed
- ✅ **Event Capture**: Never misses shutdown/sleep events
- ✅ **Graceful Handling**: Proper cleanup and error handling
- ✅ **User Control**: User decides tray vs exit behavior
- ✅ **ACID Transactions**: Data integrity and reliability

## Contributing 🤝

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License 📄

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments 🙏

- Inspired by the Python reference implementation
- Built with modern C# and WPF technologies
- Designed for productivity and user experience

## Support 💬

If you encounter any issues or have suggestions:
1. Check the [Issues](https://github.com/AniketDeshmane/TaskLogger/issues) page
2. Create a new issue with detailed information
3. Include system information and error messages

---

**Happy Task Logging!** 🎉