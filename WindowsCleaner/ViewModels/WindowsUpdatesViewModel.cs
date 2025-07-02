using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WindowsCleaner.Models;
using WindowsCleaner.Services;

namespace WindowsCleaner.ViewModels
{
    /// <summary>
    /// ViewModel for the Windows Updates view
    /// </summary>
    public class WindowsUpdatesViewModel : BaseViewModel, IDisposable
    {
        private readonly WindowsUpdateService _updateService;
        private readonly LoggingService _loggingService;
        private readonly LocalizationService _localizationService;
        private CancellationTokenSource? _cancellationTokenSource;
        
        private bool _isChecking;
        private bool _isInstalling;
        private string _lastUpdateCheck = "Never";
        private string _windowsVersion = "";
        private string _checkProgress = "";
        private string _installProgress = "";
        private double _progressValue;
        private int _pendingUpdatesCount;
        private DateTime? _lastScanTime;
        private bool _restartRequired;
        private bool _autoUpdateEnabled;

        public WindowsUpdatesViewModel(
            WindowsUpdateService? updateService = null, 
            LoggingService? loggingService = null,
            LocalizationService? localizationService = null,
            SettingsService? settingsService = null)
        {
            _updateService = updateService ?? new WindowsUpdateService();
            _loggingService = loggingService ?? LoggingService.Instance;
            _localizationService = localizationService ?? LocalizationService.Instance;
            
            PendingUpdates = new ObservableCollection<PendingUpdate>();
            _cancellationTokenSource = new CancellationTokenSource();
            
            // Initialize commands
            CheckUpdatesCommand = new RelayCommand(
                async () => await CheckForUpdatesAsync(), 
                () => !IsChecking && !IsInstalling);
                
            InstallUpdatesCommand = new RelayCommand(
                async () => await InstallAllUpdatesAsync(), 
                () => HasPendingUpdates && !IsChecking && !IsInstalling);
                
            InstallUpdateCommand = new RelayCommand<PendingUpdate>(
                async (update) => await InstallUpdateAsync(update), 
                (update) => update != null && !IsChecking && !IsInstalling);
                
            CancelCommand = new RelayCommand(
                CancelOperation, 
                () => (IsChecking || IsInstalling) && _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested);
            
            // Load initial data
            LoadSystemInfoAsync();

            // Auto check for updates if enabled in settings
            var settings = settingsService ?? new SettingsService();
            if (settings.Settings is EnhancedAppSettings enhancedSettings && 
                enhancedSettings.AutoCheckWindowsUpdates)
            {
                // Use a proper async approach for delayed execution
                Task.Delay(2000).ContinueWith(async _ => 
                {
                    await CheckForUpdatesAsync();
                }, TaskScheduler.Current);
            }
        }

        public ObservableCollection<PendingUpdate> PendingUpdates { get; }

