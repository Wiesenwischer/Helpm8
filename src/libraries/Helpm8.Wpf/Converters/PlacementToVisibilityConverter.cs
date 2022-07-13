using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Helpm8.Wpf.Controls;

namespace Helpm8.Wpf.Converters
{
    public class PlacementToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var border = (BorderSide)parameter;
            var placement = (Placement)value;

            if (placement == Placement.BottomCenter ||
                placement == Placement.BottomLeft ||
                placement == Placement.BottomRight)
                return border == BorderSide.Top ? Visibility.Visible : Visibility.Collapsed;

            if (placement == Placement.TopCenter ||
                placement == Placement.TopLeft ||
                placement == Placement.TopRight)
                return border == BorderSide.Bottom ? Visibility.Visible : Visibility.Collapsed;

            if (placement == Placement.LeftBottom ||
                placement == Placement.LeftCenter ||
                placement == Placement.LeftTop)
                return border == BorderSide.Right ? Visibility.Visible : Visibility.Collapsed;

            if (placement == Placement.RightBottom ||
                placement == Placement.RightCenter ||
                placement == Placement.RightTop)
                return border == BorderSide.Left ? Visibility.Visible : Visibility.Collapsed;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}