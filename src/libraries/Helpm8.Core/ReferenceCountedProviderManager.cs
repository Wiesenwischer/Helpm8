using System;
using System.Collections.Generic;

namespace Helpm8
{
    internal sealed class ReferenceCountedProviderManager : IDisposable
    {
        private readonly object _replaceProvidersLock = new object();
        private ReferenceCountedProviders _refCountedProviders = ReferenceCountedProviders.Create(new List<IHelpProvider>());
        private bool _disposed;

        public IEnumerable<IHelpProvider> NonReferenceCountedProviders => _refCountedProviders.NonReferenceCountedProviders;

        public ReferenceCountedProviders GetReference()
        {
            // Lock to ensure oldRefCountedProviders.Dispose() in ReplaceProviders() or Dispose() doesn't decrement ref count to zero
            // before calling _refCountedProviders.AddReference().
            lock (_replaceProvidersLock)
            {
                if (_disposed)
                {
                    // Return a non-reference-counting ReferenceCountedProviders instance now that the ConfigurationManager is disposed.
                    // We could preemptively throw an ODE instead, but this might break existing apps that were previously able to
                    // continue to read configuration after disposing an ConfigurationManager.
                    return ReferenceCountedProviders.CreateDisposed(_refCountedProviders.NonReferenceCountedProviders);
                }

                _refCountedProviders.AddReference();
                return _refCountedProviders;
            }
        }

        public void AddProvider(IHelpProvider provider)
        {
            lock (_replaceProvidersLock)
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(HelpManager));
                }

                // Maintain existing references, but replace list with copy containing new item.
                _refCountedProviders.Providers = new List<IHelpProvider>(_refCountedProviders.Providers)
                {
                    provider
                };
            }
        }

        // Providers should never be concurrently modified. Reading during modification is allowed.
        public void ReplaceProviders(List<IHelpProvider> providers)
        {
            ReferenceCountedProviders oldRefCountedProviders = _refCountedProviders;

            lock (_replaceProvidersLock)
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(HelpManager));
                }

                _refCountedProviders = ReferenceCountedProviders.Create(providers);
            }

            // Decrement the reference count to the old providers. If they are being concurrently read from
            // the actual disposal of the old providers will be delayed until the final reference is released.
            // Never dispose ReferenceCountedProviders with a lock because this may call into user code.
            oldRefCountedProviders.Dispose();
        }

        public void Dispose()
        {
            ReferenceCountedProviders oldRefCountedProviders = _refCountedProviders;

            // This lock ensures that we cannot reduce the ref count to zero before GetReference() calls AddReference().
            // Once _disposed is set, GetReference() stops reference counting.
            lock (_replaceProvidersLock)
            {
                _disposed = true;
            }

            // Never dispose ReferenceCountedProviders with a lock because this may call into user code.
            oldRefCountedProviders.Dispose();
        }
    }
}
