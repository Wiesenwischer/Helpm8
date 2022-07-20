using GreenPipes;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Helpm8.Wpf
{
    public class RequestHelpContext : BasePipeContext, PipeContext
    {
        public IHelp HelpContext { get; set; }
        public string HelpKey { get; set; }
        public UIElement Target { get; set; }
        public string HelpText { get; set; }
        public Func<Task> OnClose { get; set; }
    }
}
