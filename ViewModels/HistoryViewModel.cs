using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskLogger.Models;
using TaskLogger.Services;

namespace TaskLogger.ViewModels
{
    public class HistoryViewModel : INotifyPropertyChanged
    {
        private readonly ITaskService _taskService;
        private ObservableCollection<TaskEntryViewModel> _tasks = new();
        private string _searchText = "";
        private bool _isLoading;
        private string _taskCountText = "";

        public HistoryViewModel(ITaskService taskService)
        {
            _taskService = taskService;
            LoadTasksAsync();
        }

        public ObservableCollection<TaskEntryViewModel> Tasks
        {
            get => _tasks;
            set => SetProperty(ref _tasks, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterTasksAsync();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string TaskCountText
        {
            get => _taskCountText;
            set => SetProperty(ref _taskCountText, value);
        }

        public ICommand ClearSearchCommand => new RelayCommand(() => SearchText = "");
        public ICommand ExportCommand => new AsyncRelayCommand<string>(ExportAsync);

        public event Action<string>? ExportRequested;
        public event PropertyChangedEventHandler? PropertyChanged;

        private async Task LoadTasksAsync()
        {
            IsLoading = true;
            try
            {
                var tasks = await _taskService.GetTasksAsync();
                var viewModels = tasks.Select(t => new TaskEntryViewModel(t));
                Tasks = new ObservableCollection<TaskEntryViewModel>(viewModels);
                UpdateTaskCountText();
            }
            catch (Exception ex)
            {
                // Handle error - could show message to user
                System.Diagnostics.Debug.WriteLine($"Error loading tasks: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task FilterTasksAsync()
        {
            IsLoading = true;
            try
            {
                var tasks = await _taskService.SearchTasksAsync(SearchText);
                var viewModels = tasks.Select(t => new TaskEntryViewModel(t));
                Tasks = new ObservableCollection<TaskEntryViewModel>(viewModels);
                UpdateTaskCountText();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error filtering tasks: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateTaskCountText()
        {
            var count = Tasks.Count;
            var totalCount = _taskService.GetTaskCountAsync().Result;
            
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                TaskCountText = $"Total tasks: {totalCount}";
            }
            else
            {
                TaskCountText = $"Showing {count} of {totalCount} tasks";
            }
        }

        private async Task ExportAsync(string? format)
        {
            try
            {
                // Trigger the event to let the view handle the file dialog
                ExportRequested?.Invoke(format ?? "");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting tasks: {ex.Message}");
                throw;
            }
            await Task.CompletedTask;
        }

        public async Task ExportToFileAsync(string filePath)
        {
            try
            {
                await _taskService.ExportTasksAsync(filePath, Path.GetExtension(filePath));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting to file: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateTaskAsync(TaskEntryViewModel taskViewModel)
        {
            try
            {
                await _taskService.UpdateTaskAsync(taskViewModel.TaskEntry);
                await LoadTasksAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating task: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteTaskAsync(TaskEntryViewModel taskViewModel)
        {
            try
            {
                await _taskService.DeleteTaskAsync(taskViewModel.Id);
                Tasks.Remove(taskViewModel);
                UpdateTaskCountText();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting task: {ex.Message}");
                throw;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T?, Task> _execute;
        private readonly Func<T?, bool>? _canExecute;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<T?, Task> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke((T?)parameter) ?? true);
        }

        public async void Execute(object? parameter)
        {
            if (_isExecuting) return;
            
            _isExecuting = true;
            CommandManager.InvalidateRequerySuggested();
            
            try
            {
                await _execute((T?)parameter);
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}
