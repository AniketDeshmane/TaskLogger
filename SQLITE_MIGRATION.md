# SQLite Database Migration

This document outlines the migration from file-based persistence to SQLite database for the Task Logger application.

## 🗄️ Database Schema

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

## 🔄 Migration Features

### **Automatic Migration**
- Detects existing `task_log.txt` file
- Migrates all historical data to SQLite
- Preserves timestamps and task content
- Creates backup of original file
- Only migrates if database is empty

### **Enhanced Data Model**
- **ID**: Auto-incrementing primary key
- **Task**: Task description with 1000 character limit
- **CreatedAt**: Precise DateTime instead of string
- **EventType**: Tracks how task was created (manual, shutdown, suspend)
- **Notes**: Additional context information

## 🚀 New Features

### **Advanced Querying**
- Search by task content, notes, or event type
- Filter by date ranges
- Filter by event type
- Get task statistics and counts

### **Better Performance**
- Indexed queries for fast searching
- Efficient pagination support
- Optimized database operations

### **Enhanced Export**
- CSV export includes all fields (ID, timestamp, task, event type, notes)
- Better formatted text export
- Maintains backward compatibility

## 📁 File Structure Changes

### **New Files**
- `Services/TaskLoggerDbContext.cs` - Entity Framework context
- `Services/DatabaseService.cs` - Database initialization and migration
- `Converters/StringToVisibilityConverter.cs` - UI converter for optional fields

### **Updated Files**
- `Models/TaskEntry.cs` - Enhanced with Entity Framework attributes
- `Services/ITaskService.cs` - Extended interface with new methods
- `Services/TaskService.cs` - Complete rewrite for SQLite
- `ViewModels/HistoryViewModel.cs` - Added task count display
- `Views/HistoryWindow.xaml` - Enhanced UI with event type and notes display

## 🔧 Technical Implementation

### **Entity Framework Core**
- SQLite provider for cross-platform compatibility
- Code-first approach with automatic migrations
- Optimized queries with proper indexing

### **Database Location**
- **Path**: `%USERPROFILE%\Documents\TaskLogger\TaskLogger.db`
- **Backup**: Original file renamed to `task_log.txt.backup`
- **Auto-creation**: Database and tables created automatically

### **Migration Process**
1. Check for existing `task_log.txt`
2. Parse file content line by line
3. Extract timestamps and task descriptions
4. Detect event types from task content
5. Insert into SQLite database
6. Backup original file
7. Continue with normal operation

## 📊 Benefits

### **Performance**
- ✅ Faster queries with indexed columns
- ✅ Efficient pagination for large datasets
- ✅ Optimized search operations

### **Reliability**
- ✅ ACID transactions
- ✅ Data integrity constraints
- ✅ Automatic backup and recovery

### **Functionality**
- ✅ Advanced filtering and searching
- ✅ Rich data model with metadata
- ✅ Better export capabilities
- ✅ Task statistics and analytics

### **Maintainability**
- ✅ Structured data model
- ✅ Easy to extend with new fields
- ✅ Standard database operations
- ✅ Professional data access patterns

## 🔄 Backward Compatibility

- Existing file-based data is automatically migrated
- Original file is preserved as backup
- Export formats remain compatible
- UI maintains same functionality with enhancements

## 🛠️ Development Notes

### **Dependencies Added**
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
```

### **Database Initialization**
- Automatic on first run
- Error handling for database issues
- Graceful fallback if migration fails

### **Future Enhancements**
- Database statistics dashboard
- Advanced filtering UI
- Task categories and tags
- Data analytics and reporting
- Cloud sync capabilities
