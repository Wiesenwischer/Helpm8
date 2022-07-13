using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Helpm8.Wpf.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Helpm8.Wpf
{
    public class CustomCommands
    {
        static CustomCommands()
        {
            var gestures = new InputGestureCollection()
            {
                new KeyGesture(Key.F2)
            };
            CustomHelp = new RoutedUICommand("", "CustomHelp", typeof(CustomCommands), gestures);
        }

        public static RoutedUICommand CustomHelp { get; }
    }

    public class HelpInfo : DependencyObject
    {
        private static bool _canShowHelp = true;

        static HelpInfo()
        {
            CommandManager.RegisterClassCommandBinding(typeof(FrameworkElement),
                new CommandBinding(CustomCommands.CustomHelp,
                    Executed,
                    CanExecute));
        }

        private static void CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = true;
            SetIsHelpActive(args.OriginalSource as DependencyObject, _canShowHelp);
        }

        private static void Executed(object sender, ExecutedRoutedEventArgs args)
        {
            _canShowHelp = !_canShowHelp;
            SetIsHelpActive(args.OriginalSource as DependencyObject, _canShowHelp);

            foreach (var cachedItem in _popupCache)
            {
                cachedItem.Value.IsOpen = false;
                if (_canShowHelp && cachedItem.Key.IsMouseOver)
                {
                    cachedItem.Value.IsOpen = true;
                }
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

        public static readonly DependencyProperty IsHelpActiveProperty = DependencyProperty.RegisterAttached(
            "IsHelpActive", typeof(bool), typeof(HelpInfo),
            new FrameworkPropertyMetadata
            {
                Inherits = true,
                IsNotDataBindable = false,
                DefaultValue = true
            });

        public static void SetIsHelpActive(DependencyObject element, bool value)
        {
            element.SetValue(IsHelpActiveProperty, value);
        }

        public static bool GetIsHelpActive(DependencyObject element)
        {
            return (bool)element.GetValue(IsHelpActiveProperty);
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

        private static readonly ConcurrentDictionary<UIElement, Popup> _popupCache = new ConcurrentDictionary<UIElement, Popup>();

        private static void BuildHelpInfo(UIElement d, string helpText)
        {
            if (d == null) return;
            if (string.IsNullOrEmpty(helpText)) return;

            var hiControl = new HelpInfoControl
            {
                Content = helpText
            };

            var po = _popupCache.GetOrAdd(d, (target) =>
             {
                 var popup = new Popup
                 {
                     AllowsTransparency = true,
                     //Child = hiControl,
                     PlacementTarget = target,
                     Placement = PlacementMode.Bottom
                 };

                 d.MouseLeave += (sender, args) => { popup.IsOpen = false; };
                 d.MouseEnter += (sender, args) => { popup.IsOpen = _canShowHelp; };
                 return popup;
             });

            po.Child = hiControl;
        }
    }
}
