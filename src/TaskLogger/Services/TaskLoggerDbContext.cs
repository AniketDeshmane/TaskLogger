using Microsoft.EntityFrameworkCore;
using TaskLogger.Models;
using System;
using System.IO;

namespace TaskLogger.Services
{
    public class TaskLoggerDbContext : DbContext
    {
        private readonly ILoggingService _logger;
        public DbSet<TaskEntry> Tasks { get; set; }

        public TaskLoggerDbContext()
        {
            _logger = LoggingService.Instance;
            _logger.LogDebug("TaskLoggerDbContext default constructor called");
        }

        public TaskLoggerDbContext(DbContextOptions<TaskLoggerDbContext> options) : base(options)
        {
            _logger = LoggingService.Instance;
            _logger.LogDebug("TaskLoggerDbContext options constructor called");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            _logger?.LogDebug("OnConfiguring called");
            
            if (!optionsBuilder.IsConfigured)
            {
                var dbPath = GetDatabasePath();
                _logger?.LogInfo($"Configuring database with path: {dbPath}");
                
                // Ensure directory exists
                var directory = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    _logger?.LogInfo($"Creating database directory: {directory}");
                    Directory.CreateDirectory(directory);
                }
                
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
                _logger?.LogDebug("SQLite provider configured");
            }
            else
            {
                _logger?.LogDebug("Options builder already configured");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _logger?.LogDebug("OnModelCreating called");
            base.OnModelCreating(modelBuilder);

            // Configure TaskEntry entity
            modelBuilder.Entity<TaskEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Task).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.EventType).HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(100);
                
                // Create index on CreatedAt for better query performance
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_Tasks_CreatedAt");
                
                // Create index on EventType for filtering
                entity.HasIndex(e => e.EventType).HasDatabaseName("IX_Tasks_EventType");
            });
            
            _logger?.LogDebug("Model configuration completed");
        }

        private static string GetDatabasePath()
        {
            var logger = LoggingService.Instance;
            
            try
            {
                logger.LogDebug("Getting database path from configuration");
                var configService = new DatabaseConfigService();
                var path = configService.GetDatabasePath();
                logger.LogDebug($"Database path from config: {path}");
                return path;
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Failed to get database path from config: {ex.Message}");
                
                // Fallback to default path
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var appDataPath = Path.Combine(documentsPath, "TaskLogger");
                Directory.CreateDirectory(appDataPath);
                var defaultPath = Path.Combine(appDataPath, "TaskLogger.db");
                
                logger.LogWarning($"Using fallback database path: {defaultPath}");
                return defaultPath;
            }
        }

        public static string GetDatabasePathStatic()
        {
            try
            {
                var configService = new DatabaseConfigService();
                return configService.GetDatabasePath();
            }
            catch
            {
                // Fallback to default path
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var appDataPath = Path.Combine(documentsPath, "TaskLogger");
                Directory.CreateDirectory(appDataPath);
                return Path.Combine(appDataPath, "TaskLogger.db");
            }
        }
    }
}
