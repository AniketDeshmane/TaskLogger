using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskLogger.Models;

namespace TaskLogger.Services
{
    public class TaskService : ITaskService
    {
        private readonly TaskLoggerDbContext _context;

        public TaskService()
        {
            _context = new TaskLoggerDbContext();
            _context.Database.EnsureCreated();
        }

        public TaskService(TaskLoggerDbContext context)
        {
            _context = context;
        }

        public async Task SaveTaskAsync(string task, string? eventType = null, string? notes = null)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            
            // Check if there's already an entry for today
            var existingEntry = await _context.Tasks
                .Where(t => t.CreatedAt >= today && t.CreatedAt < tomorrow)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();
            
            if (existingEntry != null && string.IsNullOrEmpty(eventType))
            {
                // Append to existing entry for today (only for manual entries)
                existingEntry.Task = existingEntry.Task + "\n" + task;
                existingEntry.CreatedAt = DateTime.Now; // Update timestamp to latest
                _context.Tasks.Update(existingEntry);
            }
            else
            {
                // Create new entry
                var taskEntry = new TaskEntry
                {
                    Task = task,
                    EventType = eventType,
                    Notes = notes,
                    CreatedAt = DateTime.Now
                };
                _context.Tasks.Add(taskEntry);
            }
            
            await _context.SaveChangesAsync();
        }

        public async Task<List<TaskEntry>> GetTasksAsync()
        {
            var tasks = await _context.Tasks
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
            
            // Format multi-line tasks for display
            foreach (var task in tasks)
            {
                if (task.Task.Contains('\n'))
                {
                    var lines = task.Task.Split('\n');
                    task.Task = string.Join("\n• ", lines);
                    if (!task.Task.StartsWith("• "))
                        task.Task = "• " + task.Task;
                }
            }
            
            return tasks;
        }

        public async Task<List<TaskEntry>> SearchTasksAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return await GetTasksAsync();
            }

            var searchLower = searchText.ToLower();
            return await _context.Tasks
                .Where(t => t.Task.ToLower().Contains(searchLower) ||
                           (t.Notes != null && t.Notes.ToLower().Contains(searchLower)) ||
                           (t.EventType != null && t.EventType.ToLower().Contains(searchLower)))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<TaskEntry>> GetTasksByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Tasks
                .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<TaskEntry>> GetTasksByEventTypeAsync(string eventType)
        {
            return await _context.Tasks
                .Where(t => t.EventType == eventType)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<TaskEntry?> GetTaskByIdAsync(int id)
        {
            return await _context.Tasks.FindAsync(id);
        }

        public async Task UpdateTaskAsync(TaskEntry task)
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTaskAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetTaskCountAsync()
        {
            return await _context.Tasks.CountAsync();
        }

        public async Task<List<TaskEntry>> GetRecentTasksAsync(int count = 10)
        {
            return await _context.Tasks
                .OrderByDescending(t => t.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<TaskEntry?> GetTodayTaskAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            
            return await _context.Tasks
                .Where(t => t.CreatedAt >= today && t.CreatedAt < tomorrow && string.IsNullOrEmpty(t.EventType))
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task ExportTasksAsync(string filePath, string format)
        {
            var tasks = await GetTasksAsync();
            var extension = Path.GetExtension(filePath).ToLower();

            switch (extension)
            {
                case ".json":
                    await ExportToJsonAsync(filePath, tasks);
                    break;
                case ".csv":
                    await ExportToCsvAsync(filePath, tasks);
                    break;
                case ".xlsx":
                    await ExportToExcelAsync(filePath, tasks);
                    break;
                case ".txt":
                default:
                    await ExportToTextAsync(filePath, tasks);
                    break;
            }
        }

        private async Task ExportToTextAsync(string filePath, List<TaskEntry> tasks)
        {
            using var writer = new StreamWriter(filePath);
            await writer.WriteLineAsync("Task Logger - History Export");
            await writer.WriteLineAsync($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            await writer.WriteLineAsync($"Total Tasks: {tasks.Count}");
            await writer.WriteLineAsync(new string('=', 50));
            await writer.WriteLineAsync();

            foreach (var task in tasks)
            {
                var eventInfo = !string.IsNullOrEmpty(task.EventType) ? $"[{task.EventType}] " : "";
                var notesInfo = !string.IsNullOrEmpty(task.Notes) ? $" ({task.Notes})" : "";
                await writer.WriteLineAsync($"{task.Timestamp} - {eventInfo}{task.Task}{notesInfo}");
            }
        }

        private async Task ExportToCsvAsync(string filePath, List<TaskEntry> tasks)
        {
            using var writer = new StreamWriter(filePath);
            await writer.WriteLineAsync("Id,Date,Time,Task,EventType,Notes");
            
            foreach (var task in tasks)
            {
                var escapedTask = task.Task.Replace("\"", "\"\"");
                var escapedNotes = (task.Notes ?? "").Replace("\"", "\"\"");
                var eventType = task.EventType ?? "Manual";
                
                await writer.WriteLineAsync($"\"{task.Id}\",\"{task.CreatedAt:yyyy-MM-dd}\",\"{task.CreatedAt:HH:mm:ss}\",\"{escapedTask}\",\"{eventType}\",\"{escapedNotes}\"");
            }
        }

        private async Task ExportToJsonAsync(string filePath, List<TaskEntry> tasks)
        {
            var jsonData = tasks.Select(t => new
            {
                t.Id,
                Date = t.CreatedAt.ToString("yyyy-MM-dd"),
                Time = t.CreatedAt.ToString("HH:mm:ss"),
                t.Task,
                EventType = t.EventType ?? "Manual",
                Notes = t.Notes ?? ""
            });

            var json = System.Text.Json.JsonSerializer.Serialize(jsonData, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            await File.WriteAllTextAsync(filePath, json);
        }

        private async Task ExportToExcelAsync(string filePath, List<TaskEntry> tasks)
        {
            // For Excel, we'll create a simple CSV that Excel can open
            // True Excel format would require additional libraries
            using var writer = new StreamWriter(filePath);
            await writer.WriteLineAsync("sep=,"); // Excel hint for CSV separator
            await writer.WriteLineAsync("Id,Date,Time,Task,Event Type,Notes");
            
            foreach (var task in tasks)
            {
                var escapedTask = task.Task.Replace("\"", "\"\"");
                var escapedNotes = (task.Notes ?? "").Replace("\"", "\"\"");
                var eventType = task.EventType ?? "Manual";
                
                await writer.WriteLineAsync($"{task.Id},\"{task.CreatedAt:yyyy-MM-dd}\",\"{task.CreatedAt:HH:mm:ss}\",\"{escapedTask}\",\"{eventType}\",\"{escapedNotes}\"");
            }
        }

        public string GetDatabasePath()
        {
            return TaskLoggerDbContext.GetDatabasePathStatic();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
