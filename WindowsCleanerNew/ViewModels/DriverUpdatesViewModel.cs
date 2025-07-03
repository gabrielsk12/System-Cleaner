using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WindowsCleaner.Models;
using WindowsCleaner.Services;

namespace WindowsCleaner.ViewModels
{
    public partial class DriverUpdatesViewModel : BaseViewModel
    {
        private readonly DriverService _driverService;
        private bool _isLoading;
        private string _statusMessage = string.Empty;

        public DriverUpdatesViewModel()
        {
            _driverService = new DriverService();
            AvailableDrivers = new ObservableCollection<DriverInfo>();
            
            RefreshCommand = new RelayCommand(async () => await RefreshDriversAsync());
            UpdateDriverCommand = new RelayCommand<DriverInfo>(async (driver) => await UpdateDriverAsync(driver));
            UpdateAllCommand = new RelayCommand(async () => await UpdateAllDriversAsync());
            
            // Load drivers when the view model is created
            _ = RefreshDriversAsync();
        }

        public ObservableCollection<DriverInfo> AvailableDrivers { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand UpdateDriverCommand { get; }
        public ICommand UpdateAllCommand { get; }

        private async Task RefreshDriversAsync()
        {
            IsLoading = true;
            StatusMessage = "Scanning for driver updates...";
            
            try
            {
                AvailableDrivers.Clear();
                var drivers = await _driverService.GetAvailableDriversAsync();
                
                foreach (var driver in drivers)
                {
                    AvailableDrivers.Add(driver);
                }
                
                StatusMessage = $"Found {AvailableDrivers.Count} driver(s) available for update";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error scanning drivers: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UpdateDriverAsync(DriverInfo? driver)
        {
            if (driver == null) return;

            IsLoading = true;
            StatusMessage = $"Updating {driver.Name}...";
            
            try
            {
                await _driverService.UpdateDriverAsync(driver);
                StatusMessage = $"Successfully updated {driver.Name}";
                await RefreshDriversAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating {driver.Name}: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UpdateAllDriversAsync()
        {
            if (!AvailableDrivers.Any()) return;

            IsLoading = true;
            StatusMessage = "Updating all drivers...";
            
            try
            {
                foreach (var driver in AvailableDrivers.ToList())
                {
                    await _driverService.UpdateDriverAsync(driver);
                }
                
                StatusMessage = "Successfully updated all drivers";
                await RefreshDriversAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating drivers: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
