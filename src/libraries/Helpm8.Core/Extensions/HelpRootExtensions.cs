using System;
using System.Collections.Generic;
using System.Linq;

namespace Helpm8
{
    internal static class HelpRootExtensions
    {
        internal static IEnumerable<IHelpSection> GetChildrenImplementation(this IHelpRoot root, string? path)
        {
            using ReferenceCountedProviders? reference = (root as HelpManager)?.GetProvidersReference();
            IEnumerable<IHelpProvider> providers = reference?.Providers ?? root.Providers;

            IEnumerable<IHelpSection> children = providers
                .Aggregate(Enumerable.Empty<string>(),
                    (seed, source) => source.GetChildKeys(seed, path))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(key => root.GetSection(path == null ? key : HelpPath.Combine(path, key)));

            if (reference is null)
            {
                return children;
            }
            else
            {
                // Eagerly evaluate the IEnumerable before releasing the reference so we don't allow iteration over disposed providers.
                return children.ToList();
            }
        }
    }
}
