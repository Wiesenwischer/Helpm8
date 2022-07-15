using System.Windows;

namespace Helpm8.Wpf
{
    public class HelpInfoKeyProvider : IProvideHelpKey
    {
        public string GetHelpKeyFor(UIElement element)
        {
            return HelpInfo.GetHelpKey(element);
        }
    }
}