using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace WindowsCleaner.Models
{
    /// <summary>
    /// Driver information model
    /// </summary>
    public class DriverInfo : INotifyPropertyChanged
    {
        private string _currentVersion = "";
        private string _latestVersion = "";
        private bool _hasUpdate;
        
        public string DeviceName { get; set; } = "";
        public string DriverVersion { get; set; } = "";
        public DateTime? DriverDate { get; set; }
        public string InfName { get; set; } = "";
        public string HardwareID { get; set; } = "";
        public string CompatID { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public bool IsSigned { get; set; }
        public string Location { get; set; } = "";
        public string DeviceClass { get; set; } = "";
        public bool IsActive { get; set; }
        
        public string CurrentVersion
        {
            get => _currentVersion;
            set
            {
                if (_currentVersion != value)
                {
                    _currentVersion = value;
                    OnPropertyChanged(nameof(CurrentVersion));
                }
            }
        }
        
        public string LatestVersion
        {
            get => _latestVersion;
            set
            {
                if (_latestVersion != value)
                {
                    _latestVersion = value;
                    OnPropertyChanged(nameof(LatestVersion));
                }
            }
        }
        
        public bool HasUpdate
        {
            get => _hasUpdate;
            set
            {
                if (_hasUpdate != value)
                {
                    _hasUpdate = value;
                    OnPropertyChanged(nameof(HasUpdate));
                }
            }
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Driver update information
    /// </summary>
    public class DriverUpdateInfo
    {
        public DriverInfo DriverInfo { get; set; } = new();
        public bool HasUpdate { get; set; }
        public bool UpdateAvailable { get; set; }
        public string NewVersion { get; set; } = "";
        public string UpdateSource { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public string ReleaseNotes { get; set; } = "";
        public long UpdateSizeBytes { get; set; }
        public string ErrorMessage { get; set; } = "";
        public DateTime? ReleaseDate { get; set; }
        public bool IsSecurityUpdate { get; set; }
        public bool IsCriticalUpdate { get; set; }
    }

    /// <summary>
    /// Windows Update status model
    /// </summary>
    public class WindowsUpdateStatus
    {
        public DateTime? LastUpdateCheck { get; set; }
        public DateTime? LastInstallDate { get; set; }
        public bool PendingRestartRequired { get; set; }
        public string WindowsVersion { get; set; } = "";
        public string BuildNumber { get; set; } = "";
        public bool UpdatesAvailable { get; set; }
        public bool AutoUpdateEnabled { get; set; }
        public List<PendingUpdate> PendingUpdates { get; set; } = new();
        public string ErrorMessage { get; set; } = "";
        public UpdateServiceStatus ServiceStatus { get; set; }
        public int TotalPendingUpdates => PendingUpdates.Count;
        public long TotalUpdateSizeBytes => PendingUpdates.Sum(u => u.SizeInBytes);
    }

    /// <summary>
    /// Pending Windows update
    /// </summary>
    public class PendingUpdate
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsImportant { get; set; }
        public bool IsOptional { get; set; }
        public bool IsSecurityUpdate { get; set; }
        public long SizeInBytes { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string KBArticleID { get; set; } = "";
        public List<string> Categories { get; set; } = new();
    }

    /// <summary>
    /// Startup program information
    /// </summary>
    public class StartupProgramInfo : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isEnabled;
        private StartupImpact _impact;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Indicates if the program is selected in UI
        /// </summary>
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

        /// <summary>
        /// Program name displayed
        /// </summary>
        public string Name { get; set; } = "";
        public string Command { get; set; } = "";
        public string FilePath { get; set; } = "";
        public StartupLocation Location { get; set; }

        /// <summary>
        /// Indicates if the program is enabled at startup
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged(nameof(IsEnabled));
                }
            }
        }

        /// <summary>
        /// Startup impact level
        /// </summary>
        public StartupImpact Impact
        {
            get => _impact;
            set
            {
                if (_impact != value)
                {
                    _impact = value;
                    OnPropertyChanged(nameof(Impact));
                    OnPropertyChanged(nameof(ImpactLevel));
                }
            }
        }

        /// <summary>
        /// Alias for Impact property used in ViewModel filters
        /// </summary>
        public StartupImpact ImpactLevel
        {
            get => Impact;
            set => Impact = value;
        }

        public string Publisher { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime? LastModified { get; set; }
        public long FileSize { get; set; }

        /// <summary>
        /// Indicates if the program is recommended for disabling
        /// </summary>
        public bool IsRecommendedToDisable => Impact == StartupImpact.High && !IsSystemCritical();

        private bool IsSystemCritical()
        {
            var criticalPrograms = new[] { "windows", "microsoft", "defender", "antivirus", "security" };
            var nameOrPublisher = $"{Name} {Publisher}".ToLower();
            return criticalPrograms.Any(p => nameOrPublisher.Contains(p));
        }
    }

    // Removed duplicate CrashInfo class definition to resolve CS0101

    /// <summary>
    /// Animation settings for UI
    /// </summary>
    public class AnimationSettings
    {
        public bool EnableAnimations { get; set; } = true;
        public AnimationSpeed Speed { get; set; } = AnimationSpeed.Normal;
        public bool EnableTransitions { get; set; } = true;
        public bool EnableParallax { get; set; } = true;
        public bool EnableBlurEffects { get; set; } = true;
        public bool ReduceMotion { get; set; } = false;
    }

    /// <summary>
    /// Enhanced application settings
    /// </summary>
    public class EnhancedAppSettings : AppSettings
    {
        public AnimationSettings Animations { get; set; } = new();
        public new string Language { get; set; } = "en-US";
        public bool EnableCrashReporting { get; set; } = true;
        public bool AutoCheckDriverUpdates { get; set; } = false;
        public bool AutoCheckWindowsUpdates { get; set; } = true;
        public bool EnableStartupOptimization { get; set; } = true;
        public int MaxCrashReports { get; set; } = 50;
        public bool ShowPerformanceMetrics { get; set; } = false;
        public bool EnableAdvancedLogging { get; set; } = false;
        public int CleanupThreads { get; set; } = Environment.ProcessorCount;
        public bool EnablePreloadingAnalysis { get; set; } = true;
    }

    /// <summary>
    /// Enums for the new features
    /// </summary>
    public enum StartupLocation
    {
        CurrentUser,
        AllUsers,
        CurrentUserOnce,
        AllUsersOnce,
        StartupFolder,
        TaskScheduler
    }

    public enum StartupImpact
    {
        Low,
        Medium,
        High
    }

    public enum UpdateServiceStatus
    {
        Unknown,
        Running,
        Stopped,
        Disabled,
        Error
    }

    public enum AnimationSpeed
    {
        Slow,
        Normal,
        Fast,
        VeryFast
    }

    /// <summary>
    /// System performance metrics
    /// </summary>
    public class SystemPerformanceMetrics
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
        public long AvailableMemoryMB { get; set; }
        public long TotalMemoryMB { get; set; }
        public TimeSpan Uptime { get; set; }
        public int ProcessCount { get; set; }
        public int ThreadCount { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Enhanced cleanup result with more details
    /// </summary>
    public class EnhancedCleanupResult : CleanupResult
    {
        public TimeSpan Duration { get; set; }
        public new List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public Dictionary<string, long> CategorySizes { get; set; } = new();
        public SystemPerformanceMetrics BeforeMetrics { get; set; } = new();
        public SystemPerformanceMetrics AfterMetrics { get; set; } = new();
        public bool RequiresRestart { get; set; }
        public double PerformanceImprovement => CalculatePerformanceImprovement();

        private double CalculatePerformanceImprovement()
        {
            if (BeforeMetrics == null || AfterMetrics == null)
                return 0.0;

            // Simple metric based on memory usage improvement
            var memoryImprovement = BeforeMetrics.MemoryUsage - AfterMetrics.MemoryUsage;
            return Math.Max(0, memoryImprovement);
        }
    }
}
