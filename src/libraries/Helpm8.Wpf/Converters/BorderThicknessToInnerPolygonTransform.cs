using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Helpm8.Wpf.Controls;

namespace Helpm8.Wpf.Converters
{
    public class BorderThicknessToInnerPolygonTransform : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parametersAreValid = value is Thickness && parameter is BorderSide;
            if (!parametersAreValid) return 0;

            var thickness = (Thickness)value;
            var borderSide = (BorderSide)parameter;

            var borderThickness = ThicknessToStrokeThickness.GetThicknessForSide(borderSide, thickness);

            if (borderSide == BorderSide.Bottom || borderSide == BorderSide.Right)
                borderThickness *= -1;

            return borderThickness;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}