using System.Collections.Generic;

namespace Helpm8
{
    /// <summary>
    /// Concept of a builder to build help infos for an application.
    /// </summary>
    public interface IHelpBuilder
    {
        /// <summary>
        /// Adds a new source to retrieve help infos from.
        /// </summary>
        /// <param name="source">The source to add.</param>
        /// <returns>The same <see cref="IHelpBuilder"/> to provide a fluent api for adding sources.</returns>
        IHelpBuilder Add(IHelpSource source);

        /// <summary>
        /// Gets the sources used to retrieve help info values.
        /// </summary>
        IList<IHelpSource> Sources { get; }

        /// <summary>
        /// Builds an <see cref="IHelp"/> with keys and values from the set of sources registered in <see cref="Sources"/>.
        /// </summary>
        /// <returns>An <see cref="IHelpRoot"/> with keys ans values from the registered sources.</returns>
        IHelpRoot Build();
    }
}
