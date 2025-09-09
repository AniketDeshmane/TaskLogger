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
        private readonly ILoggingService _logger;

        public MainWindow()
        {
            _logger = LoggingService.Instance;
            _logger.LogInfo("MainWindow constructor started");
            
            try
            {
                _logger.LogInfo("Calling InitializeComponent");
                InitializeComponent();
                _logger.LogInfo("InitializeComponent completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogFatal(ex, "Failed to initialize MainWindow components");
                MessageBox.Show($"Failed to initialize window: {ex.Message}\n\nCheck the log file for details.", 
                              "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
            
            try
            {
                // Initialize services
                _logger.LogInfo("Initializing services...");
                
                _logger.LogDebug("Creating SystemTrayService");
                _systemTrayService = new SystemTrayService();
                
                _logger.LogDebug("Creating StartupPromptService");
                _startupPromptService = new StartupPromptService();
                
                _logger.LogDebug("Creating StartupService");
                _startupService = new StartupService();
                
                _logger.LogDebug("Creating DatabaseConfigService");
                _databaseConfigService = new DatabaseConfigService();
                
                _logger.LogInfo("All services initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogFatal(ex, "Failed to initialize services");
                MessageBox.Show($"Failed to initialize services: {ex.Message}", "Fatal Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
            
            // Check database configuration first
            try
            {
                _logger.LogInfo("Checking database configuration");
                if (!_databaseConfigService.IsDatabasePathConfigured())
                {
                    _logger.LogInfo("Database path not configured, showing configuration window");
                    ShowDatabaseConfiguration();
                }
                else
                {
                    _logger.LogInfo($"Database already configured at: {_databaseConfigService.GetDatabasePath()}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking database configuration");
                MessageBox.Show($"Error checking database configuration: {ex.Message}", "Error", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
            // Initialize database
            _logger.LogInfo("Starting database initialization");
            InitializeDatabaseAsync();
            
            // Initialize other services
            try
            {
                _logger.LogInfo("Creating TaskService");
                var taskService = new TaskService();
                
                _logger.LogInfo("Creating SystemEventService");
                var systemEventService = new SystemEventService();
            
                // Initialize background service
                _logger.LogInfo("Creating BackgroundService");
                _backgroundService = new BackgroundService(systemEventService, taskService, _systemTrayService);
            
                // Initialize ViewModel
                _logger.LogInfo("Creating MainViewModel");
                _viewModel = new MainViewModel(taskService, systemEventService);
                DataContext = _viewModel;
                _logger.LogInfo("DataContext set to MainViewModel");
            
                // Subscribe to ViewModel events
                _logger.LogInfo("Subscribing to ViewModel events");
                _viewModel.ViewHistory += OnViewHistory;
                _viewModel.OpenSettings += OnOpenSettings;
            
                // Initialize system tray
                _logger.LogInfo("Initializing system tray");
                InitializeSystemTray();
            
                // Start background service
                _logger.LogInfo("Starting background service");
                _backgroundService.Start();
                _backgroundService.TaskPromptRequested += OnTaskPromptRequested;
                _logger.LogInfo("Background service started");
            
                // Check for startup prompt
                _logger.LogInfo("Checking startup prompt");
                CheckStartupPrompt();
                
                _logger.LogInfo("MainWindow initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogFatal(ex, "Fatal error during MainWindow initialization");
                MessageBox.Show($"Fatal error during initialization: {ex.Message}\n\nThe application will now close.", 
                              "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(1);
            }
        }

        private async void InitializeDatabaseAsync()
        {
            try
            {
                _logger.LogInfo("InitializeDatabaseAsync started");
                var databaseService = new DatabaseService();
                await databaseService.InitializeDatabaseAsync();
                databaseService.Dispose();
                _logger.LogInfo("Database initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing database");
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

        private void DebugLogsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger.LogInfo("Opening debug log viewer window");
                var logViewerWindow = new LogViewerWindow();
                logViewerWindow.Owner = this;
                logViewerWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening log viewer");
                MessageBox.Show($"Error opening log viewer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeSystemTray()
        {
            try
            {
                _logger.LogDebug("Initializing system tray service");
                _systemTrayService.Initialize();
                _systemTrayService.ShowRequested += OnShowFromTray;
                _systemTrayService.ExitRequested += OnExitRequested;
                _logger.LogDebug("System tray initialized and events subscribed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize system tray");
                throw;
            }
        }

        private void ShowDatabaseConfiguration()
        {
            try
            {
                _logger.LogInfo("Showing database configuration window");
                var configWindow = new DatabaseConfigWindow();
                configWindow.Owner = this;
                
                var result = configWindow.ShowDialog();
                if (result != true)
                {
                    // User cancelled, use default path
                    var defaultPath = _databaseConfigService.GetDefaultDatabasePath();
                    _logger.LogInfo($"User cancelled, using default path: {defaultPath}");
                    _databaseConfigService.SetDatabasePath(defaultPath);
                }
                else
                {
                    _logger.LogInfo($"Database path configured: {_databaseConfigService.GetDatabasePath()}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing database configuration");
                throw;
            }
        }

        private void CheckStartupPrompt()
        {
            try
            {
                if (_startupPromptService.ShouldPromptForStartup())
                {
                    _logger.LogInfo("Showing startup prompt to user");
                    _startupPromptService.PromptForStartup(shouldEnable =>
                    {
                        if (shouldEnable)
                        {
                            _logger.LogInfo("User chose to enable startup with Windows");
                            _startupService.EnableStartup();
                            _systemTrayService.ShowBalloonTip("Task Logger", 
                                "Task Logger will now start automatically with Windows!", 
                                BalloonIcon.Info);
                        }
                        else
                        {
                            _logger.LogInfo("User chose not to enable startup with Windows");
                        }
                    });
                }
                else
                {
                    _logger.LogDebug("Startup prompt not needed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking startup prompt");
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
            _logger.LogInfo("MainWindow OnClosed called");
            
            try
            {
                _backgroundService?.Stop();
                _backgroundService?.Dispose();
                _logger.LogDebug("Background service stopped and disposed");
                
                _viewModel?.Dispose();
                _logger.LogDebug("ViewModel disposed");
                
                _systemTrayService?.Dispose();
                _logger.LogDebug("System tray service disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during MainWindow cleanup");
            }
            
            _logger.LogInfo("MainWindow closed");
            base.OnClosed(e);
        }
    }
}