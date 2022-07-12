using System;
using System.Collections;
using System.Collections.Generic;
using Helpm8.InMemory;

namespace Helpm8
{
    public sealed class HelpManager : IHelpBuilder, IHelpRoot, IDisposable
    {
        private readonly HelpSources _sources;
        private readonly ReferenceCountedProviderManager _providerManager = new ReferenceCountedProviderManager();

        public HelpManager()
        {
            _sources = new HelpSources(this) 
            {
                //Make sure there's some default storage since there are no default providers.
                new InMemoryHelpSource()
            };
        }

        /// <inheritdoc/>
        public string? this[string key]
        {
            get
            {
                using ReferenceCountedProviders reference = _providerManager.GetReference();
                return HelpRoot.GetHelp(reference.Providers, key);
            }
            set
            {
                using ReferenceCountedProviders reference = _providerManager.GetReference();
                HelpRoot.SetHelp(reference.Providers, key, value);
            }
        }

        /// <inheritdoc/>
        public IList<IHelpSource> Sources => _sources;

        public IEnumerable<IHelpProvider> Providers { get; } = new List<IHelpProvider>();

        public IHelpSection GetSection(string key) => new HelpSection(this, key);

        public IEnumerable<IHelpSection> GetChildren() => this.GetChildrenImplementation(null);

        public void Dispose()
        {
            _providerManager.Dispose();
        }

        IHelpBuilder IHelpBuilder.Add(IHelpSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            _sources.Add(source);
            return this;
        }

        IHelpRoot IHelpBuilder.Build() => this;

        void IHelpRoot.Load()
        {
            using ReferenceCountedProviders reference = _providerManager.GetReference();
            foreach (IHelpProvider provider in reference.Providers)
            {
                provider.Load();
            }
        }

        internal ReferenceCountedProviders GetProvidersReference() => _providerManager.GetReference();

        // Don't rebuild and reload all providers in the common case when a source is simply added to the IList
        private void AddSource(IHelpSource source)
        {
            IHelpProvider provider = source.Build(this);

            provider.Load();
            _providerManager.AddProvider(provider);
        }
        
        // Something other than Add was called on IHelpBuilder.Sources
        private void ReloadSources()
        {
            var newProvidersList = new List<IHelpProvider>();

            foreach (IHelpSource source in _sources)
            {
                newProvidersList.Add(source.Build(this));
            }

            foreach (IHelpProvider provider in newProvidersList)
            {
                provider.Load();
            }

            _providerManager.ReplaceProviders(newProvidersList);
        }

        private sealed class HelpSources : IList<IHelpSource>
        {
            private readonly List<IHelpSource> _sources = new List<IHelpSource>();
            private readonly HelpManager _helpManager;

            public HelpSources(HelpManager helpManager)
            {
                _helpManager = helpManager;
            }

            public int Count => _sources.Count;

            public bool IsReadOnly => false;

            public IHelpSource this[int index]
            {
                get => _sources[index];
                set
                {
                    _sources[index] = value;
                    _helpManager.ReloadSources();
                }
            }

            public IEnumerator<IHelpSource> GetEnumerator()
            {
                return _sources.GetEnumerator();
            }

            public int IndexOf(IHelpSource source)
            {
                return _sources.IndexOf(source);
            }

            public void Add(IHelpSource source)
            {
                _sources.Add(source);
                _helpManager.AddSource(source);
            }

            public void Insert(int index, IHelpSource source)
            {
                _sources.Insert(index, source);
                _helpManager.ReloadSources();
            }

            public bool Contains(IHelpSource source)
            {
                return _sources.Contains(source);
            }

            public void CopyTo(IHelpSource[] array, int arrayIndex)
            {
                _sources.CopyTo(array, arrayIndex);
            }

            public bool Remove(IHelpSource source)
            {
                var removed = _sources.Remove(source);
                _helpManager.ReloadSources();
                return removed;
            }

            public void RemoveAt(int index)
            {
                _sources.RemoveAt(index);
                _helpManager.ReloadSources();
            }

            public void Clear()
            {
                _sources.Clear();
                _helpManager.ReloadSources();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
