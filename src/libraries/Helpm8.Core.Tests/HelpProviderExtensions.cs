using System;
using System.Diagnostics.CodeAnalysis;

namespace Helpm8.Core.Tests
{
    [ExcludeFromCodeCoverage]
    public static class HelpProviderExtensions
    {
        public static string Get(this IHelpProvider provider, string key)
        {
            if (!provider.TryGet(key, out string value))
            {
                throw new InvalidOperationException("Key not found");
            }

            return value;
        }
    }
}