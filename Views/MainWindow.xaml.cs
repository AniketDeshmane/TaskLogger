using System;
using System.Windows;
using TaskLogger.ViewModels;
using TaskLogger.Services;

namespace TaskLogger.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly ISystemTrayService _systemTrayService;
        private readonly IStartupPromptService _startupPromptService;
        private readonly IStartupService _startupService;
        private readonly IBackgroundService _backgroundService;
        private readonly IDatabaseConfigService _databaseConfigService;

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize services
            _systemTrayService = new SystemTrayService();
            _startupPromptService = new StartupPromptService();
            _startupService = new StartupService();
            _databaseConfigService = new DatabaseConfigService();
            
            // Check database configuration first
            if (!_databaseConfigService.IsDatabasePathConfigured())
            {
                ShowDatabaseConfiguration();
            }
            
            // Initialize database
            InitializeDatabaseAsync();
            
            // Initialize other services
            var taskService = new TaskService();
            var systemEventService = new SystemEventService();
            
            // Initialize background service
            _backgroundService = new BackgroundService(systemEventService, taskService, _systemTrayService);
            
            // Initialize ViewModel
            _viewModel = new MainViewModel(taskService, systemEventService);
            DataContext = _viewModel;
            
            // Subscribe to ViewModel events
            _viewModel.ViewHistory += OnViewHistory;
            _viewModel.OpenSettings += OnOpenSettings;
            
            // Initialize system tray
            InitializeSystemTray();
            
            // Start background service
            _backgroundService.Start();
            _backgroundService.TaskPromptRequested += OnTaskPromptRequested;
            
            // Check for startup prompt
            CheckStartupPrompt();
        }

        private async void InitializeDatabaseAsync()
        {
            try
            {
                var databaseService = new DatabaseService();
                await databaseService.InitializeDatabaseAsync();
                databaseService.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing database: {ex.Message}", "Database Error", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnViewHistory()
        {
            try
            {
                var historyWindow = new HistoryWindow();
                historyWindow.Owner = this;
                historyWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnOpenSettings()
        {
            try
            {
                var settingsWindow = new SettingsWindow();
                settingsWindow.Owner = this;
                settingsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeSystemTray()
        {
            _systemTrayService.Initialize();
            _systemTrayService.ShowRequested += OnShowFromTray;
            _systemTrayService.ExitRequested += OnExitRequested;
        }

        private void ShowDatabaseConfiguration()
        {
            var configWindow = new DatabaseConfigWindow();
            configWindow.Owner = this;
            
            var result = configWindow.ShowDialog();
            if (result != true)
            {
                // User cancelled, use default path
                _databaseConfigService.SetDatabasePath(_databaseConfigService.GetDefaultDatabasePath());
            }
        }

        private void CheckStartupPrompt()
        {
            if (_startupPromptService.ShouldPromptForStartup())
            {
                _startupPromptService.PromptForStartup(shouldEnable =>
                {
                    if (shouldEnable)
                    {
                        _startupService.EnableStartup();
                        _systemTrayService.ShowBalloonTip("Task Logger", 
                            "Task Logger will now start automatically with Windows!", 
                            BalloonIcon.Info);
                    }
                });
            }
        }

        private void OnShowFromTray(object? sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
            _systemTrayService.ShowFromTray();
        }

        private void OnExitRequested(object? sender, EventArgs e)
        {
            Close();
        }

        private void OnTaskPromptRequested(object? sender, TaskPromptEventArgs e)
        {
            // Show the main window and bring to front
            Show();
            WindowState = WindowState.Normal;
            Activate();
            _systemTrayService.ShowFromTray();

            // Show notification
            var icon = e.IsUrgent ? BalloonIcon.Warning : BalloonIcon.Info;
            _systemTrayService.ShowBalloonTip("Task Logger", e.Reason, icon);
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                _systemTrayService.HideToTray();
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Ask user if they want to minimize to tray or exit
            var result = MessageBox.Show(
                "Do you want to minimize Task Logger to the system tray?\n\n" +
                "Click 'Yes' to keep it running in the background, or 'No' to exit completely.",
                "Task Logger",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.Yes);

            if (result == MessageBoxResult.Yes)
            {
                e.Cancel = true;
                Hide();
                _systemTrayService.HideToTray();
            }
            else
            {
                _systemTrayService.Dispose();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _backgroundService?.Stop();
            _backgroundService?.Dispose();
            _viewModel?.Dispose();
            _systemTrayService?.Dispose();
            base.OnClosed(e);
        }
    }
}