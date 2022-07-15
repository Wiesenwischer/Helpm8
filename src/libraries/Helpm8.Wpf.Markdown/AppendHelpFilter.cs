using System;
using GreenPipes;
using System.Threading.Tasks;

namespace Helpm8.Wpf.Markdown
{
    public class AppendHelpFilter<TSource> 
        : IFilter<RequestHelpContext>
        where TSource: class, IProvideAdditionalHelpContext
    {
        public Task Send(RequestHelpContext context, IPipe<RequestHelpContext> next)
        {
            if (context.TryGetPayload<TSource>(out TSource additionalContentProvider))
            {
                string helpKey = context.HelpKey;
                if (!string.IsNullOrEmpty(helpKey))
                {
                    context.HelpText += additionalContentProvider.GetAdditionalHelpText(helpKey);
                }
            }

            return next.Send(context);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope(nameof(AppendHelpFilter<TSource>));
        }
    }

    public class ProvideAdditionalHelpFilter
        : IFilter<RequestHelpContext>
    {
        private readonly HelpProvider _helpProvider;

        public ProvideAdditionalHelpFilter(IHelp help)
        {
            if (help == null) throw new ArgumentNullException(nameof(help));

            _helpProvider = new HelpProvider(help);
        }

        public Task Send(RequestHelpContext context, IPipe<RequestHelpContext> next)
        {
            context.AddOrUpdatePayload(() => _helpProvider, (existing) => _helpProvider);

            return next.Send(context);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope(nameof(ProvideAdditionalHelpFilter));
        }

        private class HelpProvider : IProvideAdditionalHelpContext
        {
            private readonly IHelp _help;

            public HelpProvider(IHelp help)
            {
                _help = help;
            }

            public string GetAdditionalHelpText(string helpKey)
            {
                return _help[helpKey];
            }
        }
    }

    public interface IProvideAdditionalHelpContext
    {
        string GetAdditionalHelpText(string helpKey);
    }
}