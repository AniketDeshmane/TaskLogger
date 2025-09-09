using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskLogger.Models;

namespace TaskLogger.Services
{
    public interface ITaskService
    {
        Task SaveTaskAsync(string task, string? eventType = null, string? notes = null);
        Task<List<TaskEntry>> GetTasksAsync();
        Task<List<TaskEntry>> SearchTasksAsync(string searchText);
        Task<List<TaskEntry>> GetTasksByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<TaskEntry>> GetTasksByEventTypeAsync(string eventType);
        Task<TaskEntry?> GetTaskByIdAsync(int id);
        Task UpdateTaskAsync(TaskEntry task);
        Task DeleteTaskAsync(int id);
        Task ExportTasksAsync(string filePath, string format);
        Task<int> GetTaskCountAsync();
        Task<List<TaskEntry>> GetRecentTasksAsync(int count = 10);
        string GetDatabasePath();
    }
}
