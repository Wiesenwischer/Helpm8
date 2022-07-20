using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Helpm8.Wpf.Controls;

namespace Helpm8.Wpf.Converters
{
    public class BorderThicknessToContainerMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parametersAreValid = value is Thickness && parameter is BorderSide;
            if (!parametersAreValid) return new Thickness(0);

            var thickness = (Thickness)value;
            var borderSide = (BorderSide)parameter;

            var borderThickness = GetThicknessForSide(borderSide, thickness);

            var marginFromCorner = 10 + borderThickness;

            switch (borderSide)
            {
                case BorderSide.Bottom: // 10, 0, 10, 0
                    return new Thickness(marginFromCorner, -1, marginFromCorner, 0);
                case BorderSide.Left: // 0, 10, 0, 10
                    return new Thickness(0, marginFromCorner, -1, marginFromCorner);
                case BorderSide.Right: // 0, 10, 0, 10
                    return new Thickness(-1, marginFromCorner, 0, marginFromCorner);
                case BorderSide.Top: // 10, 0, 10, 0
                    return new Thickness(marginFromCorner, 0, marginFromCorner, -1);
                default:
                    throw new ArgumentOutOfRangeException($"State '{borderSide}' is not a valid state.");
            }
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
                    throw new ArgumentOutOfRangeException($"State '{borderSide}' is not a valid state.");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}