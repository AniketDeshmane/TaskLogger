# Task Logger - Installation Options

This document explains the different ways to install and run Task Logger.

## 🚀 Installation Options

### **Option 1: Portable (No Installation Required)**
**Best for**: Users who want to run the app immediately without installation

**How it works**:
- Download `TaskLogger-Windows-x64.zip`
- Extract to any folder
- Run `TaskLogger.exe` directly
- App prompts for database location on first run

**Pros**:
- ✅ No installation required
- ✅ Can run from USB drive
- ✅ No system modifications
- ✅ Easy to remove (just delete folder)

**Cons**:
- ❌ No desktop shortcuts
- ❌ No start menu integration
- ❌ Must manually create shortcuts

### **Option 2: Installer (Recommended)**
**Best for**: Users who want a proper Windows application experience

**How it works**:
- Download `TaskLogger-Installer.zip`
- Run `install.bat` as administrator
- Creates shortcuts and installs to user folder
- App prompts for database location on first run

**Pros**:
- ✅ Desktop shortcut created
- ✅ Start menu integration
- ✅ Professional installation
- ✅ Easy uninstall with `uninstall.bat`
- ✅ Proper Windows application experience

**Cons**:
- ❌ Requires administrator privileges
- ❌ Modifies system (shortcuts, registry)

## 🗄️ Database Configuration

### **First Run Experience**
1. **Database Location Prompt**: App asks where to store the database
2. **Path Validation**: Ensures the chosen location is writable
3. **Migration Support**: Automatically migrates existing data
4. **Backup Creation**: Creates backup of existing database

### **Database Location Options**
- **Default**: `%USERPROFILE%\Documents\TaskLogger\TaskLogger.db`
- **Custom**: User can choose any writable location
- **Network**: Can be stored on network drives (if accessible)
- **USB**: Can be stored on removable drives

### **Settings Integration**
- **View Current Location**: Settings window shows current database path
- **Change Location**: "Change Location" button in settings
- **Open Folder**: Quick access to database folder
- **Migration**: Automatic migration when changing location

## 🔧 Technical Details

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

## 📁 File Structure

### **Portable Installation**
```
TaskLogger/
├── TaskLogger.exe
├── TaskLogger.dll
├── [Other .NET files]
└── [User's chosen database location]
```

### **Installed Mode**
```
%USERPROFILE%\TaskLogger\
├── TaskLogger.exe
├── TaskLogger.dll
├── [Other .NET files]
└── [User's chosen database location]

%USERPROFILE%\Desktop\
└── Task Logger.lnk

%APPDATA%\Microsoft\Windows\Start Menu\Programs\
└── Task Logger.lnk
```

## 🎯 User Experience

### **First Run (Both Modes)**
1. **Database Configuration**: Choose database location
2. **Startup Prompt**: Option to start with Windows
3. **System Tray**: App minimizes to tray
4. **Background Monitoring**: Continues monitoring system events

### **Daily Usage**
- **System Tray**: App runs in background
- **Event Monitoring**: Detects shutdown/sleep events
- **Task Logging**: Quick task entry when needed
- **History Viewing**: Access complete task history

### **Settings Management**
- **Database Location**: View and change database path
- **Startup Settings**: Enable/disable Windows startup
- **Notification Settings**: Configure reminder preferences
- **Export Options**: Export data in various formats

## 🔄 Migration and Backup

### **Automatic Migration**
- **File to Database**: Migrates old `task_log.txt` to SQLite
- **Backup Creation**: Original file renamed to `.backup`
- **Data Preservation**: All historical data preserved
- **Error Handling**: Graceful fallback if migration fails

### **Location Changes**
- **Database Migration**: Moves database to new location
- **Backup Creation**: Creates timestamped backup
- **Validation**: Ensures new location is accessible
- **Rollback**: Can restore from backup if needed

## 🚀 Benefits

### **Flexibility**
- ✅ **Portable**: Run from anywhere
- ✅ **Installed**: Professional Windows integration
- ✅ **Customizable**: Choose database location
- ✅ **Migratable**: Easy to move between locations

### **User Experience**
- ✅ **First Run Setup**: Guided configuration
- ✅ **Professional Installation**: Proper Windows app experience
- ✅ **Easy Management**: Simple settings and configuration
- ✅ **Data Safety**: Automatic backups and migration

### **System Integration**
- ✅ **System Tray**: Professional background operation
- ✅ **Startup Integration**: Optional Windows startup
- ✅ **Event Monitoring**: Comprehensive system event detection
- ✅ **Registry Integration**: Proper Windows application behavior
