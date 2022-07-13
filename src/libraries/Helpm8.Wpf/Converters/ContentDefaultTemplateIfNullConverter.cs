using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Helpm8.Wpf.Converters
{
    public class ContentDefaultTemplateIfNullConverter : IValueConverter
    {
        public DataTemplate DefaultTemplate { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var customDataTemplate = value as DataTemplate;
            return customDataTemplate ?? DefaultTemplate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}