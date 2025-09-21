using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskLogger.Models;

namespace TaskLogger.Services
{
    public class DatabaseService
    {
        private readonly TaskLoggerDbContext _context;
        private readonly ILoggingService _logger;

        public DatabaseService()
        {
            _logger = LoggingService.Instance;
            _logger.LogDebug("DatabaseService constructor called");
            
            try
            {
                _context = new TaskLoggerDbContext();
                _logger.LogDebug("TaskLoggerDbContext created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create TaskLoggerDbContext");
                throw;
            }
        }

        public async Task InitializeDatabaseAsync()
        {
            try
            {
                _logger.LogInfo("Starting database initialization");
                
                // Log connection string (without sensitive data)
                var connectionString = _context.Database.GetConnectionString();
                _logger.LogDebug($"Database connection string: {connectionString}");
                
                // Ensure database is created
                _logger.LogInfo("Ensuring database exists...");
                var created = await _context.Database.EnsureCreatedAsync();
                _logger.LogInfo(created ? "Database created successfully" : "Database already exists");
                
                // Check if we need to migrate from old file-based system
                await MigrateFromFileSystemAsync();
                
                _logger.LogInfo("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing database");
                throw;
            }
        }

        private async Task MigrateFromFileSystemAsync()
        {
            try
            {
                _logger.LogInfo("Checking for old file-based logs to migrate");
                var oldLogFile = GetOldLogFilePath();
                _logger.LogDebug($"Looking for old log file at: {oldLogFile}");
                
                if (System.IO.File.Exists(oldLogFile))
                {
                    _logger.LogInfo($"Found old log file to migrate: {oldLogFile}");
                    var lines = await System.IO.File.ReadAllLinesAsync(oldLogFile);
                    _logger.LogInfo($"Read {lines.Length} lines from old log file");
                    
                    var hasExistingData = await _context.Tasks.AnyAsync();
                    
                    // Only migrate if database is empty
                    if (!hasExistingData)
                    {
                        _logger.LogInfo("Database is empty, starting migration");
                        int migratedCount = 0;
                        foreach (var line in lines)
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            var parts = line.Split(new[] { " - " }, 2, StringSplitOptions.None);
                            if (parts.Length == 2)
                            {
                                var timestampStr = parts[0];
                                var taskText = parts[1];
                                
                                if (DateTime.TryParse(timestampStr, out var timestamp))
                                {
                                    var taskEntry = new TaskEntry
                                    {
                                        Task = taskText,
                                        CreatedAt = timestamp,
                                        EventType = taskText.StartsWith("[") && taskText.Contains("]") ? 
                                                   ExtractEventType(taskText) : null,
                                        Notes = taskText.StartsWith("[") && taskText.Contains("]") ? 
                                               "Migrated from file system" : null
                                    };

                                    _context.Tasks.Add(taskEntry);
                                    migratedCount++;
                                }
                            }
                        }
                        
                        await _context.SaveChangesAsync();
                        _logger.LogInfo($"Successfully migrated {migratedCount} tasks to database");
                        
                        // Backup the old file
                        var backupFile = oldLogFile + ".backup";
                        System.IO.File.Move(oldLogFile, backupFile);
                        _logger.LogInfo($"Old log file backed up to: {backupFile}");
                    }
                    else
                    {
                        _logger.LogInfo("Database already has data, skipping migration");
                    }
                }
                else
                {
                    _logger.LogDebug("No old log file found, nothing to migrate");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error migrating from file system: {ex.Message}");
                // Don't throw - migration failure shouldn't break the app
            }
        }

        private string ExtractEventType(string taskText)
        {
            if (taskText.StartsWith("[") && taskText.Contains("]"))
            {
                var endIndex = taskText.IndexOf("]");
                if (endIndex > 1)
                {
                    return taskText.Substring(1, endIndex - 1);
                }
            }
            return null;
        }

        private string GetOldLogFilePath()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return System.IO.Path.Combine(documentsPath, "TaskLogger", "task_log.txt");
        }

        public async Task<DatabaseStats> GetDatabaseStatsAsync()
        {
            var totalTasks = await _context.Tasks.CountAsync();
            var todayTasks = await _context.Tasks
                .CountAsync(t => t.CreatedAt.Date == DateTime.Today);
            var thisWeekTasks = await _context.Tasks
                .CountAsync(t => t.CreatedAt >= DateTime.Today.AddDays(-7));
            var shutdownTasks = await _context.Tasks
                .CountAsync(t => t.EventType == "System Shutdown");

            return new DatabaseStats
            {
                TotalTasks = totalTasks,
                TodayTasks = todayTasks,
                ThisWeekTasks = thisWeekTasks,
                ShutdownTasks = shutdownTasks
            };
        }

        public void Dispose()
        {
            _logger.LogDebug("DatabaseService.Dispose called");
            _context?.Dispose();
        }
    }

    public class DatabaseStats
    {
        public int TotalTasks { get; set; }
        public int TodayTasks { get; set; }
        public int ThisWeekTasks { get; set; }
        public int ShutdownTasks { get; set; }
    }
}
