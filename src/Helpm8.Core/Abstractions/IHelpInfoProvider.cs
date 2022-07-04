namespace Helpm8
{
    /// <summary>
    /// Provides help info key/values for an application.
    /// </summary>
    public interface IHelpInfoProvider
    {
        /// <summary>
        /// Loads help info values from the source represented by this <see cref="IHelpInfoProvider"/>.
        /// </summary>
        void Load();

        /// <summary>
        /// Tries to get a help info value for the specified key.
        /// </summary>
        /// <param name="key">The key of the help info.</param>
        /// <param name="value">The value of the help info.</param>
        /// <returns><c>True</c> if a value for the given key was found, otherwise <c>false</c>.</returns>
        bool TryGet(string key, out string? value);

        /// <summary>
        /// Sets a help info value for the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Set(string key, string? value);
    }
}
