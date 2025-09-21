using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TaskLogger.Models;

namespace TaskLogger.ViewModels
{
    public class TaskEntryViewModel : INotifyPropertyChanged
    {
        private readonly TaskEntry _taskEntry;
        private bool _isEditing;
        private string _originalTask;

        public TaskEntryViewModel(TaskEntry taskEntry)
        {
            _taskEntry = taskEntry;
            _originalTask = taskEntry.Task;
        }

        public TaskEntry TaskEntry => _taskEntry;

        public int Id => _taskEntry.Id;
        
        public string Task
        {
            get => _taskEntry.Task;
            set
            {
                if (_taskEntry.Task != value)
                {
                    _taskEntry.Task = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime CreatedAt => _taskEntry.CreatedAt;
        public string? EventType => _taskEntry.EventType;
        public string? Notes => _taskEntry.Notes;
        public string Timestamp => _taskEntry.Timestamp;

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    if (_isEditing)
                    {
                        _originalTask = Task;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public void StartEdit()
        {
            IsEditing = true;
        }

        public void CancelEdit()
        {
            Task = _originalTask;
            IsEditing = false;
        }

        public void SaveEdit()
        {
            IsEditing = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}