using System;
using System.Collections.Generic;

namespace Helpm8.InMemory
{
    /// <summary>
    /// In-memory implementation of <see cref="IHelpSource"/>.
    /// </summary>
    public class InMemoryHelpSource : IHelpSource
    {
        /// <summary>
        /// Initial key value help pairs.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string?>>? InitialData { get; set; }

        /// <summary>
        /// Builds the <see cref="InMemoryHelpProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IHelpProvider"/></param>
        /// <returns>A <see cref="InMemoryHelpProvider"/></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IHelpProvider Build(IHelpBuilder builder)
        {
            return new InMemoryHelpProvider(this);
        }
    }
}
