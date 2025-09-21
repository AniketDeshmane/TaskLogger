using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace TaskLogger.Services
{
    public class StartupPromptService : IStartupPromptService
    {
        private const string PromptedKey = "StartupPrompted";
        private const string RegistryPath = @"SOFTWARE\TaskLogger";

        public bool ShouldPromptForStartup()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, false);
                return key?.GetValue(PromptedKey) == null;
            }
            catch
            {
                return true; // If we can't check, prompt to be safe
            }
        }

        public void MarkStartupPrompted()
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(RegistryPath);
                key?.SetValue(PromptedKey, "true");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error marking startup prompted: {ex.Message}");
            }
        }

        public void PromptForStartup(Action<bool> onResult)
        {
            var result = MessageBox.Show(
                "Would you like Task Logger to start automatically with Windows?\n\n" +
                "This will help ensure you never miss logging your tasks when shutting down your computer.",
                "Start with Windows?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.Yes);

            var shouldEnable = result == MessageBoxResult.Yes;
            onResult(shouldEnable);
            MarkStartupPrompted();
        }
    }
}
