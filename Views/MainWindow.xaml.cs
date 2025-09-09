using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using TaskLogger.ViewModels;
using TaskLogger.Services;

namespace TaskLogger.Views
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly MainViewModel _viewModel;
        private readonly ISystemTrayService _systemTrayService;
        private readonly IStartupPromptService _startupPromptService;
        private readonly IStartupService _startupService;
        private readonly IBackgroundService _backgroundService;
        private readonly IDatabaseConfigService _databaseConfigService;
        private readonly ILoggingService _logger;
        private readonly IThemeService _themeService;
        private bool _showInTaskbar = true;
        
        public bool ShowInTaskbar
        {
            get => _showInTaskbar;
            set
            {
                if (_showInTaskbar != value)
                {
                    _showInTaskbar = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public bool IsDarkMode => _themeService?.IsDarkMode ?? false;
        
        public event PropertyChangedEventHandler PropertyChanged;

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
                
                _logger.LogDebug("Creating ThemeService");
                _themeService = new ThemeService();
                _themeService.LoadThemePreference();
                
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

        private void TaskTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Check if Enter key is pressed (without Shift for multi-line support)
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if ((System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Shift) == System.Windows.Input.ModifierKeys.Shift)
                {
                    // Shift+Enter: Add new line
                    var textBox = sender as System.Windows.Controls.TextBox;
                    if (textBox != null)
                    {
                        int caretIndex = textBox.CaretIndex;
                        textBox.Text = textBox.Text.Insert(caretIndex, Environment.NewLine);
                        textBox.CaretIndex = caretIndex + Environment.NewLine.Length;
                        e.Handled = true;
                    }
                }
                else
                {
                    // Enter only: Save the task
                    if (_viewModel.SaveTaskCommand.CanExecute(null))
                    {
                        _viewModel.SaveTaskCommand.Execute(null);
                    }
                    e.Handled = true;
                }
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
                // Don't set Owner since MainWindow hasn't been shown yet
                configWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                
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
            ShowInTaskbar = true;
            WindowState = WindowState.Normal;
            Activate();
            _systemTrayService.ShowFromTray();
        }

        private void OnExitRequested(object? sender, EventArgs e)
        {
            // Force close the application
            _systemTrayService?.Dispose();
            Application.Current.Shutdown();
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
                ShowInTaskbar = false;
                _systemTrayService.HideToTray();
            }
        }
        
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
            else
            {
                DragMove();
            }
        }
        
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            if (_themeService != null && sender is System.Windows.Controls.Primitives.ToggleButton toggle)
            {
                _themeService.IsDarkMode = toggle.IsChecked ?? false;
                OnPropertyChanged(nameof(IsDarkMode));
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Always minimize to tray on close to keep background service running
            e.Cancel = true;
            Hide();
            ShowInTaskbar = false;
            _systemTrayService.HideToTray();
            _systemTrayService.ShowBalloonTip("Task Logger", 
                "Task Logger is still running in the background. Right-click the tray icon to exit.", 
                BalloonIcon.Info);
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
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}