using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Helpm8.Wpf
{
    public static class HelpInfo
    {
        public static string GetHelpKey(DependencyObject obj)
        {
            return (string)obj.GetValue(HelpKeyProperty);
        }

        public static void SetHelpKey(DependencyObject obj, string value)
        {
            obj.SetValue(HelpKeyProperty, value);
        }

        // Using a DependencyProperty as the backing store for HelpKey.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HelpKeyProperty =
            DependencyProperty.RegisterAttached("HelpKey", typeof(string), typeof(HelpInfo),
                new PropertyMetadata(null, OnHelpKeyChanged));

        public static IHelp GetHelpContext(DependencyObject obj)
        {
            return (IHelp)obj.GetValue(HelpContextProperty);
        }

        public static void SetHelpContext(DependencyObject obj, IHelp value)
        {
            obj.SetValue(HelpContextProperty, value);
        }

        // Using a DependencyProperty as the backing store for HelpContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HelpContextProperty =
            DependencyProperty.RegisterAttached("HelpContext", typeof(IHelp), typeof(HelpInfo),
                new FrameworkPropertyMetadata
                {
                    Inherits = true,
                    IsNotDataBindable = false,
                    DefaultValue = null,
                    SubPropertiesDoNotAffectRender = false,
                    AffectsRender = true,
                    DefaultUpdateSourceTrigger =UpdateSourceTrigger.PropertyChanged,
                    PropertyChangedCallback = OnHelpContextChanged
                });

        public static event EventHandler<HelpTargetEventArgs> HelpTargetAvailable;

        private static void OnHelpKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement uiElement)
            {
                HelpTargetAvailable?.Invoke(null, new HelpTargetEventArgs(uiElement));
            }
        }

        private static void OnHelpContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement uiElement)
            {
                HelpTargetAvailable?.Invoke(null, new HelpTargetEventArgs(uiElement));
            }
        }

    }
}
