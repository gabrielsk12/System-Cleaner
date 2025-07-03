using System.ComponentModel;

namespace WindowsCleaner.Models
{
    public enum CleanupType
    {
        WindowsUpdate,
        TemporaryFiles,
        RecycleBin,
        SystemCache,
        BrowserCache,
        LogFiles,
        ErrorReports,
        ThumbnailCache
    }

    public class CleanupCategory : INotifyPropertyChanged
    {
        private bool _isSelected;
        private int _filesFound;
        private long _sizeInBytes;

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public CleanupType Category { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public int FilesFound
        {
            get => _filesFound;
            set
            {
                if (_filesFound != value)
                {
                    _filesFound = value;
                    OnPropertyChanged(nameof(FilesFound));
                    OnPropertyChanged(nameof(DisplayText));
                }
            }
        }

        public long SizeInBytes
        {
            get => _sizeInBytes;
            set
            {
                if (_sizeInBytes != value)
                {
                    _sizeInBytes = value;
                    OnPropertyChanged(nameof(SizeInBytes));
                    OnPropertyChanged(nameof(DisplayText));
                }
            }
        }

        public string DisplayText
        {
            get
            {
                if (FilesFound == 0)
                    return Description;
                
                return $"{Description} ({FilesFound:N0} files, {FormatBytes(SizeInBytes)})";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F1} MB";
            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F1} GB";
        }
    }

    public class ScanProgress
    {
        public double ProgressPercentage { get; set; }
        public string CurrentOperation { get; set; } = string.Empty;
        public CleanupType Category { get; set; }
        public int FilesFound { get; set; }
        public long SizeInBytes { get; set; }
    }

    public class CleanProgress
    {
        public double ProgressPercentage { get; set; }
        public string CurrentOperation { get; set; } = string.Empty;
        public int FilesProcessed { get; set; }
        public long BytesProcessed { get; set; }
    }

    public class CleanupResult
    {
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Crash information for error handling and reporting
    /// </summary>
    public class CrashInfo
    {
        public string CrashId { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string ExceptionType { get; set; } = "";
        public string Message { get; set; } = "";
        public string StackTrace { get; set; } = "";
        public string AdditionalInfo { get; set; } = "";
        public string ApplicationVersion { get; set; } = "";
        public string OperatingSystem { get; set; } = "";
        public bool IsReported { get; set; }
    }
}