        public bool IsChecking
        {
            get => _isChecking;
            set
            {
                if (SetProperty(ref _isChecking, value))
                {
                    OnPropertyChanged(nameof(IsNotBusy));
                    (CheckUpdatesCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (InstallUpdatesCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsInstalling
        {
            get => _isInstalling;
            set
            {
                if (SetProperty(ref _isInstalling, value))
                {
                    OnPropertyChanged(nameof(IsNotBusy));
                    (CheckUpdatesCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (InstallUpdatesCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsNotBusy => !IsChecking && !IsInstalling;
        public bool IsBusy => IsChecking || IsInstalling;

        public string LastUpdateCheck
        {
            get => _lastUpdateCheck;
            set => SetProperty(ref _lastUpdateCheck, value);
        }

        public string WindowsVersion
        {
            get => _windowsVersion;
            set => SetProperty(ref _windowsVersion, value);
        }

        public string CheckProgress
        {
            get => _checkProgress;
            set => SetProperty(ref _checkProgress, value);
        }

        public string InstallProgress
        {
            get => _installProgress;
            set => SetProperty(ref _installProgress, value);
        }

        public double ProgressValue
        {
            get => _progressValue;
            set => SetProperty(ref _progressValue, value);
        }

        public int PendingUpdatesCount
        {
            get => _pendingUpdatesCount;
            set
            {
                if (SetProperty(ref _pendingUpdatesCount, value))
                {
                    OnPropertyChanged(nameof(HasPendingUpdates));
                    OnPropertyChanged(nameof(IsEmpty));
                    (InstallUpdatesCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public DateTime? LastScanTime
        {
            get => _lastScanTime;
            set => SetProperty(ref _lastScanTime, value);
        }
        
        public bool RestartRequired
        {
            get => _restartRequired;
            set => SetProperty(ref _restartRequired, value);
        }
        
        public bool AutoUpdateEnabled
        {
            get => _autoUpdateEnabled;
            set => SetProperty(ref _autoUpdateEnabled, value);
        }

        public bool HasPendingUpdates => PendingUpdatesCount > 0;
        public bool IsEmpty => PendingUpdatesCount == 0 && !IsChecking;

        public ICommand CheckUpdatesCommand { get; }
        public ICommand InstallUpdatesCommand { get; }
        public ICommand InstallUpdateCommand { get; }
        public ICommand CancelCommand { get; }
        
        private void CancelOperation()
        {
            _cancellationTokenSource?.Cancel();
            _loggingService.LogInfo(_localizationService.GetString("WindowsUpdateCancelledByUser") ?? "Windows update operation cancelled by user.");
        }

        private async void LoadSystemInfoAsync()
        {
            try
            {
                var status = await _updateService.GetUpdateStatusAsync();
                
                LastUpdateCheck = status.LastUpdateCheck?.ToString("yyyy-MM-dd HH:mm") ?? 
                    (_localizationService.GetString("Never") ?? "Never");
                WindowsVersion = $"{Environment.OSVersion.Version} (Build {Environment.OSVersion.Version.Build})";
                LastScanTime = status.LastUpdateCheck;
                
                // Update the auto update enabled status from system
                AutoUpdateEnabled = await _updateService.IsAutoUpdateEnabledAsync();
                
                PendingUpdates.Clear();
                foreach (var update in status.PendingUpdates)
                {
                    PendingUpdates.Add(update);
                }
                
                PendingUpdatesCount = PendingUpdates.Count;
                
                // Check if restart is required from previous updates
                RestartRequired = await _updateService.IsRestartRequiredAsync();
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to load system information", ex);
                ShowErrorMessage(_localizationService.GetString("FailedToLoadSystemInfo") ?? 
                    "Failed to load system information.");
            }
        }

        private async Task CheckForUpdatesAsync()
        {
            // Create new cancellation token source
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            
            try
            {
                IsChecking = true;
                CheckProgress = _localizationService.GetString("ConnectingToWindowsUpdateServers") ?? "Connecting to Windows Update servers...";
                ProgressValue = 20;

                await Task.Delay(1000, _cancellationTokenSource.Token); // Simulate connection time
                if (_cancellationTokenSource.Token.IsCancellationRequested) return;
                
                CheckProgress = _localizationService.GetString("CheckingForAvailableUpdates") ?? "Checking for available updates...";
                ProgressValue = 50;

                var status = await _updateService.GetUpdateStatusAsync(_cancellationTokenSource.Token);
                if (_cancellationTokenSource.Token.IsCancellationRequested) return;
                
                CheckProgress = _localizationService.GetString("ProcessingUpdateInformation") ?? "Processing update information...";
                ProgressValue = 80;
                
                PendingUpdates.Clear();
                foreach (var update in status.PendingUpdates)
                {
                    PendingUpdates.Add(update);
                }
                
                PendingUpdatesCount = PendingUpdates.Count;
                LastUpdateCheck = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                LastScanTime = DateTime.Now;
                
                CheckProgress = _localizationService.GetString("UpdateCheckCompleted") ?? "Update check completed";
                ProgressValue = 100;
                
                _loggingService.LogInfo($"Windows Update check completed. {PendingUpdatesCount} updates found.");
                
                await Task.Delay(1000, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                _loggingService.LogInfo(_localizationService.GetString("WindowsUpdateCheckCancelled") ?? "Windows update check was cancelled.");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to check for Windows updates", ex);
                CheckProgress = _localizationService.GetString("FailedToCheckForUpdates") ?? "Failed to check for updates. Please try again.";
            }
            finally
            {
                IsChecking = false;
                ProgressValue = 0;
                CheckProgress = "";
            }
        }

        private async Task InstallAllUpdatesAsync()
        {
            // Create new cancellation token source
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            
            try
            {
                IsInstalling = true;
                var updatesToInstall = PendingUpdates.ToList();
                
                for (int i = 0; i < updatesToInstall.Count; i++)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;
                        
                    var update = updatesToInstall[i];
                    string progressMessage = _localizationService.GetString("InstallingUpdateProgress") ?? "Installing {0}... ({1}/{2})";
                    InstallProgress = string.Format(progressMessage, update.Title, i + 1, updatesToInstall.Count);
                    ProgressValue = (double)(i + 1) / updatesToInstall.Count * 100;

                    // Simulate installation time based on update size
                    var installTime = Math.Max(2000, update.SizeInBytes / 1000000); // Minimum 2 seconds
                    await Task.Delay((int)installTime, _cancellationTokenSource.Token);
                    
                    if (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        PendingUpdates.Remove(update);
                    }
                }

                if (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    PendingUpdatesCount = 0;
                    InstallProgress = _localizationService.GetString("AllUpdatesInstalledSuccessfully") ?? "All updates installed successfully!";
                    
                    // Check if restart is required
                    RestartRequired = await _updateService.IsRestartRequiredAsync();
                    
                    _loggingService.LogInfo($"Successfully installed {updatesToInstall.Count} Windows updates.");
                    
                    await Task.Delay(2000, _cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                _loggingService.LogInfo(_localizationService.GetString("WindowsUpdateInstallCancelled") ?? "Windows update installation was cancelled.");
                InstallProgress = _localizationService.GetString("InstallationCancelled") ?? "Installation cancelled.";
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to install Windows updates", ex);
                InstallProgress = _localizationService.GetString("SomeUpdatesFailed") ?? "Some updates failed to install. Please check Windows Update settings.";
            }
            finally
            {
                IsInstalling = false;
                ProgressValue = 0;
                
                // Don't clear progress message immediately if there was an error
                if (string.IsNullOrEmpty(InstallProgress) || InstallProgress.Contains("successfully"))
                {
                    InstallProgress = "";
                }
                
                // Refresh pending updates count
                PendingUpdatesCount = PendingUpdates.Count;
            }
        }

        private async Task InstallUpdateAsync(PendingUpdate update)
        {
            if (update == null)
                return;
                
            // Create new cancellation token source
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            
            try
            {
                IsInstalling = true;
                string installingMessage = _localizationService.GetString("InstallingUpdate") ?? "Installing {0}...";
                InstallProgress = string.Format(installingMessage, update.Title);
                ProgressValue = 50;

                // Simulate installation time based on update size
                var installTime = Math.Max(2000, update.SizeInBytes / 1000000); // Minimum 2 seconds
                await Task.Delay((int)installTime, _cancellationTokenSource.Token);
                
                if (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    PendingUpdates.Remove(update);
                    PendingUpdatesCount--;
                    
                    string successMessage = _localizationService.GetString("SuccessfullyInstalledUpdate") ?? "Successfully installed {0}";
                    InstallProgress = string.Format(successMessage, update.Title);
                    
                    // Check if restart is required
                    RestartRequired = await _updateService.IsRestartRequiredAsync();
                    
                    _loggingService.LogInfo($"Successfully installed Windows update: {update.Title}");
                    
                    await Task.Delay(1000, _cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                _loggingService.LogInfo(_localizationService.GetString("WindowsUpdateInstallCancelled") ?? "Windows update installation was cancelled.");
                string cancelledMessage = _localizationService.GetString("UpdateInstallationCancelled") ?? "Installation cancelled.";
                InstallProgress = cancelledMessage;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Failed to install Windows update: {update.Title}", ex);
                string failureMessage = _localizationService.GetString("FailedToInstallUpdate") ?? "Failed to install {0}. Please try again.";
                InstallProgress = string.Format(failureMessage, update.Title);
            }
            finally
            {
                IsInstalling = false;
                ProgressValue = 0;
                
                // Don't clear progress message immediately if there was an error
                if (string.IsNullOrEmpty(InstallProgress) || InstallProgress.Contains("Successfully"))
                {
                    InstallProgress = "";
                }
            }
        }

        public void Dispose()
        {
            // Cancel any pending operations and dispose resources
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
        
        private void ShowErrorMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;
                
            // Use the UI thread to show a message box
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    message,
                    _localizationService.GetString("Error") ?? "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            });
        }
    }
}
