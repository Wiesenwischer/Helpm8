using System.Windows;

namespace Helpm8.Wpf
{
    public interface IProvideHelpKey
    {
        string GetHelpKeyFor(UIElement element);
    }
}