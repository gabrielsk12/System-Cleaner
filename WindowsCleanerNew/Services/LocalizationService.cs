using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Threading;
using System.Windows;

namespace WindowsCleaner.Services
{
    /// <summary>
    /// Service for handling localization and language support
    /// </summary>
    public class LocalizationService
    {
        private static LocalizationService? _instance;
        private CultureInfo _currentCulture;
        private readonly Dictionary<string, ResourceManager> _resourceManagers;
        private const string ResourcePath = "WindowsCleaner.Resources.Strings.{0}";

        public static LocalizationService Instance => _instance ??= new LocalizationService();

        public event EventHandler<CultureInfo>? LanguageChanged;

        private LocalizationService()
        {
            // Initialize with the current culture or default to English
            _currentCulture = GetInitialCulture();
            _resourceManagers = new Dictionary<string, ResourceManager>();
            InitializeResourceManagers();
        }
        
        private CultureInfo GetInitialCulture()
        {
            try
            {
                // Check if there's a saved language preference
                var settings = SettingsService.Instance;
                var savedLanguage = settings.GetSetting<string>("Language");

                if (!string.IsNullOrEmpty(savedLanguage))
                {
                    try
                    {
                        return new CultureInfo(savedLanguage);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to load saved culture {savedLanguage}: {ex.Message}");
                    }
                }

                // Fall back to system language if supported
                var systemCulture = CultureInfo.CurrentCulture;
                if (IsCultureSupported(systemCulture))
                    return systemCulture;

                // Default to English
                return new CultureInfo("en-US");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error determining initial culture: {ex.Message}");
                return new CultureInfo("en-US");
            }
        }
        
        private bool IsCultureSupported(CultureInfo culture)
        {
            foreach (var supportedCulture in SupportedCultures)
            {
                if (supportedCulture.Name == culture.Name || 
                    supportedCulture.TwoLetterISOLanguageName == culture.TwoLetterISOLanguageName)
                    return true;
            }
            return false;
        }

        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set
            {
                if (_currentCulture != value)
                {
                    _currentCulture = value;
                    Thread.CurrentThread.CurrentCulture = value;
                    Thread.CurrentThread.CurrentUICulture = value;
                    
                    // Update WPF resource dictionaries
                    UpdateResourceDictionaries();
                    
                    // Save selected language
                    SaveSelectedLanguage(value.Name);
                    
                    // Notify subscribers about the language change
                    LanguageChanged?.Invoke(this, value);
                }
            }
        }
        
        private void SaveSelectedLanguage(string cultureName)
        {
            try
            {
                var settings = SettingsService.Instance;
                settings.SaveSetting("Language", cultureName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save language preference: {ex.Message}");
            }
        }
        
        private void UpdateResourceDictionaries()
        {
            try
            {
                // For dynamic XAML localization, update ResourceDictionary
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Reload the resource dictionary
                    var dictionaries = Application.Current.Resources.MergedDictionaries;
                    
                    // Find and reload the localized strings dictionary
                    foreach (var dict in dictionaries)
                    {
                        if (dict.Source != null && dict.Source.ToString().Contains("LocalizedStrings.xaml"))
                        {
                            var newDict = new ResourceDictionary();
                            newDict.Source = dict.Source;
                            
                            var index = dictionaries.IndexOf(dict);
                            dictionaries.Remove(dict);
                            dictionaries.Insert(index, newDict);
                            break;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to update ResourceDictionaries: {ex.Message}");
            }
        }

        public IEnumerable<CultureInfo> SupportedCultures => new[]
        {
            new CultureInfo("en-US"), // English
            new CultureInfo("sk-SK"), // Slovak
            new CultureInfo("cs-CZ"), // Czech
            new CultureInfo("ru-RU"), // Russian
            new CultureInfo("de-DE"), // German
            new CultureInfo("ko-KR"), // Korean
            new CultureInfo("ja-JP"), // Japanese
            new CultureInfo("zh-CN")  // Chinese (Simplified)
        };

        private void InitializeResourceManagers()
        {
            // Initialize resource managers for all supported cultures
            foreach (var culture in SupportedCultures)
            {
                try
                {
                    var resourceName = string.Format(ResourcePath, culture.Name);
                    var resourceManager = new ResourceManager(resourceName, typeof(LocalizationService).Assembly);
                    
                    // Test if the resource manager can access at least one string
                    // This will throw an exception if the resource doesn't exist
                    resourceManager.GetString("AppTitle");
                    
                    _resourceManagers[culture.Name] = resourceManager;
                    System.Diagnostics.Debug.WriteLine($"Successfully loaded resources for {culture.Name}");
                }
                catch (Exception ex)
                {
                    // Log error but continue - fallback to default culture
                    System.Diagnostics.Debug.WriteLine($"Failed to load resources for {culture.Name}: {ex.Message}");
                }
            }
            
            // Always ensure we have at least the English resources
            if (!_resourceManagers.ContainsKey("en-US"))
            {
                try
                {
                    var resourceName = string.Format(ResourcePath, "en-US");
                    var resourceManager = new ResourceManager(resourceName, typeof(LocalizationService).Assembly);
                    _resourceManagers["en-US"] = resourceManager;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"CRITICAL: Failed to load default English resources: {ex.Message}");
                }
            }
        }

        public string GetString(string key, string? defaultValue = null)
        {
            try
            {
                if (_resourceManagers.TryGetValue(_currentCulture.Name, out var resourceManager))
                {
                    var value = resourceManager.GetString(key);
                    if (!string.IsNullOrEmpty(value))
                        return value;
                }

                // Fallback to English
                if (_resourceManagers.TryGetValue("en-US", out var fallbackManager))
                {
                    var value = fallbackManager.GetString(key);
                    if (!string.IsNullOrEmpty(value))
                        return value;
                }

                // Return default value or key if nothing found
                return defaultValue ?? key;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Localization error for key '{key}': {ex.Message}");
                return defaultValue ?? key;
            }
        }

        public string GetFormattedString(string key, params object[] args)
        {
            var format = GetString(key);
            try
            {
                return string.Format(_currentCulture, format, args);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"String formatting error for key '{key}': {ex.Message}");
                return format;
            }
        }
    }
}
