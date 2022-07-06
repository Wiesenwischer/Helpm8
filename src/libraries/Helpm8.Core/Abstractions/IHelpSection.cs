namespace Helpm8
{
    /// <summary>
    /// Concept of a section of help values.
    /// </summary>
    /// <remarks>
    /// A section of help values can group related values together, e.g. all help values to a specific input form or page.
    /// </remarks>
    public interface IHelpSection : IHelp
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