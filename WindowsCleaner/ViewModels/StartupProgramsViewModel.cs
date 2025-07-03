using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WindowsCleaner.Models;
using WindowsCleaner.Services;

namespace WindowsCleaner.ViewModels
{
    /// <summary>
    /// ViewModel for the Startup Programs view
    /// </summary>
public partial class StartupProgramsViewModel : BaseViewModel
    {
        private readonly WindowsUpdateService _updateService;
        private readonly LoggingService _loggingService;
        
        private bool _isLoading;
        private bool _showDetails;
        private string _selectedFilter = "All";
        private string _loadingProgress = "";
        private double _progressValue;
        private int _totalPrograms;
        private int _highImpactCount;
        private int _mediumImpactCount;
        private int _lowImpactCount;

        public StartupProgramsViewModel()
        {
            _updateService = new WindowsUpdateService();
            _loggingService = LoggingService.Instance;
            
            StartupPrograms = new ObservableCollection<StartupProgramInfo>();
            FilteredPrograms = new ObservableCollection<StartupProgramInfo>();
            
            FilterOptions = new ObservableCollection<string> { "All", "High Impact", "Medium Impact", "Low Impact", "Enabled", "Disabled" };
            
            RefreshCommand = new RelayCommand(async () => await LoadStartupProgramsAsync(), () => !IsLoading);
            OptimizeCommand = new RelayCommand(async () => await OptimizeStartupAsync(), () => !IsLoading);
            ToggleDetailsCommand = new RelayCommand(ToggleDetails);
            DisableSelectedCommand = new RelayCommand(async () => await DisableSelectedProgramsAsync(), () => HasSelectedPrograms && !IsLoading);
            ToggleStartupCommand = new RelayCommand<StartupProgramInfo>(async (program) => await ToggleStartupProgramAsync(program), (program) => program != null && !IsLoading);
            
            _ = LoadStartupProgramsAsync();
        }

