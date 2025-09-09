using System;
using System.Windows;
using TaskLogger.ViewModels;
using TaskLogger.Services;

namespace TaskLogger.Views
{
    public partial class HistoryWindow : Window
    {
        private readonly HistoryViewModel _viewModel;

        public HistoryWindow()
        {
            InitializeComponent();
            
            // Initialize services
            var taskService = new TaskService();
            
            // Initialize ViewModel
            _viewModel = new HistoryViewModel(taskService);
            DataContext = _viewModel;
            
            // Subscribe to ViewModel events
            _viewModel.ExportRequested += OnExportRequested;
        }

        private async void OnExportRequested(string format)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Export Task History",
                    Filter = "Text files (*.txt)|*.txt|CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    DefaultExt = "txt",
                    FileName = $"TaskHistory_{DateTime.Now:yyyyMMdd}.txt"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    await _viewModel.ExportCommand.Execute(saveDialog.FileName);
                    MessageBox.Show($"History exported successfully to:\n{saveDialog.FileName}", 
                                  "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}