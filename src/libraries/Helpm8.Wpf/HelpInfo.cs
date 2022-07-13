using Helpm8.Wpf.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Helpm8.Wpf
{
    public class HelpInfo : DependencyObject
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
                new PropertyMetadata(null, OnHelpTextChanged));

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
                    PropertyChangedCallback = OnHelpContextChanged
                });


        private static void OnHelpContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UpdateHelpText(d);
        }

        private static void UpdateHelpText(DependencyObject d)
        {
            var ctx = GetHelpContext(d);
            var key = GetHelpKey(d);
            
            if (ctx != null && string.IsNullOrEmpty(key) == false)
            {
                var helpText = ctx[key];
                BuildHelpInfo(d as UIElement, helpText);
            }
        }

        private static void OnHelpTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UpdateHelpText(d);
        }

        private static void BuildHelpInfo(UIElement d, string helpText)
        {
            if (d == null) return;
            if (string.IsNullOrEmpty(helpText)) return;

            var hiControl = new HelpInfoControl
            {
                Content = helpText
            };

            var popup = new Popup
            {
                AllowsTransparency = true,
                Child = hiControl,
                PlacementTarget = (UIElement)d,
                Placement = PlacementMode.Bottom
            };

            d.MouseLeave += (sender, args) => { popup.IsOpen = false; };
            d.MouseEnter += (sender, args) => { popup.IsOpen = true; };
        }
    }
}
