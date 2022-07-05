using System;
using System.Collections.Generic;

namespace Helpm8
{
    public sealed class HelpManager : IHelpBuilder, IHelpRoot, IDisposable
    {
        public IHelpBuilder Add(IHelpSource source)
        {
            throw new NotImplementedException();
        }

        public IList<IHelpSource> Sources { get; }
        public IHelpRoot Build()
        {
            throw new NotImplementedException();
        }

        public string? this[string key]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public IHelpSection GetSection(string key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IHelpSection> GetChildren()
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IHelpProvider> Providers { get; }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        internal ReferenceCountedProviders GetProvidersReference() => throw new NotImplementedException();
    }
}
