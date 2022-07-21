using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Helpm8
{
    public static  class HelpExtensions
    {
        /// <summary>
        /// Get the enumeration of key value pairs within the <see cref="IHelp" />
        /// </summary>
        /// <param name="help">The help to enumerate.</param>
        /// <returns>An enumeration of key value pairs.</returns>
        public static IEnumerable<KeyValuePair<string, string?>> AsEnumerable(this IHelp help) => help.AsEnumerable(makePathsRelative: false);

        /// <summary>
        /// Get the enumeration of key value pairs within the <see cref="IHelp" />
        /// </summary>
        /// <param name="help">The help to enumerate.</param>
        /// <param name="makePathsRelative">If true, the child keys returned will have the current help's Path trimmed from the front.</param>
        /// <returns>An enumeration of key value pairs.</returns>
        [PublicAPI]
        public static IEnumerable<KeyValuePair<string, string?>> AsEnumerable(this IHelp help, bool makePathsRelative)
        {
            var stack = new Stack<IHelp>();
            stack.Push(help);
            int prefixLength = (makePathsRelative && help is IHelpSection rootSection) ? rootSection.Path.Length + 1 : 0;
            while (stack.Count > 0)
            {
                IHelp poppedHelp = stack.Pop();
                // Don't include the sections value if we are removing paths, since it will be an empty key
                if (poppedHelp is IHelpSection section && (!makePathsRelative || poppedHelp != help))
                {
                    yield return new KeyValuePair<string, string?>(section.Path.Substring(prefixLength), section.Value);
                }
                foreach (IHelpSection child in poppedHelp.GetChildren())
                {
                    stack.Push(child);
                }
            }
        }

        /// <summary>
        /// Determines whether the section has a <see cref="IHelpSection.Value"/> or has children
        /// </summary>
        /// <param name="section">The section to enumerate.</param>
        /// <returns><see langword="true" /> if the section has values or children; otherwise, <see langword="false" />.</returns>
        public static bool Exists(this IHelpSection? section)
        {
            if (section == null)
            {
                return false;
            }
            return section.Value != null || section.GetChildren().Any();
        }
    }
}
