using GreenPipes;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Helpm8.Wpf
{
    public class HelpRequestHandler : IHelpRequestHandler
    {
        private readonly IProvideHelpKey _helpKeyProvider;
        private readonly IProvideHelpContext _helpContextProvider;
        private readonly IPipe<RequestHelpContext> _pipe;

        public HelpRequestHandler(IProvideHelpKey helpKeyProvider, IProvideHelpContext helpContextProvider, IPipe<RequestHelpContext> pipe)
        {
            _helpKeyProvider = helpKeyProvider ?? throw new ArgumentNullException(nameof(helpKeyProvider));
            _helpContextProvider = helpContextProvider ?? throw new ArgumentNullException(nameof(helpContextProvider));
            _pipe = pipe ?? throw new ArgumentNullException(nameof(pipe));
        }

        public async Task RequestHelpFor(UIElement target)
        {
            var key = _helpKeyProvider.GetHelpKeyFor(target);
            if (key == null) return;

            var help = _helpContextProvider.GetHelpContextFor(target);
            if (help == null) return;
            
            var helpText = help[key];
            var context = new RequestHelpContext()
            {
                HelpContext = help,
                HelpText = helpText,
                Target = target,
                HelpKey = key
            };
            await _pipe.Send(context);
            _onClose = context.OnClose;
        }

        private Func<Task> _onClose;

        public Task CloseHelp()
        {
            return _onClose?.Invoke();
        }
    }
}