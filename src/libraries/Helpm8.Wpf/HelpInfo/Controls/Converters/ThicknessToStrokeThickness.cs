using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Helpm8.Wpf.Controls
{
    public class ThicknessToStrokeThickness : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parametersAreValid = value is Thickness && parameter is BorderSide;
            if (!parametersAreValid) return 0;

            var thickness = (Thickness)value;
            var borderSide = (BorderSide)parameter;

            return GetThicknessForSide(borderSide, thickness);
        }

        internal static double GetThicknessForSide(BorderSide borderSide, Thickness thickness)
        {
            switch (borderSide)
            {
                case BorderSide.Bottom:
                    return thickness.Bottom;
                case BorderSide.Left:
                    return thickness.Left;
                case BorderSide.Right:
                    return thickness.Right;
                case BorderSide.Top:
                    return thickness.Top;
                default:
                    throw new NotImplementedException($"Case for state '{borderSide}' is not implemeted.");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}