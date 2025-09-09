using System;
using Microsoft.Win32;

namespace TaskLogger.Services
{
    public class SystemEventService : ISystemEventService
    {
        public event EventHandler<SystemShutdownEventArgs>? SystemShutdown;
        public event EventHandler<SystemSuspendEventArgs>? SystemSuspend;

        public void StartMonitoring()
        {
            SystemEvents.SessionEnding += OnSessionEnding;
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
        }

        public void StopMonitoring()
        {
            SystemEvents.SessionEnding -= OnSessionEnding;
            SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        }

        private void OnSessionEnding(object sender, SessionEndingEventArgs e)
        {
            var reason = e.Reason switch
            {
                SessionEndReasons.Logoff => "User Logoff",
                SessionEndReasons.SystemShutdown => "System Shutdown",
                SessionEndReasons.SystemReboot => "System Reboot",
                _ => "Unknown"
            };

            SystemShutdown?.Invoke(this, new SystemShutdownEventArgs { Reason = reason });
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    SystemSuspend?.Invoke(this, new SystemSuspendEventArgs { Reason = "System Suspend (Sleep)" });
                    break;
                case PowerModes.Resume:
                    // System is resuming from sleep/hibernate
                    System.Diagnostics.Debug.WriteLine("System resumed from sleep/hibernate");
                    break;
            }
        }
    }
}
