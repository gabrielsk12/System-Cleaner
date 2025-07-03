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
        public bool IsReversed { get; set; }
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bValue = false;
            if (value is bool b) 
                bValue = b;
            else if (value != null) 
                bValue = System.Convert.ToBoolean(value);
            
            if (parameter is string strParam && strParam.ToLower() == "reverse") 
                bValue = !bValue;
            else if (IsReversed) 
                bValue = !bValue;
            
            return bValue ? Visibility.Visible : Visibility.Collapsed;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = (value is Visibility visibility && visibility == Visibility.Visible);
            if (parameter is string strParam && strParam.ToLower() == "reverse") 
                result = !result;
            else if (IsReversed) 
                result = !result;
            return result;
        }
    }

    /// <summary>
    /// Converts integer to Visibility (0 = Collapsed, non-zero = Visible)
    /// </summary>
    public class Int32ToVisibilityConverter : IValueConverter
    {
        public bool IsReversed { get; set; }
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int iValue = 0;
            if (value is int i) 
                iValue = i;
            else if (value != null) 
                iValue = System.Convert.ToInt32(value);
            
            bool isVisible = iValue != 0;
            if (parameter is string strParam && strParam.ToLower() == "reverse") 
                isVisible = !isVisible;
            else if (IsReversed) 
                isVisible = !isVisible;
            
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool isVisible = (visibility == Visibility.Visible);
                if (parameter is string strParam && strParam.ToLower() == "reverse") 
                    isVisible = !isVisible;
                else if (IsReversed) 
                    isVisible = !isVisible;
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
        public bool IsReversed { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool hasValue = value != null;
            
            if (value is string s)
            {
                hasValue = !string.IsNullOrEmpty(s);
            }
            
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Converting Visibility back to object is not supported");
        }
    }
}
