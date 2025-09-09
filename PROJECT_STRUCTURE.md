# Task Logger - Project Structure

This document outlines the organized folder structure of the Task Logger application.

## 📁 Folder Structure

```
TaskLogger/
├── 📁 Views/                    # WPF User Interface Views
│   ├── MainWindow.xaml          # Main application window
│   ├── MainWindow.xaml.cs       # Main window code-behind
│   ├── HistoryWindow.xaml       # Task history window
│   ├── HistoryWindow.xaml.cs    # History window code-behind
│   ├── SettingsWindow.xaml      # Settings configuration window
│   └── SettingsWindow.xaml.cs   # Settings window code-behind
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
│   └── StartupService.cs        # Windows startup integration
│
├── 📁 Models/                   # Data Models
│   └── TaskEntry.cs             # Task data model
│
├── 📁 Utils/                    # Utility Classes
│   └── DateTimeHelper.cs        # Date/time utility functions
│
├── 📁 Converters/               # WPF Value Converters
│   └── BoolToColorConverter.cs  # Boolean to color converter
│
├── 📁 .github/                  # GitHub Actions CI/CD
│   └── 📁 workflows/
│       └── build-and-publish.yml # Build and publish workflow
│
├── App.xaml                     # Application resources and styling
├── App.xaml.cs                  # Application entry point
├── TaskLogger.csproj            # Project configuration
├── README.md                    # Project documentation
├── PROJECT_STRUCTURE.md         # This file
└── .gitignore                   # Git ignore rules
```

## 🏗️ Architecture Overview

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

## 🔧 Key Components

### **Views**
- **MainWindow**: Primary task logging interface
- **HistoryWindow**: Task history viewing and export
- **SettingsWindow**: Application configuration

### **Services**
- **TaskService**: Handles task persistence and retrieval
- **SystemEventService**: Monitors system shutdown/suspend events
- **StartupService**: Manages Windows startup integration

### **Features**
- ✅ Modern WPF UI with data binding
- ✅ MVVM architecture
- ✅ Async/await patterns
- ✅ System event monitoring
- ✅ Data export functionality
- ✅ Weekend mode detection
- ✅ Startup integration
- ✅ GitHub Actions CI/CD

## 🚀 Benefits of This Structure

1. **Maintainability**: Clear separation of concerns
2. **Testability**: Services are easily mockable
3. **Scalability**: Easy to add new features
4. **Reusability**: Services can be reused across views
5. **Professional**: Industry-standard architecture
6. **CI/CD Ready**: Automated build and deployment

## 📝 Development Guidelines

- Keep Views minimal - only UI logic
- Put business logic in ViewModels
- Use Services for data access and external integrations
- Follow async/await patterns for I/O operations
- Implement proper error handling
- Use data binding instead of code-behind where possible
