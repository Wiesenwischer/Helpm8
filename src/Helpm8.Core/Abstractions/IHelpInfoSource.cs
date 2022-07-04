namespace Helpm8
{
    /// <summary>
    /// Concept of a source of help info key/values for an application.
    /// </summary>
    public interface IHelpInfoSource
    {
        /// <summary>
        /// Builds the <see cref="IHelpInfoProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IHelpInfoBuilder"/>.</param>
        /// <returns>An <see cref="IHelpInfoProvider"/>.</returns>
        IHelpInfoProvider Build(IHelpInfoBuilder builder);
    }
}