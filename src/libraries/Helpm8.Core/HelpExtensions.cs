using System;
using System.Collections.Generic;
using System.Text;

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
        public static IEnumerable<KeyValuePair<string, string?>> AsEnumerable(this IHelp help, bool makePathsRelative)
        {
            var stack = new Stack<IHelp>();
            stack.Push(help);
            int prefixLength = (makePathsRelative && help is IHelpSection rootSection) ? rootSection.Path.Length + 1 : 0;
            while (stack.Count > 0)
            {
                IHelp config = stack.Pop();
                // Don't include the sections value if we are removing paths, since it will be an empty key
                if (config is IHelpSection section && (!makePathsRelative || config != help))
                {
                    yield return new KeyValuePair<string, string?>(section.Path.Substring(prefixLength), section.Value);
                }
                foreach (IHelpSection child in config.GetChildren())
                {
                    stack.Push(child);
                }
            }
        }
    }
}
