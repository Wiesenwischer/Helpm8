using System;
using System.Diagnostics.CodeAnalysis;

namespace Helpm8{
/// <summary>
/// Extension methods for adding <see cref="IHelp"/> to an <see cref="IHelpBuilder"/>.
/// </summary>
public static class ChainedBuilderExtensions
{
    /// <summary>
    /// Adds an existing help to <paramref name="helpBuilder"/>.
    /// </summary>
    /// <param name="helpBuilder">The <see cref="IHelpBuilder"/> to add to.</param>
    /// <param name="help">The <see cref="IHelp"/> to add.</param>
    /// <returns>The <see cref="IHelpBuilder"/>.</returns>
    public static IHelpBuilder AddHelp(this IHelpBuilder helpBuilder, IHelp help)
        => AddHelp(helpBuilder, help, shouldDisposeHelp: false);

    /// <summary>
    /// Adds an existing help to <paramref name="helpBuilder"/>.
    /// </summary>
    /// <param name="helpBuilder">The <see cref="IHelpBuilder"/> to add to.</param>
    /// <param name="help">The <see cref="IHelp"/> to add.</param>
    /// <param name="shouldDisposeHelp">Whether the help should get disposed when the help provider is disposed.</param>
    /// <returns>The <see cref="IHelpBuilder"/>.</returns>
    public static IHelpBuilder AddHelp(this IHelpBuilder helpBuilder, IHelp help, bool shouldDisposeHelp)
    {
        if (helpBuilder == null) throw new ArgumentNullException(nameof(helpBuilder));
        if (help == null) throw new ArgumentNullException(nameof(help));

        helpBuilder.Add(new ChainedHelpSource
        {
            Help = help,
            ShouldDisposeHelp = shouldDisposeHelp,
        });
        return helpBuilder;
    }
}
}