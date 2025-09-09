using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskLogger.Services;
using TaskLogger.Utils;

namespace TaskLogger.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ITaskService _taskService;
        private readonly ISystemEventService _systemEventService;
        private string _taskText = "";
        private string _statusText = "Ready to log tasks";
        private bool _isWeekend;
        private string _dateText = "";
        private bool _isStatusSuccess = true;

        public MainViewModel(ITaskService taskService, ISystemEventService systemEventService)
        {
            _taskService = taskService;
            _systemEventService = systemEventService;
            
            InitializeAsync();
            SetupSystemEvents();
        }

        public string TaskText
        {
            get => _taskText;
            set => SetProperty(ref _taskText, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public bool IsWeekend
        {
            get => _isWeekend;
            set => SetProperty(ref _isWeekend, value);
        }

        public string DateText
        {
            get => _dateText;
            set => SetProperty(ref _dateText, value);
        }

        public bool IsStatusSuccess
        {
            get => _isStatusSuccess;
            set => SetProperty(ref _isStatusSuccess, value);
        }

        public ICommand SaveTaskCommand => new AsyncRelayCommand(SaveTaskAsync);
        public ICommand ViewHistoryCommand => new RelayCommand(() => ViewHistory?.Invoke());
        public ICommand OpenSettingsCommand => new RelayCommand(() => OpenSettings?.Invoke());

        public event Action? ViewHistory;
        public event Action? OpenSettings;
        public event PropertyChangedEventHandler? PropertyChanged;

        private async Task InitializeAsync()
        {
            DateText = DateTimeHelper.GetFormattedDate();
            IsWeekend = DateTimeHelper.IsWeekend();
        }

        private void SetupSystemEvents()
        {
            _systemEventService.SystemShutdown += OnSystemShutdown;
            _systemEventService.SystemSuspend += OnSystemSuspend;
            _systemEventService.StartMonitoring();
        }

        private async Task SaveTaskAsync()
        {
            if (IsWeekend)
            {
                ShowStatus("Weekend mode - no logging allowed! 😴", false);
                return;
            }

            var task = TaskText?.Trim();
            if (string.IsNullOrEmpty(task))
            {
                ShowStatus("Please enter a task before saving! ⚠️", false);
                return;
            }

            try
            {
                await _taskService.SaveTaskAsync(task);
                TaskText = "";
                ShowStatus("Task saved successfully! ✅", true);
            }
            catch (Exception ex)
            {
                ShowStatus($"Error saving task: {ex.Message} ❌", false);
            }
        }

        private async void OnSystemShutdown(object? sender, SystemShutdownEventArgs e)
        {
            if (IsWeekend) return;

            var task = TaskText?.Trim();
            if (!string.IsNullOrEmpty(task))
            {
                try
                {
                    await _taskService.SaveTaskAsync(task, e.Reason, "System shutdown detected");
                }
                catch
                {
                    // Ignore errors during shutdown
                }
            }
        }

        private async void OnSystemSuspend(object? sender, SystemSuspendEventArgs e)
        {
            if (IsWeekend) return;

            var task = TaskText?.Trim();
            if (!string.IsNullOrEmpty(task))
            {
                try
                {
                    await _taskService.SaveTaskAsync(task, e.Reason, "System suspend detected");
                }
                catch
                {
                    // Ignore errors during suspend
                }
            }
        }

        private void ShowStatus(string message, bool isSuccess)
        {
            StatusText = message;
            IsStatusSuccess = isSuccess;
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

        public void Dispose()
        {
            _systemEventService.StopMonitoring();
            _systemEventService.SystemShutdown -= OnSystemShutdown;
            _systemEventService.SystemSuspend -= OnSystemSuspend;
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
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
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object? parameter)
        {
            _execute();
        }
    }

    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
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
            return !_isExecuting && (_canExecute?.Invoke() ?? true);
        }

        public async void Execute(object? parameter)
        {
            if (_isExecuting) return;
            
            _isExecuting = true;
            CommandManager.InvalidateRequerySuggested();
            
            try
            {
                await _execute();
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}
