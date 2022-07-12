using System;

namespace Helpm8
{
    public static  class HelpBuilderExtensions
    {
        /// <summary>
        /// Adds a new help source.
        /// </summary>
        /// <param name="builder">The builder to add to.</param>
        /// <param name="configureSource">Configures the help source.</param>
        /// <returns>The <see cref="IHelpBuilder"/>.</returns>
        public static IHelpBuilder Add<TSource>(this IHelpBuilder builder, Action<TSource>? configureSource) where TSource : IHelpSource, new()
        {
            var source = new TSource();
            configureSource?.Invoke(source);
            return builder.Add(source);
        }
    }
}
