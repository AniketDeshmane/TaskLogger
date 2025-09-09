using System;
using System.Windows;
using System.Windows.Input;
using TaskLogger.ViewModels;
using TaskLogger.Services;

namespace TaskLogger.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsWindow()
        {
            InitializeComponent();
            
            // Initialize services
            var startupService = new StartupService();
            var taskService = new TaskService();
            
            // Initialize ViewModel
            _viewModel = new SettingsViewModel(startupService, taskService);
            DataContext = _viewModel;
            
            // Subscribe to ViewModel events
            _viewModel.SettingsSaved += OnSettingsSaved;
        }

        private void OnSettingsSaved()
        {
            MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}