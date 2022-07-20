using System.Windows;

namespace Helpm8.Wpf
{
    public interface IProvideHelpContext
    {
        IHelp GetHelpContextFor(UIElement element);
    }
}