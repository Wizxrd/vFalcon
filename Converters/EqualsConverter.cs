using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace vFalcon.Converters
{
    public class EqualsConverter : IValueConverter
    {
        public object Convert(object value, Type t, object parameter, CultureInfo c)
            => value?.ToString() == parameter?.ToString();
        public object ConvertBack(object value, Type t, object parameter, CultureInfo c)
            => (value is bool b && b) ? System.Convert.ToInt32(parameter) : Binding.DoNothing;
    }
}
