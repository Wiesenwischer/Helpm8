using System.Collections.Generic;

namespace Helpm8
{
    /// <summary>
    /// Concept of a builder to build help infos for an application.
    /// </summary>
    public interface IHelpInfoBuilder
    {
        /// <summary>
        /// Adds a new source to retrieve help infos from.
        /// </summary>
        /// <param name="source">The source to add.</param>
        /// <returns>The same <see cref="IHelpInfoBuilder"/> to provide a fluent api for adding sources.</returns>
        IHelpInfoBuilder Add(IHelpInfoSource source);

        /// <summary>
        /// Gets the sources used to retrieve help info values.
        /// </summary>
        IList<IHelpInfoSource> Sources { get; }

        /// <summary>
        /// Builds an <see cref="IHelpInfo"/> with keys and values from the set of sources registered in <see cref="Sources"/>.
        /// </summary>
        /// <returns>An <see cref="IHelpInfoRoot"/> with keys ans values from the registered sources.</returns>
        IHelpInfoRoot Build();
    }
}
