namespace Helpm8
{
    /// <summary>
    /// Concept of a source of help info key/values for an application.
    /// </summary>
    public interface IHelpSource
    {
        /// <summary>
        /// Builds the <see cref="IHelpProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IHelpBuilder"/>.</param>
        /// <returns>An <see cref="IHelpProvider"/>.</returns>
        IHelpProvider Build(IHelpBuilder builder);
    }
}