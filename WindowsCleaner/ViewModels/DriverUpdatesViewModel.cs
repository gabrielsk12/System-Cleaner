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
    /// ViewModel for the Driver Updates view
    /// </summary>
    public partial class DriverUpdatesViewModel : BaseViewModel, IDisposable
    {
        private readonly DriverService _driverService;
        private readonly LoggingService _loggingService;
        private readonly LocalizationService _localizationService;
        private CancellationTokenSource? _cancellationTokenSource;
        
        private bool _isScanning;
        private bool _isUpdating;
        private string _statusText;
        private string _scanProgress = "";
        private string _updateProgress = "";
        private double _progressValue;
        private int _totalDrivers;
        private int _updatesAvailable;
        private bool _hasLoadedDrivers;
        private DateTime? _lastScanTime;

        public DriverUpdatesViewModel()
        {
            _driverService = new DriverService();
            _loggingService = LoggingService.Instance;
            _localizationService = LocalizationService.Instance;
            
            _statusText = _localizationService.GetString("ReadyToScanDrivers", "Ready to scan for drivers");
            
            Drivers = new ObservableCollection<DriverInfo>();
            
            ScanDriversCommand = new RelayCommand(async () => await ScanDriversAsync(), () => !IsScanning && !IsUpdating);
            UpdateAllDriversCommand = new RelayCommand(async () => await UpdateAllDriversAsync(), () => HasUpdatesAvailable && !IsScanning && !IsUpdating);
            UpdateDriverCommand = new RelayCommand<DriverInfo>(async (driver) => await UpdateDriverAsync(driver!), (driver) => driver?.HasUpdate == true && !IsScanning && !IsUpdating);
            CancelCommand = new RelayCommand(CancelOperation, () => IsScanning || IsUpdating);
            
            LoadDriversAsync();

            // Auto check for updates if enabled in settings
            var settingsService = new SettingsService();
            if (settingsService.Settings is EnhancedAppSettings enhancedSettings && 
                enhancedSettings.AutoCheckDriverUpdates)
            {
                Task.Delay(2000).ContinueWith(_ => ScanDriversAsync());
            }
        }

        public ObservableCollection<DriverInfo> Drivers { get; }

        public bool IsScanning
        {
            get => _isScanning;
            set
            {
                if (SetProperty(ref _isScanning, value))
                {
                    OnPropertyChanged(nameof(IsNotBusy));
                    OnPropertyChanged(nameof(IsBusy));
                    (ScanDriversCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (UpdateAllDriversCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (CancelCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsUpdating
        {
            get => _isUpdating;
            set
            {
                if (SetProperty(ref _isUpdating, value))
                {
                    OnPropertyChanged(nameof(IsNotBusy));
                    OnPropertyChanged(nameof(IsBusy));
                    (ScanDriversCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (UpdateAllDriversCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (CancelCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsNotBusy => !IsScanning && !IsUpdating;
        public bool IsBusy => IsScanning || IsUpdating;

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public string ScanProgress
        {
            get => _scanProgress;
            set => SetProperty(ref _scanProgress, value);
        }

        public string UpdateProgress
        {
            get => _updateProgress;
            set => SetProperty(ref _updateProgress, value);
        }

        public double ProgressValue
        {
            get => _progressValue;
            set => SetProperty(ref _progressValue, value);
        }

        public int TotalDrivers
        {
            get => _totalDrivers;
            set => SetProperty(ref _totalDrivers, value);
        }

        public int UpdatesAvailable
        {
            get => _updatesAvailable;
            set
            {
                if (SetProperty(ref _updatesAvailable, value))
                {
                    OnPropertyChanged(nameof(HasUpdatesAvailable));
                    (UpdateAllDriversCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public DateTime? LastScanTime
        {
            get => _lastScanTime;
            set => SetProperty(ref _lastScanTime, value);
        }

        public bool HasUpdatesAvailable => UpdatesAvailable > 0;

        public ICommand ScanDriversCommand { get; }
        public ICommand UpdateAllDriversCommand { get; }
        public ICommand UpdateDriverCommand { get; }
        public ICommand CancelCommand { get; }

        private void CancelOperation()
        {
            _cancellationTokenSource?.Cancel();
            _loggingService.LogInfo("Driver scan or update operation cancelled by user.");
        }

        private async void LoadDriversAsync()
        {
            if (_hasLoadedDrivers)
                return;
                
            try
            {
                IsScanning = true;
                StatusText = _localizationService.GetString("LoadingInstalledDrivers", "Loading installed drivers...");
                ProgressValue = 0;

                _cancellationTokenSource = new CancellationTokenSource();
                var drivers = await _driverService.GetSystemDriversAsync();
                
                Drivers.Clear();
                foreach (var driver in drivers)
                {
                    // Add CurrentVersion property for UI binding
                    driver.CurrentVersion = driver.DriverVersion;
                    Drivers.Add(driver);
                }

                TotalDrivers = Drivers.Count;
                UpdatesAvailable = 0; // We haven't checked for updates yet
                StatusText = _localizationService.GetFormattedString("FoundDriversNeedsScan", TotalDrivers);
                ProgressValue = 100;
                _hasLoadedDrivers = true;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to load drivers", ex);
                StatusText = _localizationService.GetString("FailedToLoadDrivers", "Failed to load drivers. Please try again.");
            }
            finally
            {
                IsScanning = false;
                ProgressValue = 0;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private async Task ScanDriversAsync()
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            
            try
            {
                IsScanning = true;
                StatusText = _localizationService.GetString("ScanningForDriverUpdates", "Scanning for driver updates...");
                ProgressValue = 0;

                // Use progress reporting for better UI updates
                var progress = new Progress<DriverCheckProgress>(UpdateScanProgress);
                var updateInfos = await _driverService.CheckAllDriverUpdatesAsync(progress, _cancellationTokenSource.Token);
                
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    StatusText = _localizationService.GetString("DriverScanCancelled", "Driver scan was cancelled.");
                    return;
                }
                
                // Update our driver collection
                Drivers.Clear();
                foreach (var updateInfo in updateInfos)
                {
                    var driver = updateInfo.DriverInfo;
                    driver.HasUpdate = updateInfo.HasUpdate;
                    driver.CurrentVersion = driver.DriverVersion;
                    
                    if (updateInfo.HasUpdate)
                    {
                        driver.LatestVersion = updateInfo.NewVersion;
                    }
                    else
                    {
                        driver.LatestVersion = driver.DriverVersion;
                    }
                    
                    Drivers.Add(driver);
                }

                TotalDrivers = Drivers.Count;
                UpdatesAvailable = updateInfos.Count(info => info.HasUpdate);
                LastScanTime = DateTime.Now;

                string statusMsg;
                if (UpdatesAvailable > 0)
                {
                    statusMsg = _localizationService.GetFormattedString("ScanCompleteWithUpdates", 
                        UpdatesAvailable);
                }
                else
                {
                    statusMsg = _localizationService.GetString("ScanCompleteNoUpdates", 
                        "Scan complete! All drivers are up to date.");
                }
                
                StatusText = statusMsg;
                _loggingService.LogInfo($"Driver scan completed. {UpdatesAvailable} updates found out of {TotalDrivers} drivers.");
            }
            catch (OperationCanceledException)
            {
                StatusText = _localizationService.GetString("DriverScanCancelled", "Driver scan was cancelled.");
                _loggingService.LogInfo("Driver scan was cancelled by user.");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to scan for driver updates", ex);
                StatusText = _localizationService.GetString("FailedToScan", "Failed to scan for updates. Please try again.");
                
                // Show error to user in the UI
                MessageBox.Show(
                    _localizationService.GetFormattedString("DriverScanErrorMessage", ex.Message),
                    _localizationService.GetString("Error", "Error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsScanning = false;
                ProgressValue = 0;
                ScanProgress = "";
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private void UpdateScanProgress(DriverCheckProgress progress)
        {
            ScanProgress = _localizationService.GetFormattedString("CheckingDriver", 
                progress.CurrentDriver, progress.CompletedCount + 1, progress.TotalCount);
            
            ProgressValue = progress.Progress;
        }

        private async Task UpdateAllDriversAsync()
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            
            try
            {
                IsUpdating = true;
                var driversToUpdate = Drivers.Where(d => d.HasUpdate).ToList();
                
                if (driversToUpdate.Count == 0)
                {
                    StatusText = _localizationService.GetString("NoDriverUpdatesAvailable", "No driver updates available.");
                    return;
                }
                
                StatusText = _localizationService.GetString("UpdatingDrivers", "Updating drivers...");
                
                int successCount = 0;
                int failureCount = 0;
                
                for (int i = 0; i < driversToUpdate.Count; i++)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        StatusText = _localizationService.GetString("DriverUpdateCancelled", "Driver update was cancelled.");
                        break;
                    }
                    
                    var driver = driversToUpdate[i];
                    UpdateProgress = _localizationService.GetFormattedString("UpdatingDriver", 
                        driver.DeviceName, i + 1, driversToUpdate.Count);
                    ProgressValue = (double)(i + 1) / driversToUpdate.Count * 100;

                    try 
                    {
                        // Create update info object
                        var updateInfo = new DriverUpdateInfo
                        {
                            DriverInfo = driver,
                            HasUpdate = true,
                            UpdateAvailable = true,
                            NewVersion = driver.LatestVersion
                        };
                        
                        var success = await _driverService.UpdateDriverAsync(updateInfo);
                        if (success)
                        {
                            driver.CurrentVersion = driver.LatestVersion;
                            driver.HasUpdate = false;
                            successCount++;
                        }
                        else
                        {
                            failureCount++;
                            _loggingService.LogWarning($"Failed to update driver: {driver.DeviceName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        _loggingService.LogError($"Error updating driver: {driver.DeviceName}", ex);
                    }

                    // Small delay for UI responsiveness
                    await Task.Delay(500, _cancellationTokenSource.Token);
                }

                UpdatesAvailable = Drivers.Count(d => d.HasUpdate);
                
                if (failureCount == 0)
                {
                    StatusText = _localizationService.GetFormattedString("AllUpdatesCompleted", 
                        successCount);
                }
                else
                {
                    StatusText = _localizationService.GetFormattedString("SomeUpdatesFailed", 
                        successCount, failureCount);
                }
                
                _loggingService.LogInfo($"Driver update batch completed. {successCount} successful, {failureCount} failed.");
            }
            catch (OperationCanceledException)
            {
                StatusText = _localizationService.GetString("DriverUpdateCancelled", "Driver update was cancelled.");
                _loggingService.LogInfo("Driver update was cancelled by user.");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to update drivers", ex);
                StatusText = _localizationService.GetString("FailedToUpdateDrivers", "Failed to update drivers. Please try again.");
                
                // Show error to user in the UI
                MessageBox.Show(
                    _localizationService.GetFormattedString("DriverUpdateErrorMessage", ex.Message),
                    _localizationService.GetString("Error", "Error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsUpdating = false;
                ProgressValue = 0;
                UpdateProgress = "";
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private async Task UpdateDriverAsync(DriverInfo driver)
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            
            try
            {
                IsUpdating = true;
                UpdateProgress = _localizationService.GetFormattedString("UpdatingDriver", driver.DeviceName);
                ProgressValue = 50;

                // Create update info object
                var updateInfo = new DriverUpdateInfo
                {
                    DriverInfo = driver,
                    HasUpdate = true,
                    UpdateAvailable = true,
                    NewVersion = driver.LatestVersion
                };
                
                var success = await _driverService.UpdateDriverAsync(updateInfo);
                
                if (success)
                {
                    driver.CurrentVersion = driver.LatestVersion;
                    driver.HasUpdate = false;
                    UpdatesAvailable--;
                    StatusText = _localizationService.GetFormattedString("UpdatedDriverSuccess", driver.DeviceName);
                    _loggingService.LogInfo($"Successfully updated driver: {driver.DeviceName}");
                }
                else
                {
                    StatusText = _localizationService.GetFormattedString("FailedToUpdateDriver", driver.DeviceName);
                    _loggingService.LogWarning($"Failed to update driver: {driver.DeviceName}");
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Failed to update driver: {driver.DeviceName}", ex);
                StatusText = _localizationService.GetFormattedString("FailedToUpdateDriver", driver.DeviceName);
                
                // Show error to user in the UI
                MessageBox.Show(
                    _localizationService.GetFormattedString("DriverUpdateErrorMessage", ex.Message),
                    _localizationService.GetString("Error", "Error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsUpdating = false;
                ProgressValue = 0;
                UpdateProgress = "";
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}
