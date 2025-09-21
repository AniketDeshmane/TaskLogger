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
                    Filter = "JSON files (*.json)|*.json|Excel files (*.xlsx)|*.xlsx|CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    DefaultExt = "json",
                    FileName = $"TaskHistory_{DateTime.Now:yyyyMMdd}"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    await _viewModel.ExportToFileAsync(saveDialog.FileName);
                    MessageBox.Show($"History exported successfully to:\n{saveDialog.FileName}", 
                                  "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is ViewModels.TaskEntryViewModel taskEntry)
            {
                taskEntry.StartEdit();
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is ViewModels.TaskEntryViewModel taskEntry)
            {
                try
                {
                    taskEntry.SaveEdit();
                    await _viewModel.UpdateTaskAsync(taskEntry);
                    MessageBox.Show("Task updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating task: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is ViewModels.TaskEntryViewModel taskEntry)
            {
                taskEntry.CancelEdit();
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is ViewModels.TaskEntryViewModel taskEntry)
            {
                var result = MessageBox.Show($"Are you sure you want to delete this task?\n\n{taskEntry.Task}", 
                                            "Confirm Delete", 
                                            MessageBoxButton.YesNo, 
                                            MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _viewModel.DeleteTaskAsync(taskEntry);
                        MessageBox.Show("Task deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting task: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}