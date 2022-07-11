using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Helpm8.Wpf.Controls
{
    public class PlacementToAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var border = (BorderSide)parameter;
            var placement = (Placement)value;
            if (placement == Placement.Center ||
                placement == Placement.BottomCenter ||
                placement == Placement.TopCenter ||
                placement == Placement.LeftCenter ||
                placement == Placement.RightCenter)
            {
                if (border == BorderSide.Bottom || border == BorderSide.Top)
                    return HorizontalAlignment.Center;
                return VerticalAlignment.Center;
            }
            if (placement == Placement.BottomLeft ||
                placement == Placement.TopLeft ||
                placement == Placement.LeftTop ||
                placement == Placement.RightTop)
            {
                if (border == BorderSide.Bottom || border == BorderSide.Top)
                    return HorizontalAlignment.Left;
                return VerticalAlignment.Top;
            }

            if (border == BorderSide.Bottom || border == BorderSide.Top)
                return HorizontalAlignment.Right;
            return VerticalAlignment.Bottom;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}