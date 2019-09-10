using System;
using System.Globalization;
using System.Windows.Data;

namespace TogglDesktop.Converters
{
    public class EqualToValueConverter : IValueConverter
    {
        public EqualToValueConverter()
        {
            OnEqual = true;
            OnNotEqual = false;
        }

        public object ExpectedValue { get; set; }

        public object OnEqual { get; set; }

        public object OnNotEqual { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Equals(value, ExpectedValue) ? OnEqual : OnNotEqual;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}