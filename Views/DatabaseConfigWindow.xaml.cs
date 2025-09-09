using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using TaskLogger.Services;

namespace TaskLogger.Views
{
    public partial class DatabaseConfigWindow : Window
    {
        private readonly IDatabaseConfigService _databaseConfigService;
        private string _databasePath = "";
        private bool _isPathValid = false;

        public DatabaseConfigWindow()
        {
            InitializeComponent();
            _databaseConfigService = new DatabaseConfigService();
            
            // Set default path
            _databasePath = _databaseConfigService.GetDefaultDatabasePath();
            DatabasePathTextBox.Text = _databasePath;
            
            ValidatePath();
        }

        public string DatabasePath => _databasePath;
        public bool IsPathValid => _isPathValid;

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Title = "Choose Database Location",
                Filter = "SQLite Database (*.db)|*.db|All files (*.*)|*.*",
                DefaultExt = "db",
                FileName = Path.GetFileName(_databasePath),
                InitialDirectory = Path.GetDirectoryName(_databasePath)
            };

            if (saveDialog.ShowDialog() == true)
            {
                _databasePath = saveDialog.FileName;
                DatabasePathTextBox.Text = _databasePath;
                ValidatePath();
            }
        }

        private void UseDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            _databasePath = _databaseConfigService.GetDefaultDatabasePath();
            DatabasePathTextBox.Text = _databasePath;
            ValidatePath();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isPathValid)
            {
                MessageBox.Show("Please select a valid database location.", "Invalid Path", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Save the database path
                _databaseConfigService.SetDatabasePath(_databasePath);
                
                // Migrate existing database if needed
                if (CreateBackupCheckBox.IsChecked == true)
                {
                    MigrateExistingDatabase();
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving database configuration: {ex.Message}", "Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ValidatePath()
        {
            _isPathValid = _databaseConfigService.ValidateDatabasePath(_databasePath);
            
            if (_isPathValid)
            {
                // Check if file already exists
                if (File.Exists(_databasePath))
                {
                    var fileInfo = new FileInfo(_databasePath);
                    PathValidationMessage.Text = $"✅ Valid path. Existing database found ({fileInfo.Length / 1024} KB, modified {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm})";
                    PathValidationBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGreen);
                    PathValidationTextBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkGreen);
                }
                else
                {
                    PathValidationMessage.Text = "✅ Valid path. New database will be created.";
                    PathValidationBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightBlue);
                    PathValidationTextBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkBlue);
                }
            }
            else
            {
                PathValidationMessage.Text = "❌ Invalid path. Please choose a valid location.";
                PathValidationBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightCoral);
                PathValidationTextBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkRed);
            }
        }

        private void MigrateExistingDatabase()
        {
            try
            {
                var defaultPath = _databaseConfigService.GetDefaultDatabasePath();
                
                // If the new path is the same as default, no migration needed
                if (string.Equals(_databasePath, defaultPath, StringComparison.OrdinalIgnoreCase))
                    return;

                // If default database exists and new path is different, create backup
                if (File.Exists(defaultPath))
                {
                    var backupPath = defaultPath + ".backup." + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    File.Copy(defaultPath, backupPath);
                    
                    // If new path doesn't exist, copy the database
                    if (!File.Exists(_databasePath))
                    {
                        File.Copy(defaultPath, _databasePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Warning: Could not migrate existing database: {ex.Message}", "Migration Warning", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Properties for data binding
        public string PathValidationMessage { get; set; } = "";
        public System.Windows.Media.Brush PathValidationBrush { get; set; } = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Transparent);
        public System.Windows.Media.Brush PathValidationTextBrush { get; set; } = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
    }
}
