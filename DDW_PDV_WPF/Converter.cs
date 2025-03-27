using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DDW_PDV_WPF
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEmpty = string.IsNullOrEmpty(value as string);

            if (parameter?.ToString() == "inverse")
                return isEmpty ? Visibility.Visible : Visibility.Collapsed;

            return isEmpty ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}