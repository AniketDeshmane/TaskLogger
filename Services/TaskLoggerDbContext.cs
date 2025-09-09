using Microsoft.EntityFrameworkCore;
using TaskLogger.Models;
using System.IO;

namespace TaskLogger.Services
{
    public class TaskLoggerDbContext : DbContext
    {
        public DbSet<TaskEntry> Tasks { get; set; }

        public TaskLoggerDbContext()
        {
        }

        public TaskLoggerDbContext(DbContextOptions<TaskLoggerDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var dbPath = GetDatabasePath();
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
        }

        private static string GetDatabasePath()
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
