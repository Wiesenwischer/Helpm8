using System;
using System.Collections.Generic;

namespace Helpm8
{
    public class HelpRoot : IHelpRoot, IDisposable
    {
        private readonly IList<IHelpProvider> _providers;

        public HelpRoot(IList<IHelpProvider> providers)
        {
            _providers = providers ?? throw new ArgumentNullException(nameof(providers));
            Load();
        }

        public string? this[string key]
        {
            get => GetConfiguration(_providers, key);
            set => SetConfiguration(_providers, key, value);
        }

        public IEnumerable<IHelpProvider> Providers => _providers;

        public IHelpSection GetSection(string key) => new HelpSection(this, key);

        public IEnumerable<IHelpSection> GetChildren() => this.GetChildrenImplementation(null);

        public void Load()
        {
            foreach (IHelpProvider provider in _providers)
            {
                provider.Load();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (IHelpProvider provider in _providers)
            {
                (provider as IDisposable)?.Dispose();
            }
        }

        internal static string? GetConfiguration(IList<IHelpProvider> providers, string key)
        {
            for (int i = providers.Count - 1; i >= 0; i--)
            {
                IHelpProvider provider = providers[i];

                if (provider.TryGet(key, out string? value))
                {
                    return value;
                }
            }

            return null;
        }

        internal static void SetConfiguration(IList<IHelpProvider> providers, string key, string? value)
        {
            if (providers.Count == 0)
            {
                throw new InvalidOperationException("A help source is not registered. Please register one before setting a value.");
            }

            foreach (IHelpProvider provider in providers)
            {
                provider.Set(key, value);
            }
        }
    }
}
