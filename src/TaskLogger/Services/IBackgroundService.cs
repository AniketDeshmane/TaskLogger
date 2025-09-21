using System;

namespace TaskLogger.Services
{
    public interface IBackgroundService : IDisposable
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
