using Helpm8.InMemory;
using System;
using System.Collections.Generic;

namespace Helpm8
{
    /// <summary>
    /// IHelpBuilder extension methods for the InMemoryHelpProvider.
    /// </summary>
    public static class InMemoryHelpBuilderExtensions
    {
        /// <summary>
        /// Adds the in-memory help provider to <paramref name="helpBuilder"/>.
        /// </summary>
        /// <param name="helpBuilder">The <see cref="IHelpBuilder"/> to add to.</param>
        /// <returns>The <see cref="IHelpBuilder"/>.</returns>
        public static IHelpBuilder AddInMemoryCollection(this IHelpBuilder helpBuilder)
        {
            if (helpBuilder == null) throw new ArgumentNullException(nameof(helpBuilder));

            helpBuilder.Add(new InMemoryHelpSource());
            return helpBuilder;
        }

        /// <summary>
        /// Adds the in-memory help provider to <paramref name="helpBuilder"/>.
        /// </summary>
        /// <param name="helpBuilder">The <see cref="IHelpBuilder"/> to add to.</param>
        /// <param name="initialData">The data to add to in-memory help provider.</param>
        /// <returns>The <see cref="IHelpBuilder"/>.</returns>
        public static IHelpBuilder AddInMemoryCollection(
            this IHelpBuilder helpBuilder,
            IEnumerable<KeyValuePair<string, string?>>? initialData)
        {
            if (helpBuilder == null) throw new ArgumentNullException(nameof(helpBuilder));

            helpBuilder.Add(new InMemoryHelpSource { InitialData = initialData });
            return helpBuilder;
        }
    }
}
