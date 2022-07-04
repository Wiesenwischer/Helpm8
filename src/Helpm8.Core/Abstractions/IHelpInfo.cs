using System.Collections.Generic;

namespace Helpm8
{
    /// <summary>
    /// Concept of a help info as a set of key/value pairs.
    /// </summary>
    public interface IHelpInfo
    {
        /// <summary>
        /// Gets or sets a help info value.
        /// </summary>
        /// <param name="key">The help info key.</param>
        /// <returns>The help info value.</returns>
        string? this[string key] { get; set; }

        /// <summary>
        /// Gets a section of help infos with the given key.
        /// </summary>
        /// <param name="key">Th key of the help info section.</param>
        /// <returns>The <see cref="IHelpInfoSection"/>.</returns>
        /// <remarks>
        /// This method will never return <c>null</c>. If no section is found with the given key, an empty <see cref="IHelpInfoSection"/> will be returned.
        /// </remarks>
        IHelpInfoSection GetSection(string key);

        /// <summary>
        /// Gets the child sections.
        /// </summary>
        /// <returns>The sub-sections of this help info.</returns>
        IEnumerable<IHelpInfoSection> GetChildren();
    }
}
