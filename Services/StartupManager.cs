using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace TaskLogger.Services
{
    public class StartupService : IStartupService
    {
        private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "TaskLogger";

        public bool IsStartupEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
                return key?.GetValue(AppName) != null;
            }
            catch
            {
                return false;
            }
        }

        public void EnableStartup()
        {
            try
            {
                var executablePath = Path.Combine(AppContext.BaseDirectory, "TaskLogger.exe");
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                key?.SetValue(AppName, executablePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to enable startup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void DisableStartup()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                key?.DeleteValue(AppName, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to disable startup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ToggleStartup()
        {
            if (IsStartupEnabled())
            {
                DisableStartup();
            }
            else
            {
                EnableStartup();
            }
        }
    }
}
