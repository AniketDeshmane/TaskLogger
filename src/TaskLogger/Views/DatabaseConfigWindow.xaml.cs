using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using TaskLogger.Services;

namespace TaskLogger.Views
{
    public partial class DatabaseConfigWindow : Window, System.ComponentModel.INotifyPropertyChanged
    {
        private readonly IConfigService _configService;
        private string _databasePath = "";
        private bool _isPathValid = false;

        public DatabaseConfigWindow()
        {
            InitializeComponent();
            _configService = new ConfigService();
            
            // Set default path
            _databasePath = _configService.GetDefaultDatabasePath();
            DatabasePathTextBox.Text = _databasePath;
            
            ValidatePath();
            DataContext = this;
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
            _databasePath = _configService.GetDefaultDatabasePath();
            DatabasePathTextBox.Text = _databasePath;
            ValidatePath();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Title = "Select Existing Database",
                Filter = "SQLite Database (*.db)|*.db|All files (*.*)|*.*",
                DefaultExt = "db"
            };

            if (openDialog.ShowDialog() == true)
            {
                var existingDbPath = openDialog.FileName;
                var newDbPath = DatabasePathTextBox.Text;

                try
                {
                    if (File.Exists(newDbPath))
                    {
                        var result = MessageBox.Show("A database already exists at the target location. Do you want to overwrite it with the selected database?",
                                                     "Overwrite Database?",
                                                     MessageBoxButton.YesNo,
                                                     MessageBoxImage.Warning);
                        if (result == MessageBoxResult.No)
                        {
                            return;
                        }
                    }

                    File.Copy(existingDbPath, newDbPath, true);
                    MessageBox.Show("Database loaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ValidatePath();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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
                _configService.SetDatabasePath(_databasePath);
                
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
            _isPathValid = _configService.ValidateDatabasePath(_databasePath);
            
            if (_isPathValid)
            {
                // Check if file already exists
                if (File.Exists(_databasePath))
                {
                    var fileInfo = new FileInfo(_databasePath);
                    PathValidationMessage = $"✅ Valid path. Existing database found ({fileInfo.Length / 1024} KB, modified {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm})";
                    PathValidationBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGreen);
                    PathValidationTextBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkGreen);
                }
                else
                {
                    PathValidationMessage = "✅ Valid path. New database will be created.";
                    PathValidationBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightBlue);
                    PathValidationTextBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkBlue);
                }
            }
            else
            {
                PathValidationMessage = "❌ Invalid path. Please choose a valid location.";
                PathValidationBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightCoral);
                PathValidationTextBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkRed);
            }
        }

        private void MigrateExistingDatabase()
        {
            try
            {
                var defaultPath = _configService.GetDefaultDatabasePath();
                
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
        private string _pathValidationMessage = "";
        public string PathValidationMessage
        {
            get => _pathValidationMessage;
            set
            {
                _pathValidationMessage = value;
                OnPropertyChanged();
            }
        }

        private System.Windows.Media.Brush _pathValidationBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Transparent);
        public System.Windows.Media.Brush PathValidationBrush
        {
            get => _pathValidationBrush;
            set
            {
                _pathValidationBrush = value;
                OnPropertyChanged();
            }
        }

        private System.Windows.Media.Brush _pathValidationTextBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
        public System.Windows.Media.Brush PathValidationTextBrush
        {
            get => _pathValidationTextBrush;
            set
            {
                _pathValidationTextBrush = value;
                OnPropertyChanged();
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
