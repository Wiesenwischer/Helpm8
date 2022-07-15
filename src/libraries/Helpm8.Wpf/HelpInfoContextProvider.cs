using System.Windows;

namespace Helpm8.Wpf
{
    public class HelpInfoContextProvider : IProvideHelpContext
    {
        public IHelp GetHelpContextFor(UIElement element)
        {
            return HelpInfo.GetHelpContext(element);
        }
    }
}