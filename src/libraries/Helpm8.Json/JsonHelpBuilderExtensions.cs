using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Helpm8.Json
{
    public static class JsonHelpBuilderExtensions
    {
        /// <summary>
        /// Adds the JSON help provider at <paramref name="path"/> to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IHelpBuilder"/> to add to.</param>
        /// <param name="path">Path of the help source.</param>
        /// <returns>The <see cref="IHelpBuilder"/>.</returns>
        public static IHelpBuilder AddJsonFile(this IHelpBuilder builder, string path)
        {
            return AddJsonFile(builder, path: path, optional: false);
        }

        /// <summary>
        /// Adds the JSON help provider at <paramref name="path"/> to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IHelpBuilder"/> to add to.</param>
        /// <param name="path">Path of the help source.</param>
        /// <param name="optional">Whether the file is optional.</param>
        /// <returns>The <see cref="IHelpBuilder"/>.</returns>
        /// <summary>
        public static IHelpBuilder AddJsonFile(this IHelpBuilder builder, string path, bool optional)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("File path must be a non-empty string.", nameof(path));
            }

            return builder.AddJsonFile(s =>
            {
                s.Root = Path.GetDirectoryName(path);
                s.FileName = Path.GetFileName(path);
                s.Optional = optional;
            });
        }

        /// <summary>
        /// Adds a JSON help source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IHelpBuilder"/> to add to.</param>
        /// <param name="configureSource">Configures the source.</param>
        /// <returns>The <see cref="IHelpBuilder"/>.</returns>
        public static IHelpBuilder AddJsonFile(this IHelpBuilder builder, Action<JsonHelpSource>? configureSource)
            => builder.Add(configureSource);
    }
}
