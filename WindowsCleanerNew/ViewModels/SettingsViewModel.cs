using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using WindowsCleaner.Models;
using WindowsCleaner.Services;

namespace WindowsCleaner.ViewModels
{
    public class LanguageOption
    {
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;

        public override string ToString() => DisplayName;
    }

    public class SettingsViewModel : BaseViewModel
    {
        private readonly SettingsService _settingsService;
        private readonly LocalizationService _localizationService;
        private LanguageOption _selectedLanguage = new();

        public SettingsViewModel()
        {
            _settingsService = new SettingsService();
            _localizationService = LocalizationService.Instance;
            Settings = _settingsService.Settings;

            SaveCommand = new RelayCommand(SaveSettings);
            ResetCommand = new RelayCommand(ResetSettings);
            
            // Initialize language options
            InitializeLanguageOptions();
        }

        public SettingsViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;
            _localizationService = LocalizationService.Instance;
            Settings = _settingsService.Settings;

            SaveCommand = new RelayCommand(SaveSettings);
            ResetCommand = new RelayCommand(ResetSettings);
            
            // Initialize language options
            InitializeLanguageOptions();
        }

        public AppSettings Settings { get; }

        public RelayCommand SaveCommand { get; }
        public RelayCommand ResetCommand { get; }

        public Array ThemeOptions => Enum.GetValues(typeof(AppTheme));
        public Array FrequencyOptions => Enum.GetValues(typeof(ScheduleFrequency));
        public Array DayOfWeekOptions => Enum.GetValues(typeof(DayOfWeek));
        
        public ObservableCollection<LanguageOption> LanguageOptions { get; } = new ObservableCollection<LanguageOption>();
        
        public LanguageOption SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (SetProperty(ref _selectedLanguage, value) && value != null)
                {
                    // Apply language change
                    _localizationService.CurrentCulture = value.Culture;
                }
            }
        }
        
        private void InitializeLanguageOptions()
        {
            // Add all supported languages
            foreach (var culture in _localizationService.SupportedCultures)
            {
                LanguageOptions.Add(new LanguageOption
                {
                    Name = culture.Name,
                    DisplayName = culture.DisplayName,
                    Culture = culture
                });
            }
            
            // Set the currently selected language
            _selectedLanguage = LanguageOptions.FirstOrDefault(l => l.Culture.Name == _localizationService.CurrentCulture.Name) ?? 
                                LanguageOptions.FirstOrDefault() ?? 
                                new LanguageOption { Name = "en-US", DisplayName = "English (United States)" };
        }

        private void SaveSettings()
        {
            _settingsService.SaveSettings();
            
            System.Windows.MessageBox.Show(
                "Settings saved successfully!",
                "Settings",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }

        private void ResetSettings()
        {
            var result = System.Windows.MessageBox.Show(
                "Are you sure you want to reset all settings to default values?",
                "Reset Settings",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                // Reset to defaults
                Settings.Theme = AppTheme.System;
                Settings.StartWithWindows = false;
                Settings.ShowNotifications = true;
                Settings.ConfirmBeforeDelete = true;
                Settings.MaxLogFiles = 100;
                
                Settings.ScheduleSettings.IsEnabled = false;
                Settings.ScheduleSettings.Frequency = ScheduleFrequency.Weekly;
                Settings.ScheduleSettings.Time = new TimeSpan(2, 0, 0);
                Settings.ScheduleSettings.DayOfWeek = DayOfWeek.Sunday;
                Settings.ScheduleSettings.DayOfMonth = 1;

                _settingsService.SaveSettings();
                
                System.Windows.MessageBox.Show(
                    "Settings have been reset to default values.",
                    "Settings Reset",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
        }
    }
}
