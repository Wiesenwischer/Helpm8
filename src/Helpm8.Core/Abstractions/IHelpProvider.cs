using System.Collections.Generic;

namespace Helpm8
{
    /// <summary>
    /// Provides help info key/values for an application.
    /// </summary>
    public interface IHelpProvider
    {
        /// <summary>
        /// Loads help info values from the source represented by this <see cref="IHelpProvider"/>.
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

        /// <summary>
        /// Returns the immediate descendant help keys for a given parent path based on this
        /// <see cref="IHelpProvider"/>s data and the set of keys returned by all the preceding
        /// <see cref="IHelpProvider"/>s.
        /// </summary>
        /// <param name="earlierKeys">The child keys returned by the preceding providers for the same parent path.</param>
        /// <param name="parentPath">The parent path.</param>
        /// <returns>The child keys.</returns>
        IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath);
    }
}
