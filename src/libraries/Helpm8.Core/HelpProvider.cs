using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Helpm8
{
    /// <summary>
    /// Base helper class for implementing an <see cref="IHelpProvider"/>
    /// </summary>
    public abstract class HelpProvider : IHelpProvider
    {
        /// <summary>
        /// Initializes a new <see cref="IHelpProvider"/>
        /// </summary>
        protected HelpProvider()
        {
            Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The help key value pairs for this provider.
        /// </summary>
        protected IDictionary<string, string?> Data { get; set; }

        /// <summary>
        /// Attempts to find a value with the given key, returns true if one is found, false otherwise.
        /// </summary>
        /// <param name="key">The key to lookup.</param>
        /// <param name="value">The value found at key if one is found.</param>
        /// <returns>True if key has a value, false otherwise.</returns>
        public virtual bool TryGet(string key, out string? value)
            => Data.TryGetValue(key, out value);

        /// <summary>
        /// Sets a value for a given key.
        /// </summary>
        /// <param name="key">The configuration key to set.</param>
        /// <param name="value">The value to set.</param>
        public virtual void Set(string key, string? value)
            => Data[key] = value;

        /// <summary>
        /// Loads (or reloads) the data for this provider.
        /// </summary>
        public virtual void Load()
        { }

        /// <summary>
        /// Returns the list of keys that this provider has.
        /// </summary>
        /// <param name="earlierKeys">The earlier keys that other providers contain.</param>
        /// <param name="parentPath">The path for the parent IHelp.</param>
        /// <returns>The list of keys for this provider.</returns>
        public virtual IEnumerable<string> GetChildKeys(
            IEnumerable<string> earlierKeys,
            string? parentPath)
        {
            var results = new List<string>();

            if (parentPath is null)
            {
                foreach (KeyValuePair<string, string?> kv in Data)
                {
                    results.Add(Segment(kv.Key, 0));
                }
            }
            else
            {
                Debug.Assert(HelpPath.KeyDelimiter == ":");

                foreach (KeyValuePair<string, string?> kv in Data)
                {
                    if (kv.Key.Length > parentPath.Length &&
                        kv.Key.StartsWith(parentPath, StringComparison.OrdinalIgnoreCase) &&
                        kv.Key[parentPath.Length] == ':')
                    {
                        results.Add(Segment(kv.Key, parentPath.Length + 1));
                    }
                }
            }

            results.AddRange(earlierKeys);

            return results;
        }

        private static string Segment(string key, int prefixLength)
        {
            int indexOf = key.IndexOf(HelpPath.KeyDelimiter, prefixLength, StringComparison.OrdinalIgnoreCase);
            return indexOf < 0 ? key.Substring(prefixLength) : key.Substring(prefixLength, indexOf - prefixLength);
        }

        /// <summary>
        /// Generates a string representing this provider name and relevant details.
        /// </summary>
        /// <returns> The configuration name. </returns>
        public override string ToString() => $"{GetType().Name}";
    }
}