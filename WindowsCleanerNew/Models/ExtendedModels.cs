using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WindowsCleaner.Models
{
    public class FileSystemItem : INotifyPropertyChanged
    {
        private bool _isExpanded;
        private bool _isSelected;
        private long _size;
        private double _sizePercentage;

        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public string DisplaySize { get; set; } = string.Empty;
        public FileSystemItemType ItemType { get; set; }
        public DateTime LastModified { get; set; }
        public string Extension { get; set; } = string.Empty;
        public List<FileSystemItem> Children { get; set; } = new();
        public FileSystemItem? Parent { get; set; }

        public long Size
        {
            get => _size;
            set
            {
                SetProperty(ref _size, value);
                DisplaySize = FormatBytes(value);
                OnPropertyChanged(nameof(DisplaySize));
            }
        }

        public double SizePercentage
        {
            get => _sizePercentage;
            set => SetProperty(ref _sizePercentage, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public bool HasChildren => Children.Any();

        public string Icon
        {
            get
            {
                return ItemType switch
                {
                    FileSystemItemType.Drive => "🖥️",
                    FileSystemItemType.Folder => IsExpanded ? "📂" : "📁",
                    FileSystemItemType.File => GetFileIcon(Extension),
                    _ => "📄"
                };
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private static string GetFileIcon(string extension)
        {
            return extension.ToLower() switch
            {
                ".exe" => "⚙️",
                ".dll" => "🔧",
                ".txt" => "📝",
                ".log" => "📋",
                ".tmp" => "🗂️",
                ".cache" => "💾",
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "🖼️",
                ".mp4" or ".avi" or ".mkv" or ".mov" => "🎥",
                ".mp3" or ".wav" or ".flac" => "🎵",
                ".pdf" => "📕",
                ".doc" or ".docx" => "📘",
                ".xls" or ".xlsx" => "📗",
                ".zip" or ".rar" or ".7z" => "📦",
                _ => "📄"
            };
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F1} MB";
            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F1} GB";
        }
    }

    public enum FileSystemItemType
    {
        Drive,
        Folder,
        File
    }

    public class ScheduleSettings : INotifyPropertyChanged
    {
        private bool _isEnabled;
        private ScheduleFrequency _frequency = ScheduleFrequency.Weekly;
        private TimeSpan _time = new(2, 0, 0); // 2 AM
        private DayOfWeek _dayOfWeek = DayOfWeek.Sunday;
        private int _dayOfMonth = 1;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public ScheduleFrequency Frequency
        {
            get => _frequency;
            set => SetProperty(ref _frequency, value);
        }

        public TimeSpan Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }

        public DayOfWeek DayOfWeek
        {
            get => _dayOfWeek;
            set => SetProperty(ref _dayOfWeek, value);
        }

        public int DayOfMonth
        {
            get => _dayOfMonth;
            set => SetProperty(ref _dayOfMonth, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public enum ScheduleFrequency
    {
        Daily,
        Weekly,
        Monthly
    }

    public enum AppTheme
    {
        Light,
        Dark,
        System
    }

    public class AppSettings : INotifyPropertyChanged
    {
        private AppTheme _theme = AppTheme.System;
        private bool _startWithWindows;
        private bool _showNotifications = true;
        private bool _confirmBeforeDelete = true;
        private int _maxLogFiles = 100;
        private string _language = "en-US";

        public AppTheme Theme
        {
            get => _theme;
            set => SetProperty(ref _theme, value);
        }

        public bool StartWithWindows
        {
            get => _startWithWindows;
            set => SetProperty(ref _startWithWindows, value);
        }

        public bool ShowNotifications
        {
            get => _showNotifications;
            set => SetProperty(ref _showNotifications, value);
        }

        public bool ConfirmBeforeDelete
        {
            get => _confirmBeforeDelete;
            set => SetProperty(ref _confirmBeforeDelete, value);
        }

        public int MaxLogFiles
        {
            get => _maxLogFiles;
            set => SetProperty(ref _maxLogFiles, value);
        }

        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }

        public ScheduleSettings ScheduleSettings { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class FileSystemItemInfo : INotifyPropertyChanged
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public string Extension { get; set; } = string.Empty;
        public string DisplaySize => FormatBytes(Size);

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private static string FormatBytes(long bytes)
        {
            const int scale = 1024;
            string[] orders = { "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (bytes > max)
                    return $"{decimal.Divide(bytes, max):##.##} {order}";
                max /= scale;
            }
            return "0 Bytes";
        }
    }
}
