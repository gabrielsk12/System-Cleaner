using System.IO;
using System.Text.Json;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using WindowsCleaner.Models;

namespace WindowsCleaner.Services
{
    public class SettingsService
    {
        private readonly string _settingsPath;
        private AppSettings _settings;

        public SettingsService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appDataPath, "WindowsCleaner");
            Directory.CreateDirectory(appFolder);
            _settingsPath = Path.Combine(appFolder, "settings.json");
            
            _settings = LoadSettings();
        }

        public AppSettings Settings => _settings;

        public event EventHandler<AppTheme>? ThemeChanged;

        public void SaveSettings()
        {
            try
            {
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(_settingsPath, json);
                
                // Apply settings
                ApplyStartupSetting();
                ApplyScheduledTask();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public void ApplyTheme(AppTheme theme)
        {
            _settings.Theme = theme;
            ThemeChanged?.Invoke(this, theme);
            SaveSettings();
        }

        private AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }
            
            return new AppSettings();
        }

        private void ApplyStartupSetting()
        {
            try
            {
                const string keyName = "WindowsCleanerPro";
                var startupKey = Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

                if (_settings.StartWithWindows)
                {
                    var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        startupKey?.SetValue(keyName, $"\"{exePath}\" --minimized");
                    }
                }
                else
                {
                    startupKey?.DeleteValue(keyName, false);
                }

                startupKey?.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting startup: {ex.Message}");
            }
        }

        private void ApplyScheduledTask()
        {
            if (!_settings.ScheduleSettings.IsEnabled)
            {
                RemoveScheduledTask();
                return;
            }

            try
            {
                using var taskService = new TaskService();
                
                // Remove existing task
                RemoveScheduledTask();

                // Create new task
                var task = taskService.NewTask();
                task.RegistrationInfo.Description = "Automatic Windows system cleaning";
                task.Principal.RunLevel = TaskRunLevel.Highest;

                // Set trigger based on frequency
                Trigger trigger = _settings.ScheduleSettings.Frequency switch
                {
                    ScheduleFrequency.Daily => new DailyTrigger 
                    { 
                        StartBoundary = DateTime.Today.Add(_settings.ScheduleSettings.Time) 
                    },
                    ScheduleFrequency.Weekly => new WeeklyTrigger((Microsoft.Win32.TaskScheduler.DaysOfTheWeek)(1 << (int)_settings.ScheduleSettings.DayOfWeek)) 
                    { 
                        StartBoundary = DateTime.Today.Add(_settings.ScheduleSettings.Time) 
                    },
                    ScheduleFrequency.Monthly => new MonthlyTrigger(_settings.ScheduleSettings.DayOfMonth) 
                    { 
                        StartBoundary = DateTime.Today.Add(_settings.ScheduleSettings.Time) 
                    },
                    _ => new DailyTrigger { StartBoundary = DateTime.Today.Add(_settings.ScheduleSettings.Time) }
                };

                task.Triggers.Add(trigger);

                // Set action
                var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrEmpty(exePath))
                {
                    task.Actions.Add(new ExecAction(exePath, "--auto-clean"));
                }

                // Register task
                taskService.RootFolder.RegisterTaskDefinition(
                    "WindowsCleanerPro_AutoClean", task);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating scheduled task: {ex.Message}");
            }
        }

        private void RemoveScheduledTask()
        {
            try
            {
                using var taskService = new TaskService();
                taskService.RootFolder.DeleteTask("WindowsCleanerPro_AutoClean", false);
            }
            catch
            {
                // Task doesn't exist, ignore
            }
        }
    }
}
