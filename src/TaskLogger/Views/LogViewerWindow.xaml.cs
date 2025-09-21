using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using TaskLogger.Services;

namespace TaskLogger.Views
{
    public partial class LogViewerWindow : Window
    {
        private readonly ILoggingService _logger;
        private DispatcherTimer? _refreshTimer;
        private string _currentFilter = "All";
        private string _searchText = "";

        public LogViewerWindow()
        {
            InitializeComponent();
            _logger = LoggingService.Instance;
            
            LogFilePathText.Text = $"Log File: {(_logger as LoggingService)?.GetLogFilePath() ?? "Unknown"}";
            
            // Set up event handlers
            LogLevelFilter.SelectionChanged += (s, e) => ApplyFilter();
            SearchBox.TextChanged += (s, e) => ApplyFilter();
            
            // Load initial logs
            LoadLogs();
            
            // Set initial status
            UpdateStatus();
        }

        private void LoadLogs()
        {
            try
            {
                var logPath = (_logger as LoggingService)?.GetLogFilePath();
                if (!string.IsNullOrEmpty(logPath) && File.Exists(logPath))
                {
                    var allLines = File.ReadAllLines(logPath);
                    DisplayFilteredLogs(allLines);
                }
                else
                {
                    LogTextBox.Text = "No log file found or log file is empty.";
                }
            }
            catch (Exception ex)
            {
                LogTextBox.Text = $"Error loading logs: {ex.Message}";
            }
        }

        private void DisplayFilteredLogs(string[] allLines)
        {
            var filteredLines = allLines.AsEnumerable();

            // Apply level filter
            if (LogLevelFilter.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
            {
                _currentFilter = selectedItem.Content?.ToString() ?? "All";
                if (_currentFilter != "All")
                {
                    filteredLines = filteredLines.Where(line => line.Contains($"[{_currentFilter}") || line.Contains($"[{_currentFilter,-5}]"));
                }
            }

            // Apply search filter
            _searchText = SearchBox.Text;
            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                filteredLines = filteredLines.Where(line => 
                    line.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            var filteredArray = filteredLines.ToArray();
            LogTextBox.Text = string.Join(Environment.NewLine, filteredArray);
            
            // Auto-scroll to bottom
            LogScrollViewer.ScrollToEnd();
            
            UpdateStatus(filteredArray.Length, allLines.Length);
        }

        private void ApplyFilter()
        {
            LoadLogs();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadLogs();
            UpdateStatus();
        }

        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            LogLevelFilter.SelectedIndex = 0; // Select "All"
            SearchBox.Text = "";
            LoadLogs();
        }

        private void AutoRefreshCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _refreshTimer.Tick += (s, args) => LoadLogs();
            _refreshTimer.Start();
            UpdateStatus();
        }

        private void AutoRefreshCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _refreshTimer?.Stop();
            _refreshTimer = null;
            UpdateStatus();
        }

        private void OpenLogFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var logPath = (_logger as LoggingService)?.GetLogFilePath();
                if (!string.IsNullOrEmpty(logPath))
                {
                    var directory = Path.GetDirectoryName(logPath);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        System.Diagnostics.Process.Start("explorer.exe", directory);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening log folder: {ex.Message}", "Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void UpdateStatus(int displayed = 0, int total = 0)
        {
            var autoRefreshStatus = _refreshTimer != null ? " | Auto-refresh: ON" : "";
            
            if (displayed > 0 || total > 0)
            {
                var filterStatus = _currentFilter != "All" || !string.IsNullOrWhiteSpace(_searchText) 
                    ? $"Showing {displayed} of {total} lines" 
                    : $"Showing all {total} lines";
                StatusText.Text = $"{filterStatus}{autoRefreshStatus}";
            }
            else
            {
                StatusText.Text = $"Ready{autoRefreshStatus}";
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _refreshTimer?.Stop();
            base.OnClosed(e);
        }
    }
}