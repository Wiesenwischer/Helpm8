using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Helpm8WpfSample.Converters
{
    [Localizability(LocalizationCategory.NeverLocalize)]
    public sealed class InvertedBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bValue = false;
            if (value is bool b)
            {
                bValue = b;
            }

            return (bValue) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
