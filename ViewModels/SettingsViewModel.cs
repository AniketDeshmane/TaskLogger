using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TaskLogger.Services;

namespace TaskLogger.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly IStartupService _startupService;
        private readonly ITaskService _taskService;
        private bool _isStartupEnabled;
        private bool _showWeekendNotifications = true;
        private string _logFilePath = "";

        public SettingsViewModel(IStartupService startupService, ITaskService taskService)
        {
            _startupService = startupService;
            _taskService = taskService;
            LoadSettings();
        }

        public bool IsStartupEnabled
        {
            get => _isStartupEnabled;
            set => SetProperty(ref _isStartupEnabled, value);
        }

        public bool ShowWeekendNotifications
        {
            get => _showWeekendNotifications;
            set => SetProperty(ref _showWeekendNotifications, value);
        }

        public string LogFilePath
        {
            get => _logFilePath;
            set => SetProperty(ref _logFilePath, value);
        }

        public ICommand SaveSettingsCommand => new RelayCommand(SaveSettings);
        public ICommand OpenLogFolderCommand => new RelayCommand(OpenLogFolder);
        public ICommand ChangeDatabaseLocationCommand => new RelayCommand(ChangeDatabaseLocation);

        public event Action? SettingsSaved;
        public event PropertyChangedEventHandler? PropertyChanged;

        private void LoadSettings()
        {
            IsStartupEnabled = _startupService.IsStartupEnabled();
            LogFilePath = _taskService.GetDatabasePath();
        }

        private void SaveSettings()
        {
            try
            {
                if (IsStartupEnabled)
                {
                    _startupService.EnableStartup();
                }
                else
                {
                    _startupService.DisableStartup();
                }

                SettingsSaved?.Invoke();
            }
            catch (Exception ex)
            {
                // Handle error - could show message to user
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        private void OpenLogFolder()
        {
            try
            {
                var folderPath = Path.GetDirectoryName(LogFilePath);
                
                if (Directory.Exists(folderPath))
                {
                    Process.Start("explorer.exe", folderPath!);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening log folder: {ex.Message}");
            }
        }

        private void ChangeDatabaseLocation()
        {
            try
            {
                var configWindow = new Views.DatabaseConfigWindow();
                var result = configWindow.ShowDialog();
                
                if (result == true)
                {
                    // Reload the database path
                    LoadSettings();
                    MessageBox.Show("Database location changed successfully!\n\nNote: You may need to restart the application for the change to take full effect.", 
                                  "Database Location Changed", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error changing database location: {ex.Message}", "Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
