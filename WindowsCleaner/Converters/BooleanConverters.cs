using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WindowsCleaner.Converters
{
    /// <summary>
    /// Converts boolean to Visibility
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// When true, converts true to Visible and false to Collapsed
        /// When false, converts true to Collapsed and false to Visible
        /// </summary>
        public bool IsReversed { get; set; }

        /// <summary>
        /// Converts boolean to Visibility
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bValue = false;

            if (value is bool b)
            {
                bValue = b;
            }
            else if (value != null)
            {
                bValue = System.Convert.ToBoolean(value);
            }

            // Check for parameter override (allows inline reversing)
            if (parameter is string strParam && strParam.ToLower() == "reverse")
            {
                bValue = !bValue;
            }
            else if (IsReversed)
            {
                bValue = !bValue;
            }

            return bValue ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts Visibility back to boolean
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = (value is Visibility visibility && visibility == Visibility.Visible);

            // Check for parameter override (allows inline reversing)
            if (parameter is string strParam && strParam.ToLower() == "reverse")
            {
                result = !result;
            }
            else if (IsReversed)
            {
                result = !result;
            }

            return result;
        }
    }

    /// <summary>
    /// Converts integer to Visibility (0 = Collapsed, non-zero = Visible)
    /// </summary>
    public class Int32ToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// When true, converts non-zero to Collapsed and zero to Visible
        /// When false, converts non-zero to Visible and zero to Collapsed
        /// </summary>
        public bool IsReversed { get; set; }
        
        /// <summary>
        /// Converts integer to Visibility
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int iValue = 0;
            
            if (value is int i)
            {
                iValue = i;
            }
            else if (value != null)
            {
                iValue = System.Convert.ToInt32(value);
            }

            bool isVisible = iValue != 0;
            
            // Check for parameter override
            if (parameter is string strParam && strParam.ToLower() == "reverse")
            {
                isVisible = !isVisible;
            }
            else if (IsReversed)
            {
                isVisible = !isVisible;
            }

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts Visibility back to integer
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool isVisible = (visibility == Visibility.Visible);
                
                // Handle reversal if needed
                if (parameter is string strParam && strParam.ToLower() == "reverse")
                {
                    isVisible = !isVisible;
                }
                else if (IsReversed)
                {
                    isVisible = !isVisible;
                }
                
                return isVisible ? 1 : 0;
            }

            return 0;
        }
    }

    /// <summary>
    /// Converts an object to Visibility - returns Visible if object is not null, otherwise Collapsed
    /// </summary>
    public class ObjectToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// When true, converts null to Visible and non-null to Collapsed
        /// When false, converts null to Collapsed and non-null to Visible
        /// </summary>
        public bool IsReversed { get; set; }

        /// <summary>
        /// Converts an object to Visibility
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool hasValue = value != null;
            
            // For strings, also check if it's empty
            if (value is string s)
            {
                hasValue = !string.IsNullOrEmpty(s);
            }
            
            // Check for parameter override (allows inline reversing)
            if (parameter is string strParam && strParam.ToLower() == "reverse")
            {
                hasValue = !hasValue;
            }
            else if (IsReversed)
            {
                hasValue = !hasValue;
            }

            return hasValue ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts Visibility back to boolean (indicating if object is null)
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Converting Visibility back to object is not supported");
        }
    }
}
