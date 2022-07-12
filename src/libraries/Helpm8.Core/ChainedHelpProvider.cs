using System;
using System.Collections.Generic;

namespace Helpm8
{
    /// <summary>
    /// Chained implementation of <see cref="IHelpProvider"/>
    /// </summary>
    public class ChainedHelpProvider : IHelpProvider, IDisposable
    {
        private readonly IHelp _help;
        private readonly bool _shouldDisposeHelp;

        /// <summary>
        /// Initialize a new instance from the source help.
        /// </summary>
        /// <param name="source">The source help.</param>
        public ChainedHelpProvider(ChainedHelpSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            _help = source.Help ?? throw new ArgumentException("InvalidNullArgument: source.Help", nameof(source));
            _shouldDisposeHelp = source.ShouldDisposeHelp;
        }

        /// <summary>
        /// Gets the chained help.
        /// </summary>
        public IHelp Help => _help;

        /// <summary>
        /// Tries to get a help value for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>True</c> if a value for the specified key was found, otherwise <c>false</c>.</returns>
        public bool TryGet(string key, out string? value)
        {
            value = _help[key];
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Sets a help value for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Set(string key, string? value) => _help[key] = value;

        /// <summary>
        /// Loads help values from the source represented by this <see cref="IHelpProvider"/>.
        /// </summary>
        public void Load() { }

        /// <summary>
        /// Returns the immediate descendant help keys for a given parent path based on this
        /// <see cref="IHelpProvider"/>s data and the set of keys returned by all the preceding
        /// <see cref="IHelpProvider"/>s.
        /// </summary>
        /// <param name="earlierKeys">The child keys returned by the preceding providers for the same parent path.</param>
        /// <param name="parentPath">The parent path.</param>
        /// <returns>The child keys.</returns>
        public IEnumerable<string> GetChildKeys(
            IEnumerable<string> earlierKeys,
            string? parentPath)
        {
            IHelp section = parentPath == null ? _help : _help.GetSection(parentPath);
            var keys = new List<string>();
            foreach (IHelpSection child in section.GetChildren())
            {
                keys.Add(child.Key);
            }
            keys.AddRange(earlierKeys);
            keys.Sort(HelpKeyComparer.Comparison);
            return keys;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_shouldDisposeHelp)
            {
                (_help as IDisposable)?.Dispose();
            }
        }
    }
}