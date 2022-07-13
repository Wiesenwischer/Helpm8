using System.Diagnostics.CodeAnalysis;

namespace Helpm8
{
    /// <summary>
    /// Represents a chained <see cref="IHelp"/> as an <see cref="IHelpSource"/>.
    /// </summary>
    public class ChainedHelpSource : IHelpSource
    {
        /// <summary>
        /// The chained help.
        /// </summary>
        //[DisallowNull]
        public IHelp? Help { get; set; }

        /// <summary>
        /// Whether the chained help should be disposed when the
        /// help provider gets disposed.
        /// </summary>
        public bool ShouldDisposeHelp { get; set; }

        /// <summary>
        /// Builds the <see cref="ChainedHelpProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IHelpBuilder"/>.</param>
        /// <returns>A <see cref="ChainedHelpProvider"/></returns>
        public IHelpProvider Build(IHelpBuilder builder)
            => new ChainedHelpProvider(this);
    }
}