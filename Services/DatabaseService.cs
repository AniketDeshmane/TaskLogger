using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskLogger.Models;

namespace TaskLogger.Services
{
    public class DatabaseService
    {
        private readonly TaskLoggerDbContext _context;

        public DatabaseService()
        {
            _context = new TaskLoggerDbContext();
        }

        public async Task InitializeDatabaseAsync()
        {
            try
            {
                // Ensure database is created
                await _context.Database.EnsureCreatedAsync();
                
                // Check if we need to migrate from old file-based system
                await MigrateFromFileSystemAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing database: {ex.Message}");
                throw;
            }
        }

        private async Task MigrateFromFileSystemAsync()
        {
            try
            {
                var oldLogFile = GetOldLogFilePath();
                if (System.IO.File.Exists(oldLogFile))
                {
                    var lines = await System.IO.File.ReadAllLinesAsync(oldLogFile);
                    var hasExistingData = await _context.Tasks.AnyAsync();
                    
                    // Only migrate if database is empty
                    if (!hasExistingData)
                    {
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
                                }
                            }
                        }
                        
                        await _context.SaveChangesAsync();
                        
                        // Backup the old file
                        var backupFile = oldLogFile + ".backup";
                        System.IO.File.Move(oldLogFile, backupFile);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error migrating from file system: {ex.Message}");
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
