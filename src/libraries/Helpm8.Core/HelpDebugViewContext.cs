using JetBrains.Annotations;

namespace Helpm8
{
    /// <summary>
    /// Provides the data about current item of the help.
    /// </summary>
    public readonly struct HelpDebugViewContext
    {
        public HelpDebugViewContext(string path, string key, string? value,
            IHelpProvider helpProvider)
        {
            Path = path;
            Key = key;
            Value = value;
            HelpProvider = helpProvider;
        }

        /// <summary>
        /// Gets the path of the current item.
        /// </summary>
        [PublicAPI]
        public string Path { get; }

        /// <summary>
        /// Gets the key of the current item.
        /// </summary>
        [PublicAPI]
        public string Key { get; }

        /// <summary>
        /// Gets the value of the current item.
        /// </summary>
        [PublicAPI]
        public string? Value { get; }

        /// <summary>
        /// Gets the <see cref="IHelpProvider" /> that was used to get the value of the current item.
        /// </summary>
        [PublicAPI]
        public IHelpProvider HelpProvider { get; }
    }
}