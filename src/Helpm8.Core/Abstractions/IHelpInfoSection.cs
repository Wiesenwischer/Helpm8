namespace Helpm8
{
    /// <summary>
    /// Concept of a section of help infos.
    /// </summary>
    /// <remarks>
    /// A section of help infos can group related values together, e.g. all help info values to a specific input form or page.
    /// </remarks>
    public interface IHelpInfoSection : IHelpInfo
    {
        /// <summary>
        /// Gets the key this section has in its parent.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Gets the path to this section.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets or sets the section value.
        /// </summary>
        string? Value { get; set; }
    }
}