using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helpm8
{
    /// <summary>
    /// Extension methods for <see cref="IHelpRoot"/>.
    /// </summary>
    public static class HelpRootExtensions
    {
        /// <summary>
        /// Generates a human-readable view of the help showing where each value came from.
        /// </summary>
        /// <returns> The debug view. </returns>
        public static string GetDebugView(this IHelpRoot root)
        {
            return GetDebugView(root, processValue: null);
        }

        /// <summary>
        /// Generates a human-readable view of the help showing where each value came from.
        /// </summary>
        /// <param name="root">Help root</param>
        /// <param name="processValue">
        /// Function for processing the value e.g. hiding secrets
        /// Parameters:
        ///   HelpDebugViewContext: Context of the current help item
        ///   returns: A string value is used to assign as the Value of the help section
        /// </param>
        /// <returns> The debug view. </returns>
        public static string GetDebugView(this IHelpRoot root, Func<HelpDebugViewContext, string>? processValue)
        {
            void RecurseChildren(
                StringBuilder stringBuilder,
                IEnumerable<IHelpSection> children,
                string indent)
            {
                foreach (IHelpSection child in children)
                {
                    (string? Value, IHelpProvider? Provider) valueAndProvider = GetValueAndProvider(root, child.Path);

                    if (valueAndProvider.Provider != null)
                    {
                        string? value = processValue != null
                            ? processValue(new HelpDebugViewContext(child.Key, child.Path, valueAndProvider.Value,
                                valueAndProvider.Provider))
                            : valueAndProvider.Value;

                        stringBuilder
                            .Append(indent)
                            .Append(child.Key)
                            .Append('=')
                            .Append(value)
                            .Append(" (")
                            .Append(valueAndProvider.Provider)
                            .AppendLine(")");
                    }
                    else
                    {
                        stringBuilder
                            .Append(indent)
                            .Append(child.Key)
                            .AppendLine(":");
                    }

                    RecurseChildren(stringBuilder, child.GetChildren(), indent + "  ");
                }
            }

            var builder = new StringBuilder();

            RecurseChildren(builder, root.GetChildren(), "");

            return builder.ToString();
        }

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

        private static (string? Value, IHelpProvider? Provider) GetValueAndProvider(
            IHelpRoot root,
            string key)
        {
            foreach (IHelpProvider provider in root.Providers.Reverse())
            {
                if (provider.TryGet(key, out string? value))
                {
                    return (value, provider);
                }
            }

            return (null, null);
        }
    }
}