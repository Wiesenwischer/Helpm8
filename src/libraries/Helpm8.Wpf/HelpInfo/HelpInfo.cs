using System;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Helpm8.Wpf.Models;
using Helpm8.Wpf.Controls;


namespace Helpm8.Wpf
{
    public class HelpInfo
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
            DependencyProperty.RegisterAttached("Helpm8Text", typeof(string), typeof(HelpInfo), new PropertyMetadata("Kein Value", OnHelpTextChanged));

        private static void OnHelpTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            if (d is UIElement uiElement)
            {
                var textblock = new TextBlock();
                textblock.Text = e.NewValue as string;
                textblock.Background = Brushes.White;
                textblock.Foreground = Brushes.White;

                var helpInformation = new HelpInformation()
                {
                    Header = "Bin dein Header",
                    Content = e.NewValue
                };


                var hiControl = new HelpInfoControl();



                var popup = new Popup();
                popup.AllowsTransparency = true;
                popup.Child = hiControl;
                (popup.Child as FrameworkElement).DataContext = helpInformation;
                popup.PlacementTarget = (UIElement)d;
                popup.StaysOpen = false;
                

                uiElement.MouseEnter += (sender, args) =>
                { 
                    popup.IsOpen = true;
                };




            }
        }



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
            DependencyProperty.RegisterAttached("HelpKey", typeof(string), typeof(HelpInfo), new PropertyMetadata(string.Empty));
    }
}
