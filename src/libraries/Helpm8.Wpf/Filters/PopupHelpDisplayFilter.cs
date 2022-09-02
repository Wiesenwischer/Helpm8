using GreenPipes;
using Helpm8.Wpf.Controls;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Helpm8.Wpf
{
    public class PopupHelpDisplayFilter : IFilter<RequestHelpContext>
    {
        private readonly List<IHelpCommandObserver> _observers = new List<IHelpCommandObserver>();
        private bool _canShowHelp = true;
        private readonly ICommand _command;
        private readonly Popup _popup;
        private bool _hasActiveHelpRequest;
        public PopupHelpDisplayFilter(PlacementMode placement, ICommand command)
        {
            _command = command;
            CommandManager.RegisterClassCommandBinding(typeof(FrameworkElement),
                new CommandBinding(command,
                    Executed,
                    CanExecute));
            _popup = new Popup()
            {
                AllowsTransparency = true,
                Placement = placement
            };
        }
        private void CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = true;
        }

        private void Executed(object sender, ExecutedRoutedEventArgs args)
        {
            _canShowHelp = !_canShowHelp;
            if (!_canShowHelp)
            {
                _popup.IsOpen = false;
            }
            else
            {
                _popup.IsOpen = _hasActiveHelpRequest && HasHelpContent();
            }
            _observers.ForEach(x => x.IsHelpActive = _canShowHelp);
        }
        public void Attach(IHelpCommandObserver helpCommandObserver)
        {
            if (!_observers.Contains(helpCommandObserver))
            {
                helpCommandObserver.IsHelpActive = _canShowHelp;
                _observers.Add(helpCommandObserver);
            }
        }

        public void Detach(IHelpCommandObserver helpCommandObserver)
        {
            _observers.Remove(helpCommandObserver);
        }

        public void Probe(ProbeContext context)
        {
            var scope = context.CreateScope(nameof(PopupHelpDisplayFilter));
            scope.Add("Popup.Placement", _popup.Placement);
            scope.Add("Command", _command);
        }

        public Task Send(RequestHelpContext context, IPipe<RequestHelpContext> next)
        {
            _hasActiveHelpRequest = true;

            if (_canShowHelp)
            {
                context.OnClose = CloseHelpPopup;
                ShowHelpPopup(context);
            }

            return next.Send(context);
        }

        private Task CloseHelpPopup()
        {
            _popup.IsOpen = false;
            _hasActiveHelpRequest = false;
            return Task.CompletedTask;
        }

        private void ShowHelpPopup(RequestHelpContext helpContext)
        {
            var hiControl = new HelpInfoControl
            {
                Content = CreatePopupContent(helpContext),
                Placement = Placement.BottomLeft
            };

            _popup.Child = hiControl;
            _popup.PlacementTarget = helpContext.Target;
            _popup.PopupAnimation = PopupAnimation.Slide;
            _popup.IsOpen = HasHelpContent();
        }

        protected virtual object CreatePopupContent(RequestHelpContext helpContext)
        {
            return helpContext.HelpText;
        }

        private bool HasHelpContent()
        {
            var control = (HelpInfoControl)_popup.Child;
            return string.IsNullOrWhiteSpace(control.Content?.ToString()) == false;
        }
    }
}