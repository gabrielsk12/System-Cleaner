using WindowsCleaner.Models;
using WindowsCleaner.Services;

namespace WindowsCleaner.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly SettingsService _settingsService;

        public SettingsViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;
            Settings = _settingsService.Settings;

            SaveCommand = new RelayCommand(SaveSettings);
            ResetCommand = new RelayCommand(ResetSettings);
        }

        public AppSettings Settings { get; }

        public RelayCommand SaveCommand { get; }
        public RelayCommand ResetCommand { get; }

        public Array ThemeOptions => Enum.GetValues(typeof(AppTheme));
        public Array FrequencyOptions => Enum.GetValues(typeof(ScheduleFrequency));
        public Array DayOfWeekOptions => Enum.GetValues(typeof(DayOfWeek));

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