        public ObservableCollection<StartupProgramInfo> StartupPrograms { get; }
        public ObservableCollection<StartupProgramInfo> FilteredPrograms { get; }
        public ObservableCollection<string> FilterOptions { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    OnPropertyChanged(nameof(IsEmpty));
                    (RefreshCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (OptimizeCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (DisableSelectedCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool ShowDetails
        {
            get => _showDetails;
            set => SetProperty(ref _showDetails, value);
        }

        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (SetProperty(ref _selectedFilter, value))
                {
                    FilterPrograms();
                }
            }
        }

        public string LoadingProgress
        {
            get => _loadingProgress;
            set => SetProperty(ref _loadingProgress, value);
        }

        public double ProgressValue
        {
            get => _progressValue;
            set => SetProperty(ref _progressValue, value);
        }

        public int TotalPrograms
        {
            get => _totalPrograms;
            set => SetProperty(ref _totalPrograms, value);
        }

        public int HighImpactCount
        {
            get => _highImpactCount;
            set => SetProperty(ref _highImpactCount, value);
        }

        public int MediumImpactCount
        {
            get => _mediumImpactCount;
            set => SetProperty(ref _mediumImpactCount, value);
        }

        public int LowImpactCount
        {
            get => _lowImpactCount;
            set => SetProperty(ref _lowImpactCount, value);
        }

        public bool HasSelectedPrograms => StartupPrograms.Any(p => p.IsSelected);
        public bool IsEmpty => !IsLoading && TotalPrograms == 0;

        public ICommand RefreshCommand { get; }
        public ICommand OptimizeCommand { get; }
        public ICommand ToggleDetailsCommand { get; }
        public ICommand DisableSelectedCommand { get; }
        public ICommand ToggleStartupCommand { get; }
        private async Task<List<StartupProgramInfo>> AnalyzeStartupProgramsAsync()
        {
            // Dummy implementation for build, replace with real logic
            await Task.Delay(100);
            return new List<StartupProgramInfo>();
        }

        private async Task DisableStartupProgramAsync(string name)
        {
            // Dummy implementation for build, replace with real logic
            await Task.Delay(50);
        }

        private async Task EnableStartupProgramAsync(string name)
        {
            // Dummy implementation for build, replace with real logic
            await Task.Delay(50);
        }

        private async Task LoadStartupProgramsAsync()
        {
            try
            {
                IsLoading = true;
                LoadingProgress = "Analyzing startup programs...";
                ProgressValue = 0;

                var programs = await AnalyzeStartupProgramsAsync();
                
                StartupPrograms.Clear();
                for (int i = 0; i < programs.Count; i++)
                {
                    var program = programs[i];
                    LoadingProgress = $"Processing {program.Name}...";
                    ProgressValue = (double)(i + 1) / programs.Count * 100;
                    
                    program.PropertyChanged += OnProgramPropertyChanged;
                    StartupPrograms.Add(program);
                    
                    await Task.Delay(50); // Small delay for UI responsiveness
                }

                TotalPrograms = StartupPrograms.Count;
                UpdateImpactCounts();
                FilterPrograms();
                
                LoadingProgress = "Analysis complete";
                
                _loggingService.LogInfo($"Startup programs analysis completed. Found {TotalPrograms} programs.");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to load startup programs", ex);
                LoadingProgress = "Failed to load startup programs. Please try again.";
            }
            finally
            {
                IsLoading = false;
                ProgressValue = 0;
                LoadingProgress = "";
            }
        }

        private void OnProgramPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StartupProgramInfo.IsSelected))
            {
                OnPropertyChanged(nameof(HasSelectedPrograms));
                (DisableSelectedCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private void UpdateImpactCounts()
        {
            HighImpactCount = StartupPrograms.Count(p => p.ImpactLevel == StartupImpact.High);
            MediumImpactCount = StartupPrograms.Count(p => p.ImpactLevel == StartupImpact.Medium);
            LowImpactCount = StartupPrograms.Count(p => p.ImpactLevel == StartupImpact.Low);
        }

        private void FilterPrograms()
        {
            FilteredPrograms.Clear();
            
            var filtered = StartupPrograms.AsEnumerable();
            
            switch (SelectedFilter)
            {
                case "High Impact":
                    filtered = filtered.Where(p => p.ImpactLevel == StartupImpact.High);
                    break;
                case "Medium Impact":
                    filtered = filtered.Where(p => p.ImpactLevel == StartupImpact.Medium);
                    break;
                case "Low Impact":
                    filtered = filtered.Where(p => p.ImpactLevel == StartupImpact.Low);
                    break;
                case "Enabled":
                    filtered = filtered.Where(p => p.IsEnabled);
                    break;
                case "Disabled":
                    filtered = filtered.Where(p => !p.IsEnabled);
                    break;
            }
            
            foreach (var program in filtered)
            {
                FilteredPrograms.Add(program);
            }
        }

        private void ToggleDetails()
        {
            ShowDetails = !ShowDetails;
        }

        private async Task OptimizeStartupAsync()
        {
            try
            {
                IsLoading = true;
                LoadingProgress = "Optimizing startup programs...";
                ProgressValue = 0;

                var highImpactPrograms = StartupPrograms.Where(p => p.ImpactLevel == StartupImpact.High && p.IsEnabled).ToList();
                
                for (int i = 0; i < highImpactPrograms.Count; i++)
                {
                    var program = highImpactPrograms[i];
                    LoadingProgress = $"Disabling {program.Name}...";
                    ProgressValue = (double)(i + 1) / highImpactPrograms.Count * 100;
                    
                    await DisableStartupProgramAsync(program.Name);
                    program.IsEnabled = false;
                    
                    await Task.Delay(500);
                }

                LoadingProgress = "Startup optimization complete";
                UpdateImpactCounts();
                FilterPrograms();
                
                _loggingService.LogInfo($"Startup optimization completed. Disabled {highImpactPrograms.Count} high-impact programs.");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to optimize startup programs", ex);
                LoadingProgress = "Failed to optimize startup. Please try again.";
            }
            finally
            {
                IsLoading = false;
                ProgressValue = 0;
                LoadingProgress = "";
            }
        }

        private async Task DisableSelectedProgramsAsync()
        {
            try
            {
                IsLoading = true;
                var selectedPrograms = StartupPrograms.Where(p => p.IsSelected).ToList();
                
                for (int i = 0; i < selectedPrograms.Count; i++)
                {
                    var program = selectedPrograms[i];
                    LoadingProgress = $"Disabling {program.Name}... ({i + 1}/{selectedPrograms.Count})";
                    ProgressValue = (double)(i + 1) / selectedPrograms.Count * 100;
                    
                    if (program.IsEnabled)
                    {
                        await DisableStartupProgramAsync(program.Name);
                        program.IsEnabled = false;
                    }
                    
                    program.IsSelected = false;
                    await Task.Delay(300);
                }

                LoadingProgress = "Selected programs disabled";
                UpdateImpactCounts();
                FilterPrograms();
                
                _loggingService.LogInfo($"Disabled {selectedPrograms.Count} selected startup programs.");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to disable selected startup programs", ex);
                LoadingProgress = "Failed to disable programs. Please try again.";
            }
            finally
            {
                IsLoading = false;
                ProgressValue = 0;
                LoadingProgress = "";
            }
        }

        private async Task ToggleStartupProgramAsync(StartupProgramInfo program)
        {
            try
            {
                IsLoading = true;
                
                if (program.IsEnabled)
                {
                    LoadingProgress = $"Disabling {program.Name}...";
                    await DisableStartupProgramAsync(program.Name);
                    program.IsEnabled = false;
                    _loggingService.LogInfo($"Disabled startup program: {program.Name}");
                }
                else
                {
                    LoadingProgress = $"Enabling {program.Name}...";
                    await EnableStartupProgramAsync(program.Name);
                    program.IsEnabled = true;
                    _loggingService.LogInfo($"Enabled startup program: {program.Name}");
                }

                UpdateImpactCounts();
                FilterPrograms();
                
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Failed to toggle startup program: {program.Name}", ex);
                LoadingProgress = $"Failed to toggle {program.Name}. Please try again.";
            }
            finally
            {
                IsLoading = false;
                LoadingProgress = "";
            }
        }
    }
}
