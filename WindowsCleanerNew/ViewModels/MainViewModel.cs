using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WindowsCleaner.Models;
using WindowsCleaner.Services;
using WindowsCleaner.ViewModels;

namespace WindowsCleaner.ViewModels
{
    // Forward declarations to avoid circular dependencies
    public partial class DriverUpdatesViewModel { }
    public partial class WindowsUpdatesViewModel { }
    public partial class StartupProgramsViewModel { }

    public class MainViewModel : BaseViewModel
    {
        private readonly CleanerService _cleanerService;
        private readonly SettingsService _settingsService;
        private bool _isScanning;
        private bool _isCleaning;
        private double _progressValue;
        private string _statusText = "Ready to scan";
        private long _totalSizeToClean;
        private string _totalSizeText = "0 MB";

        public MainViewModel()
        {
            _cleanerService = new CleanerService();
            _settingsService = SettingsService.Instance;
            CleanupCategories = new ObservableCollection<CleanupCategory>();
            
            // Initialize view models for tabs
            FileExplorerViewModel = new FileExplorerViewModel();
            DriverUpdatesViewModel = new DriverUpdatesViewModel();
            WindowsUpdatesViewModel = new WindowsUpdatesViewModel();
            StartupProgramsViewModel = new StartupProgramsViewModel();
            SettingsViewModel = new SettingsViewModel(_settingsService);
            
            InitializeCategories();

            ScanCommand = new RelayCommand(async () => await ScanForFilesAsync(), () => !IsScanning && !IsCleaning);
            CleanCommand = new RelayCommand(async () => await CleanSelectedFilesAsync(), () => !IsScanning && !IsCleaning && HasSelectedItems);
            SelectAllCommand = new RelayCommand(SelectAllCategories);
            SelectNoneCommand = new RelayCommand(SelectNoneCategories);
        }

        public ObservableCollection<CleanupCategory> CleanupCategories { get; }
        public FileExplorerViewModel FileExplorerViewModel { get; }
        public DriverUpdatesViewModel DriverUpdatesViewModel { get; }
        public WindowsUpdatesViewModel WindowsUpdatesViewModel { get; }
        public StartupProgramsViewModel StartupProgramsViewModel { get; }
        public SettingsViewModel SettingsViewModel { get; }

        public bool IsScanning
        {
            get => _isScanning;
            set
            {
                SetProperty(ref _isScanning, value);
                ((RelayCommand)ScanCommand).RaiseCanExecuteChanged();
                ((RelayCommand)CleanCommand).RaiseCanExecuteChanged();
            }
        }

        public bool IsCleaning
        {
            get => _isCleaning;
            set
            {
                SetProperty(ref _isCleaning, value);
                ((RelayCommand)ScanCommand).RaiseCanExecuteChanged();
                ((RelayCommand)CleanCommand).RaiseCanExecuteChanged();
            }
        }

        public double ProgressValue
        {
            get => _progressValue;
            set => SetProperty(ref _progressValue, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public string TotalSizeText
        {
            get => _totalSizeText;
            set => SetProperty(ref _totalSizeText, value);
        }

        public bool HasSelectedItems => CleanupCategories.Any(c => c.IsSelected && c.FilesFound > 0);

        public ICommand ScanCommand { get; }
        public ICommand CleanCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand SelectNoneCommand { get; }

        private void InitializeCategories()
        {
            CleanupCategories.Add(new CleanupCategory
            {
                Name = "Windows Update Files",
                Description = "Downloaded update files and installation cache",
                IsSelected = true,
                Category = CleanupType.WindowsUpdate
            });

            CleanupCategories.Add(new CleanupCategory
            {
                Name = "Temporary Files",
                Description = "System and application temporary files",
                IsSelected = true,
                Category = CleanupType.TemporaryFiles
            });

            CleanupCategories.Add(new CleanupCategory
            {
                Name = "Recycle Bin",
                Description = "Files in the Recycle Bin",
                IsSelected = false,
                Category = CleanupType.RecycleBin
            });

            CleanupCategories.Add(new CleanupCategory
            {
                Name = "System Cache",
                Description = "System cache and prefetch files",
                IsSelected = true,
                Category = CleanupType.SystemCache
            });

            CleanupCategories.Add(new CleanupCategory
            {
                Name = "Browser Cache",
                Description = "Web browser cache and temporary internet files",
                IsSelected = true,
                Category = CleanupType.BrowserCache
            });

            CleanupCategories.Add(new CleanupCategory
            {
                Name = "Log Files",
                Description = "System and application log files",
                IsSelected = true,
                Category = CleanupType.LogFiles
            });

            CleanupCategories.Add(new CleanupCategory
            {
                Name = "Error Reports",
                Description = "Windows error reporting files",
                IsSelected = true,
                Category = CleanupType.ErrorReports
            });

            CleanupCategories.Add(new CleanupCategory
            {
                Name = "Thumbnail Cache",
                Description = "Image and video thumbnail cache",
                IsSelected = true,
                Category = CleanupType.ThumbnailCache
            });

            // Subscribe to property changes for each category
            foreach (var category in CleanupCategories)
            {
                category.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(CleanupCategory.IsSelected))
                    {
                        OnPropertyChanged(nameof(HasSelectedItems));
                        ((RelayCommand)CleanCommand).RaiseCanExecuteChanged();
                        UpdateTotalSize();
                    }
                };
            }
        }

