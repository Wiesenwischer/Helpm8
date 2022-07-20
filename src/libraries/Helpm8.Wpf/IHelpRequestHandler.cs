using System.Threading.Tasks;
using System.Windows;

namespace Helpm8.Wpf
{
    public interface IHelpRequestHandler
    {
        Task RequestHelpFor(UIElement target);
        Task CloseHelp();
    }
}