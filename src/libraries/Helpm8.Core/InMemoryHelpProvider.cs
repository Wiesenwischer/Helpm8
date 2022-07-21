using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Helpm8.InMemory
{
    /// <summary>
    /// In-memory implementation of <see cref="IHelpProvider"/>.
    /// </summary>
    public class InMemoryHelpProvider : HelpProvider, IEnumerable<KeyValuePair<string,string?>>
    {
        /// <summary>
        /// Initialize a new instance from the source.
        /// </summary>
        /// <param name="source">The underlying source.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public InMemoryHelpProvider(InMemoryHelpSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            if (source.InitialData != null)
            {
                foreach (KeyValuePair<string, string?> pair in source.InitialData)
                {
                    Data.Add(pair.Key, pair.Value);
                }
            }
        }

        /// <summary>
        /// Add a new key and value pair.
        /// </summary>
        /// <param name="key">The help key.</param>
        /// <param name="value">The help value.</param>
        public void Add(string key, string? value)
        {
            Data.Add(key, value);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        [ExcludeFromCodeCoverage]
        public IEnumerator<KeyValuePair<string, string?>> GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        [ExcludeFromCodeCoverage]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}