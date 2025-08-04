using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace vFalcon.Converters
{
    public class BoolToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool boolValue)
                return Visibility.Collapsed;

            bool invert = parameter != null && bool.TryParse(parameter.ToString(), out bool p) && p;

            if (invert)
                boolValue = !boolValue;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            (value is Visibility vis) && vis == Visibility.Visible;
    }
}
