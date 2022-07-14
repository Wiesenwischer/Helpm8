using Helpm8.Wpf.Controls;
using System;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Helpm8.Wpf
{
    public class HelpInfo : DependencyObject
    {
        private static bool _canShowHelp = true;


        static HelpInfo()
        {
            CommandManager.RegisterClassCommandBinding(typeof(FrameworkElement),
                new CommandBinding(CustomCommands.Guidance,
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

            _popup.IsOpen = _canShowHelp;
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

        private static void OnHelpKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement uiElement)
            {
                HelpTargetAvailable?.Invoke(null, new HelpTargetEventArgs(uiElement));
            }


            UpdateHelpText(d);
        }


        private static readonly ConcurrentDictionary<UIElement, Popup> _popupCache = new ConcurrentDictionary<UIElement, Popup>();

        private static void BuildHelpInfo(UIElement d, string helpText)
        {
            if (d == null) return;
            if (string.IsNullOrEmpty(helpText)) return;

            var hiControl = new HelpInfoControl
            {
                Content = Configurator.ContentProvider.GetContentFor(helpText)
            };

            d.LostFocus += (sender, args) => { _popup.IsOpen = false; };
            d.GotFocus += (sender, args) =>
            {
                _popup.IsOpen = false;
                _popup.Child = hiControl;
                _popup.PlacementTarget = d;
                _popup.PopupAnimation = PopupAnimation.Slide;
                _popup.IsOpen = _canShowHelp;
            };
        }

        private static Popup _popup = new Popup
        {
            AllowsTransparency = true,
            Placement = PlacementMode.Bottom
        };
        
        public static void Configure(Action<IConfigureHelpInfo> config)
        {
            config(Configurator);
        }

        public static IConfigureHelpInfo Configure()
        {
            return Configurator;
        }

        public static IConfigureHelpInfo Configurator { get; } = new HelpInfoConfigurator();

        public static event EventHandler<HelpTargetEventArgs> HelpTargetAvailable;

    }

    public class HelpTargetEventArgs : EventArgs
    {
        public HelpTargetEventArgs(UIElement target)
        {
            Target = target;
        }

        public UIElement Target { get; }
    }

    public class HelpInfoConfigurator : IConfigureHelpInfo
    {
        public IContentProvider ContentProvider { get; set; } = new TextContentProvider();
    }

    public interface IConfigureHelpInfo
    {
        IContentProvider ContentProvider { get; set; }
    }

    public interface IContentProvider
    {
        object GetContentFor(string helpText);
    }

    public class TextContentProvider : IContentProvider
    {
        public object GetContentFor(string helpText)
        {
            return helpText;
        }
    }
}
