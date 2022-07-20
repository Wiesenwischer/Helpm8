using System.Windows.Input;

namespace Helpm8.Wpf
{
    public class CustomCommands
    {
        static CustomCommands()
        {
            Guidance = new RoutedUICommand("", nameof(Guidance), typeof(CustomCommands));
        }

        public static RoutedUICommand Guidance { get; }
    }
}