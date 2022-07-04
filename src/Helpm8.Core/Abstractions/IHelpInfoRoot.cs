using System.Collections.Generic;

namespace Helpm8
{
    /// <summary>
    /// Concept of the root of an <see cref="IHelpInfo"/> hierarchy for an application.
    /// </summary>
    public interface IHelpInfoRoot : IHelpInfo
    {
        /// <summary>
        /// Loads all help info values fro the underlying <see cref="IHelpProvider"/>s.
        /// </summary>
        void Load();

        /// <summary>
        /// The <see cref="IHelpProvider"/>s for these help infos.
        /// </summary>
        IEnumerable<IHelpProvider> Providers { get; }
    }
}
