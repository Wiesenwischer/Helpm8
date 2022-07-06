using System.Collections.Generic;

namespace Helpm8
{
    /// <summary>
    /// Concept of a help as a set of key/value pairs.
    /// </summary>
    public interface IHelp
    {
        /// <summary>
        /// Gets or sets a help value.
        /// </summary>
        /// <param name="key">The help key.</param>
        /// <returns>The help value.</returns>
        string? this[string key] { get; set; }

        /// <summary>
        /// Gets a section of help values with the given key.
        /// </summary>
        /// <param name="key">The key of the help section.</param>
        /// <returns>The <see cref="IHelpSection"/>.</returns>
        /// <remarks>
        /// This method will never return <c>null</c>. If no section is found with the given key, an empty <see cref="IHelpSection"/> will be returned.
        /// </remarks>
        IHelpSection GetSection(string key);

        /// <summary>
        /// Gets the child sections.
        /// </summary>
        /// <returns>The sub-sections of this help.</returns>
        IEnumerable<IHelpSection> GetChildren();
    }
}
