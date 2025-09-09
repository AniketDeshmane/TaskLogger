using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TaskLogger.Services;
using TaskLogger.Views;

namespace TaskLogger
{
    public partial class App : Application
    {
        private ILoggingService? _logger;

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                // Check for console test mode
                if (e.Args.Length > 0 && e.Args[0] == "--test-console")
                {
                    TestConsole.RunTest();
                    Shutdown(0);
                    return;
                }

                // Initialize logging first
                _logger = LoggingService.Instance;
                _logger.LogInfo("Application OnStartup called");

                // Set up global exception handlers
                SetupExceptionHandlers();

                // Log command line arguments
                if (e.Args.Length > 0)
                {
                    _logger.LogInfo($"Command line arguments: {string.Join(" ", e.Args)}");
                }

                // Set shutdown mode
                ShutdownMode = ShutdownMode.OnMainWindowClose;
                _logger.LogInfo("Shutdown mode set to OnMainWindowClose");

                // Call base startup
                base.OnStartup(e);
                _logger.LogInfo("Base OnStartup completed");

                // Clean up old logs
                Task.Run(() => (_logger as LoggingService)?.CleanupOldLogs());
            }
            catch (Exception ex)
            {
                // If logger fails, show message box
                var message = $"Fatal error during application startup:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
                MessageBox.Show(message, "Task Logger - Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Try to log if possible
                _logger?.LogFatal(ex, "Fatal error during application startup");
                
                // Shutdown the application
                Shutdown(1);
            }
        }

        private void SetupExceptionHandlers()
        {
            _logger?.LogInfo("Setting up global exception handlers");

            // Handle UI thread exceptions
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            _logger?.LogInfo("DispatcherUnhandledException handler registered");

            // Handle non-UI thread exceptions
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            _logger?.LogInfo("AppDomain.UnhandledException handler registered");

            // Handle task scheduler exceptions
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            _logger?.LogInfo("TaskScheduler.UnobservedTaskException handler registered");
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger?.LogError(e.Exception, "Unhandled UI thread exception");

            var message = $"An unexpected error occurred:\n\n{e.Exception.Message}\n\nThe application will continue running, but some features may not work correctly.";
            
            var result = MessageBox.Show(
                message + "\n\nWould you like to see the detailed error information?",
                "Task Logger - Error",
                MessageBoxButton.YesNo,
                MessageBoxImage.Error);

            if (result == MessageBoxResult.Yes)
            {
                ShowDetailedError(e.Exception);
            }

            // Mark as handled to prevent app crash
            e.Handled = true;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                _logger?.LogFatal(ex, $"Unhandled non-UI thread exception (IsTerminating: {e.IsTerminating})");

                if (e.IsTerminating)
                {
                    var message = $"A fatal error occurred and the application must close:\n\n{ex.Message}";
                    MessageBox.Show(message, "Task Logger - Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    
                    // Try to show log file location
                    ShowLogFileLocation();
                }
            }
            else
            {
                _logger?.LogFatal($"Unhandled non-UI thread exception of unknown type: {e.ExceptionObject?.GetType().Name ?? "null"}");
            }
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            _logger?.LogError(e.Exception, "Unobserved task exception");
            
            // Mark as observed to prevent process termination
            e.SetObserved();
        }

        private void ShowDetailedError(Exception ex)
        {
            var detailWindow = new Window
            {
                Title = "Task Logger - Error Details",
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var textBox = new System.Windows.Controls.TextBox
            {
                Text = $"Exception Type: {ex.GetType().FullName}\n\n" +
                       $"Message: {ex.Message}\n\n" +
                       $"Stack Trace:\n{ex.StackTrace}\n\n" +
                       $"Inner Exception: {ex.InnerException?.ToString() ?? "None"}\n\n" +
                       $"Log File: {(_logger as LoggingService)?.GetLogFilePath() ?? "Unknown"}",
                IsReadOnly = true,
                VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                Margin = new Thickness(10)
            };

            detailWindow.Content = textBox;
            detailWindow.ShowDialog();
        }

        private void ShowLogFileLocation()
        {
            try
            {
                var logPath = (_logger as LoggingService)?.GetLogFilePath();
                if (!string.IsNullOrEmpty(logPath))
                {
                    var result = MessageBox.Show(
                        $"Log file location:\n{logPath}\n\nWould you like to open the log folder?",
                        "Task Logger - Log File",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        var directory = System.IO.Path.GetDirectoryName(logPath);
                        if (!string.IsNullOrEmpty(directory))
                        {
                            System.Diagnostics.Process.Start("explorer.exe", directory);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error showing log file location");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _logger?.LogInfo($"Application exiting with code: {e.ApplicationExitCode}");
            _logger?.LogInfo("===========================================");
            base.OnExit(e);
        }
    }
}
