using Helpm8.Wpf.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Helpm8.Wpf
{
    public class HelpInfo : DependencyObject
    {
        public static string GetHelpm8Text(DependencyObject obj)
        {
            return (string)obj.GetValue(Helpm8TextProperty);
        }

        public static void SetHelpm8Text(DependencyObject obj, string value)
        {
            obj.SetValue(Helpm8TextProperty, value);
        }

        // Using a DependencyProperty as the backing store for Helpm8Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Helpm8TextProperty =
            DependencyProperty.RegisterAttached("Helpm8Text", typeof(string), typeof(HelpInfo),
                new PropertyMetadata(null, OnHelpTextChanged));

        public static object GetHelpContext(DependencyObject obj)
        {
            return obj.GetValue(HelpContextProperty);
        }

        public static void SetHelpContext(DependencyObject obj, object value)
        {
            obj.SetValue(HelpContextProperty, value);
        }

        // Using a DependencyProperty as the backing store for HelpContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HelpContextProperty =
            DependencyProperty.RegisterAttached("HelpContext", typeof(object), typeof(HelpInfo),
                new FrameworkPropertyMetadata
                {
                    Inherits = true,
                    IsNotDataBindable = false,
                    DefaultValue = null,
                    PropertyChangedCallback = OnHelpContextChanged
                });


        private static void OnHelpContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var x = e.NewValue;
            Debug.WriteLine($"HelpContext changed on: {d.GetType().Name}");

            UpdateHelpText(d);
        }

        private static void UpdateHelpText(DependencyObject d)
        {
            var ctx = d.GetValue(HelpContextProperty) as IHelp;
            var key = d.GetValue(Helpm8TextProperty) as string;
            
            if (ctx != null && string.IsNullOrEmpty(key) == false)
            {
                var helpText = ctx[key];
                BuildHelpInfo(d as UIElement, helpText);
            }
        }

        private static void OnHelpTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var x = e.NewValue;
            Debug.WriteLine($"HelpText changed on: {d.GetType().Name}");

            UpdateHelpText(d);
        }

        private static void BuildHelpInfo(UIElement d, string helpText)
        {
            if (d == null) return;
            if (string.IsNullOrEmpty(helpText)) return;

            var hiControl = new HelpInfoControl();
            hiControl.Content = helpText;

            var popup = new Popup();
            popup.AllowsTransparency = true;
            popup.Child = hiControl;
            popup.PlacementTarget = (UIElement)d;
            popup.Placement = PlacementMode.Bottom;

            d.MouseLeave += (sender, args) => { popup.IsOpen = false; };

            d.MouseEnter += (sender, args) => { popup.IsOpen = true; };
        }
    }
}