        private async Task ScanForFilesAsync()
        {
            IsScanning = true;
            ProgressValue = 0;
            StatusText = "Scanning system files...";

            try
            {
                var progress = new Progress<ScanProgress>(UpdateScanProgress);
                var categories = CleanupCategories.Where(c => c.IsSelected).ToList();

                await _cleanerService.ScanForFilesAsync(categories, progress);

                StatusText = $"Scan complete. Found {CleanupCategories.Sum(c => c.FilesFound)} files to clean.";
                UpdateTotalSize();
            }
            catch (Exception ex)
            {
                StatusText = $"Scan failed: {ex.Message}";
                MessageBox.Show($"An error occurred during scanning: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsScanning = false;
                ProgressValue = 0;
            }
        }

        private async Task CleanSelectedFilesAsync()
        {
            var selectedCategories = CleanupCategories.Where(c => c.IsSelected && c.FilesFound > 0).ToList();
            
            if (!selectedCategories.Any())
            {
                MessageBox.Show("No files selected for cleaning.", "Information", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete {selectedCategories.Sum(c => c.FilesFound)} files ({TotalSizeText})?\n\n" +
                "This action cannot be undone.",
                "Confirm Cleanup",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            IsCleaning = true;
            ProgressValue = 0;
            StatusText = "Cleaning files...";

            try
            {
                var progress = new Progress<CleanProgress>(UpdateCleanProgress);
                var cleanedFiles = await _cleanerService.CleanFilesAsync(selectedCategories, progress);

                StatusText = $"Cleanup complete. Removed {cleanedFiles.TotalFiles} files, freed {FormatBytes(cleanedFiles.TotalSize)}.";
                
                // Reset the categories
                foreach (var category in selectedCategories)
                {
                    category.FilesFound = 0;
                    category.SizeInBytes = 0;
                }
                
                UpdateTotalSize();

                MessageBox.Show(
                    $"Cleanup completed successfully!\n\n" +
                    $"Files removed: {cleanedFiles.TotalFiles:N0}\n" +
                    $"Space freed: {FormatBytes(cleanedFiles.TotalSize)}",
                    "Cleanup Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusText = $"Cleanup failed: {ex.Message}";
                MessageBox.Show($"An error occurred during cleanup: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsCleaning = false;
                ProgressValue = 0;
            }
        }

        private void UpdateScanProgress(ScanProgress progress)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ProgressValue = progress.ProgressPercentage;
                StatusText = progress.CurrentOperation;
                
                var category = CleanupCategories.FirstOrDefault(c => c.Category == progress.Category);
                if (category != null)
                {
                    category.FilesFound = progress.FilesFound;
                    category.SizeInBytes = progress.SizeInBytes;
                }
                
                UpdateTotalSize();
            });
        }

        private void UpdateCleanProgress(CleanProgress progress)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ProgressValue = progress.ProgressPercentage;
                StatusText = progress.CurrentOperation;
            });
        }

        private void UpdateTotalSize()
        {
            _totalSizeToClean = CleanupCategories.Where(c => c.IsSelected).Sum(c => c.SizeInBytes);
            TotalSizeText = FormatBytes(_totalSizeToClean);
            OnPropertyChanged(nameof(HasSelectedItems));
            ((RelayCommand)CleanCommand).RaiseCanExecuteChanged();
        }

        private void SelectAllCategories()
        {
            foreach (var category in CleanupCategories)
            {
                category.IsSelected = true;
            }
        }

        private void SelectNoneCategories()
        {
            foreach (var category in CleanupCategories)
            {
                category.IsSelected = false;
            }
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F1} MB";
            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F1} GB";
        }
    }
}
