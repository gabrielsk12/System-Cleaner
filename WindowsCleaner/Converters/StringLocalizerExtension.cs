using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using WindowsCleaner.Services;

namespace WindowsCleaner.Converters
{
    /// <summary>
    /// Markup extension that provides localized strings from resources
    /// </summary>
    [MarkupExtensionReturnType(typeof(string))]
    public class LocalizeExtension : MarkupExtension
    {
        private readonly LocalizationService _localizationService;
        private string _key = string.Empty;

        /// <summary>
        /// Resource key for the localized string
        /// </summary>
        public string Key
        {
            get => _key;
            set => _key = value;
        }

        /// <summary>
        /// Optional default value if key is not found
        /// </summary>
        public string DefaultValue { get; set; } = string.Empty;

        /// <summary>
        /// Constructor
        /// </summary>
        public LocalizeExtension()
        {
            _localizationService = LocalizationService.Instance;
        }

        /// <summary>
        /// Constructor with resource key
        /// </summary>
        public LocalizeExtension(string key)
        {
            _key = key;
            _localizationService = LocalizationService.Instance;
        }

        /// <summary>
        /// Returns the localized string for the specified key
        /// </summary>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(_key))
                return string.Empty;
            
            try
            {
                // Create a binding to the application resource
                var binding = new Binding
                {
                    Source = Application.Current.Resources,
                    Path = new PropertyPath($"[{_key}]"),
                    FallbackValue = DefaultValue ?? _key
                };
                
                // Get the target object and property
                var provideValueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
                if (provideValueTarget == null)
                    return _localizationService.GetString(_key, DefaultValue);

                // If we're in design mode, return a direct string
                if (provideValueTarget.TargetObject.GetType().FullName == "System.Windows.SharedDp")
                    return _localizationService.GetString(_key, DefaultValue);

                // Return the binding for runtime
                return binding.ProvideValue(serviceProvider);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Localization error: {ex.Message}");
                return _key; // Return the key as fallback
            }
        }
    }

    /// <summary>
    /// Markup extension that provides format string with parameters
    /// </summary>
    [MarkupExtensionReturnType(typeof(string))]
    public class LocalizeFormatExtension : MarkupExtension
    {
        private readonly LocalizationService _localizationService;
        private string _key = string.Empty;
        private object[] _parameters = Array.Empty<object>();

        /// <summary>
        /// Resource key for the localized string
        /// </summary>
        public string Key
        {
            get => _key;
            set => _key = value;
        }

        /// <summary>
        /// Format parameters
        /// </summary>
        public object[] Parameters
        {
            get => _parameters;
            set => _parameters = value;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public LocalizeFormatExtension()
        {
            _localizationService = LocalizationService.Instance;
        }

        /// <summary>
        /// Constructor with resource key
        /// </summary>
        public LocalizeFormatExtension(string key)
        {
            _key = key;
            _localizationService = LocalizationService.Instance;
        }

        /// <summary>
        /// Returns the localized and formatted string
        /// </summary>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(_key))
                return string.Empty;

            try
            {
                return _localizationService.GetFormattedString(_key, _parameters);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Localization format error: {ex.Message}");
                return _key; // Return the key as fallback
            }
        }
    }
}
