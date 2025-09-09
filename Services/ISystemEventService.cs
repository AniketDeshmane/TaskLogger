using System;

namespace TaskLogger.Services
{
    public interface ISystemEventService
    {
        event EventHandler<SystemShutdownEventArgs> SystemShutdown;
        event EventHandler<SystemSuspendEventArgs> SystemSuspend;
        void StartMonitoring();
        void StopMonitoring();
    }

    public class SystemShutdownEventArgs : EventArgs
    {
        public string Reason { get; set; } = "";
    }

    public class SystemSuspendEventArgs : EventArgs
    {
        public string Reason { get; set; } = "";
    }
}
