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

        public async Task SaveTaskAsync(string task, string? eventType = null, string? notes = null)
        {
            var taskEntry = new TaskEntry
            {
                Task = task,
                EventType = eventType,
                Notes = notes,
                CreatedAt = DateTime.Now
            };

            _context.Tasks.Add(taskEntry);
            await _context.SaveChangesAsync();
        }

        public async Task<List<TaskEntry>> GetTasksAsync()
        {
            return await _context.Tasks
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
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

        public async Task ExportTasksAsync(string filePath, string format)
        {
            var tasks = await GetTasksAsync();
            var extension = Path.GetExtension(filePath).ToLower();

            if (extension == ".csv")
            {
                await ExportToCsvAsync(filePath, tasks);
            }
            else
            {
                await ExportToTextAsync(filePath, tasks);
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
            await writer.WriteLineAsync("Id,Timestamp,Task,EventType,Notes");
            
            foreach (var task in tasks)
            {
                var escapedTask = task.Task.Replace("\"", "\"\"");
                var escapedNotes = (task.Notes ?? "").Replace("\"", "\"\"");
                var eventType = task.EventType ?? "";
                
                await writer.WriteLineAsync($"\"{task.Id}\",\"{task.Timestamp}\",\"{escapedTask}\",\"{eventType}\",\"{escapedNotes}\"");
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
