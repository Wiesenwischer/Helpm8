using System;
using System.Windows;
using System.Windows.Input;

namespace Helpm8.Wpf
{
    public class HelpInfoObserver
    {
        private readonly IHelpRequestHandler _requestHandler;

        public HelpInfoObserver(IHelpRequestHandler requestHandler)
        {
            _requestHandler = requestHandler ?? throw new ArgumentNullException(nameof(requestHandler));
        }

        public void StartObserving()
        {
            HelpInfo.HelpTargetAvailable += OnHelpTargetAvailable;
        }

        private void OnHelpTargetAvailable(object sender, HelpTargetEventArgs e)
        {
            e.Target.MouseEnter += TargetOnMouseEnter;
            e.Target.MouseLeave += TargetOnMouseLeave;
        }

        private void TargetOnMouseLeave(object sender, MouseEventArgs e)
        {
            _requestHandler.CloseHelp();
        }

        private void TargetOnMouseEnter(object sender, MouseEventArgs e)
        {
            _requestHandler.RequestHelpFor((UIElement) sender);
        }

    }
}