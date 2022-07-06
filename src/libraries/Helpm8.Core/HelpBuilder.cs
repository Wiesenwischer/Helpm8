using System;
using System.Collections.Generic;

namespace Helpm8
{
    public class HelpBuilder : IHelpBuilder
    {
        public IHelpBuilder Add(IHelpSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            Sources.Add(source);
            return this;
        }

        public IList<IHelpSource> Sources { get; } = new List<IHelpSource>();

        public IHelpRoot Build()
        {
            var providers = new List<IHelpProvider>();
            foreach (var source in Sources)
            {
                IHelpProvider provider = source.Build(this);
                providers.Add(provider);
            }

            return new HelpRoot(providers);
        }
    }
}
