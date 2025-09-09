using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using TaskLogger.Utils;

namespace TaskLogger.Services
{
    public class BackgroundService : IBackgroundService
    {
        private readonly ISystemEventService _systemEventService;
        private readonly ITaskService _taskService;
        private readonly ISystemTrayService _systemTrayService;
        private Timer? _periodicTimer;
        private bool _isRunning;

        public bool IsRunning => _isRunning;
        public event EventHandler<TaskPromptEventArgs>? TaskPromptRequested;

        public BackgroundService(ISystemEventService systemEventService, ITaskService taskService, ISystemTrayService systemTrayService)
        {
            _systemEventService = systemEventService;
            _taskService = taskService;
            _systemTrayService = systemTrayService;
        }

        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;

            // Subscribe to system events
            _systemEventService.SystemShutdown += OnSystemShutdown;
            _systemEventService.SystemSuspend += OnSystemSuspend;
            _systemEventService.StartMonitoring();

            // Start periodic check for task prompts (every 30 minutes)
            _periodicTimer = new Timer(OnPeriodicCheck, null, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));

            System.Diagnostics.Debug.WriteLine("Background service started");
        }

        public void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;

            // Unsubscribe from system events
            _systemEventService.SystemShutdown -= OnSystemShutdown;
            _systemEventService.SystemSuspend -= OnSystemSuspend;
            _systemEventService.StopMonitoring();

            // Stop periodic timer
            _periodicTimer?.Dispose();
            _periodicTimer = null;

            System.Diagnostics.Debug.WriteLine("Background service stopped");
        }

        private async void OnSystemShutdown(object? sender, SystemShutdownEventArgs e)
        {
            if (DateTimeHelper.IsWeekend()) return;

            // Show urgent task prompt for shutdown
            TaskPromptRequested?.Invoke(this, new TaskPromptEventArgs 
            { 
                Reason = $"System {e.Reason.ToLower()} detected", 
                IsUrgent = true 
            });

            // Also save any pending task automatically
            await SavePendingTaskAsync($"System {e.Reason.ToLower()}");
        }

        private async void OnSystemSuspend(object? sender, SystemSuspendEventArgs e)
        {
            if (DateTimeHelper.IsWeekend()) return;

            // Show task prompt for suspend
            TaskPromptRequested?.Invoke(this, new TaskPromptEventArgs 
            { 
                Reason = "System going to sleep", 
                IsUrgent = false 
            });

            // Save any pending task
            await SavePendingTaskAsync("System suspend");
        }

        private void OnPeriodicCheck(object? state)
        {
            // Check if it's been a while since last task (e.g., 4 hours during work hours)
            if (!DateTimeHelper.IsWeekend() && IsWorkHours())
            {
                CheckForTaskReminder();
            }
        }

        private async void CheckForTaskReminder()
        {
            try
            {
                var recentTasks = await _taskService.GetRecentTasksAsync(1);
                if (recentTasks.Count == 0) return;

                var lastTask = recentTasks[0];
                var timeSinceLastTask = DateTime.Now - lastTask.CreatedAt;

                // If it's been more than 4 hours since last task during work hours
                if (timeSinceLastTask.TotalHours > 4)
                {
                    TaskPromptRequested?.Invoke(this, new TaskPromptEventArgs 
                    { 
                        Reason = "It's been a while since your last task entry", 
                        IsUrgent = false 
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking for task reminder: {ex.Message}");
            }
        }

        private bool IsWorkHours()
        {
            var now = DateTime.Now;
            return now.Hour >= 9 && now.Hour <= 17; // 9 AM to 5 PM
        }

        private async Task SavePendingTaskAsync(string eventType)
        {
            try
            {
                // This would need to be implemented to save any pending task from the UI
                // For now, we'll just log the event
                await _taskService.SaveTaskAsync($"System event: {eventType}", eventType, "Background service detected");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving pending task: {ex.Message}");
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
