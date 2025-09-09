using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using TaskLogger.Services;
using Microsoft.Win32;

namespace TaskLogger.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly IStartupService _startupService;
        private readonly ITaskService _taskService;
        private readonly IDatabaseConfigService _databaseConfigService;
        private readonly IThemeService _themeService;
        private bool _isStartupEnabled;
        private bool _showWeekendNotifications = true;
        private string _databasePath = "";
        private bool _isDarkMode;

        public SettingsViewModel(IStartupService startupService, ITaskService taskService)
        {
            _startupService = startupService;
            _taskService = taskService;
            _databaseConfigService = new DatabaseConfigService();
            _themeService = new ThemeService();
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

        public string DatabasePath
        {
            get => _databasePath;
            set => SetProperty(ref _databasePath, value);
        }
        
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (SetProperty(ref _isDarkMode, value))
                {
                    _themeService.IsDarkMode = value;
                }
            }
        }

        public ICommand SaveSettingsCommand => new RelayCommand(SaveSettings);
        public ICommand OpenDatabaseFolderCommand => new RelayCommand(OpenDatabaseFolder);
        public ICommand BrowseDatabaseCommand => new RelayCommand(BrowseDatabase);
        public ICommand ResetDatabaseLocationCommand => new RelayCommand(ResetDatabaseLocation);

        public event Action? SettingsSaved;
        public event PropertyChangedEventHandler? PropertyChanged;

        private void LoadSettings()
        {
            IsStartupEnabled = _startupService.IsStartupEnabled();
            DatabasePath = _databaseConfigService.GetDatabasePath();
            IsDarkMode = _themeService.IsDarkMode;
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

        private void OpenDatabaseFolder()
        {
            try
            {
                var folderPath = Path.GetDirectoryName(DatabasePath);
                
                if (Directory.Exists(folderPath))
                {
                    Process.Start("explorer.exe", folderPath!);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening database folder: {ex.Message}", "Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BrowseDatabase()
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Title = "Select Database Location",
                    Filter = "Database files (*.db)|*.db|All files (*.*)|*.*",
                    DefaultExt = "db",
                    FileName = "TaskLogger.db",
                    InitialDirectory = Path.GetDirectoryName(DatabasePath)
                };
                
                if (dialog.ShowDialog() == true)
                {
                    var newPath = dialog.FileName;
                    if (_databaseConfigService.ValidateDatabasePath(newPath))
                    {
                        _databaseConfigService.SetDatabasePath(newPath);
                        DatabasePath = newPath;
                        
                        MessageBox.Show("Database location changed successfully!\n\nPlease restart the application for the change to take effect.", 
                                      "Database Location Changed", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("The selected location is not valid or writable.", "Invalid Location", 
                                      MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error changing database location: {ex.Message}", "Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void ResetDatabaseLocation()
        {
            try
            {
                var result = MessageBox.Show("Are you sure you want to reset the database location to default?\n\nThis will not delete your existing data.", 
                                            "Reset Database Location", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    var defaultPath = _databaseConfigService.GetDefaultDatabasePath();
                    _databaseConfigService.SetDatabasePath(defaultPath);
                    DatabasePath = defaultPath;
                    
                    MessageBox.Show("Database location reset to default.\n\nPlease restart the application for the change to take effect.", 
                                  "Database Location Reset", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error resetting database location: {ex.Message}", "Error", 
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
