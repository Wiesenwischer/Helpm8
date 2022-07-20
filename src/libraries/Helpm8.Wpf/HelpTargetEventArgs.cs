using System;
using System.Windows;

namespace Helpm8.Wpf
{
    public class HelpTargetEventArgs : EventArgs
    {
        public HelpTargetEventArgs(UIElement target)
        {
            Target = target;
        }

        public UIElement Target { get; }
    }
}