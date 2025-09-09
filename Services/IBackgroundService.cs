using System;

namespace TaskLogger.Services
{
    public interface IBackgroundService
    {
        void Start();
        void Stop();
        bool IsRunning { get; }
        event EventHandler<TaskPromptEventArgs>? TaskPromptRequested;
    }

    public class TaskPromptEventArgs : EventArgs
    {
        public string Reason { get; set; } = "";
        public bool IsUrgent { get; set; } = false;
    }
}
