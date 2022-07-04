using System.Collections.Generic;

namespace Helpm8
{
    /// <summary>
    /// Concept of the root of an <see cref="IHelpInfo"/> hierarchy for an application.
    /// </summary>
    public interface IHelpInfoRoot : IHelpInfo
    {
        /// <summary>
        /// Loads all help info values fro the underlying <see cref="IHelpInfoProvider"/>s.
        /// </summary>
        void Load();

        /// <summary>
        /// The <see cref="IHelpInfoProvider"/>s for these help infos.
        /// </summary>
        IEnumerable<IHelpInfoProvider> Providers { get; }
    }
}
