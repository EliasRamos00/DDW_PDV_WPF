using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DDW_PDV_WPF
{
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;

            // Verificación segura y compatible con cualquier versión de C#
            if (value != null && value is bool)
            {
                flag = (bool)value;
            }

            return flag ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Verificación segura para el regreso del valor
            if (value != null && value is Visibility)
            {
                Visibility v = (Visibility)value;
                return v != Visibility.Visible;
            }

            return Binding.DoNothing;
        }
    }
}