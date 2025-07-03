using System.IO;
using System.Text.Json;
using Microsoft.Win32;
using WindowsCleaner.Models;

namespace WindowsCleaner.Services
{
    public class SettingsService
    {
        private static SettingsService? _instance;
        private readonly string _settingsPath;
        private AppSettings _settings;

        public static SettingsService Instance => _instance ??= new SettingsService();

        private SettingsService()
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
                // TODO: Implement scheduled task creation using Windows Task Scheduler COM interface
                // or alternative scheduling mechanism
                System.Diagnostics.Debug.WriteLine("Scheduled task creation not yet implemented");
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
                // TODO: Implement scheduled task removal
                System.Diagnostics.Debug.WriteLine("Scheduled task removal not yet implemented");
            }
            catch
            {
                // Task doesn't exist, ignore
            }
        }

        public T? GetSetting<T>(string key)
        {
            if (key == "Language")
            {
                return (T)(object?)_settings.Language;
            }
            
            // Reflection-based approach for other properties
            var property = typeof(AppSettings).GetProperty(key);
            if (property != null)
            {
                var value = property.GetValue(_settings);
                if (value is T typedValue)
                {
                    return typedValue;
                }
            }
            
            return default;
        }
        
        public void SaveSetting(string key, object? value)
        {
            if (key == "Language" && value is string language)
            {
                _settings.Language = language;
                SaveSettings();
                return;
            }
            
            // Reflection-based approach for other properties
            var property = typeof(AppSettings).GetProperty(key);
            if (property != null && property.CanWrite)
            {
                property.SetValue(_settings, value);
                SaveSettings();
            }
        }
    }
}
