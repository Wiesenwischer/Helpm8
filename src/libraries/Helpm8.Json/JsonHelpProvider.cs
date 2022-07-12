using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace Helpm8.Json
{
    /// <summary>
    /// A JSON file based <see cref="HelpProvider"/>.
    /// </summary>
    public class JsonHelpProvider : HelpProvider
    {
        /// <summary>
        /// Initializes a new instance with the specified source.
        /// </summary>
        /// <param name="source">The source settings.</param>
        public JsonHelpProvider(JsonHelpSource source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public JsonHelpSource Source { get; }

        public void Load(Stream stream)
        {
            Data = JsonHelpFileParser.Parse(stream);
        }

        public override void Load()
        {
            IFileInfo? file = Source.GetFileInfo(Source.FileName ?? string.Empty);
            if (file == null || !file.Exists)
            {
                if (Source.Optional) 
                {
                    Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    var error = new StringBuilder(
                        $"The help file '{Source.FileName}' was not found and is not optional.");
                    if (!string.IsNullOrEmpty(file?.PhysicalPath))
                    {
                        error.Append($" The expected physical path was '{file.PhysicalPath}'.");
                    }
                    HandleException(ExceptionDispatchInfo.Capture(new FileNotFoundException(error.ToString())));
                }
            }
            else
            {
                static Stream OpenRead(IFileInfo fileInfo)
                {
                    if (fileInfo.PhysicalPath != null)
                    {
                        // The default physical file info assumes asynchronous IO which results in unnecessary overhead
                        // especially since the configuration system is synchronous. This uses the same settings
                        // and disables async IO.
                        return new FileStream(
                            fileInfo.PhysicalPath,
                            FileMode.Open,
                            FileAccess.Read,
                            FileShare.ReadWrite,
                            bufferSize: 1,
                            FileOptions.SequentialScan);
                    }

                    return fileInfo.CreateReadStream();
                }

                using Stream stream = OpenRead(file);
                try
                {
                    Load(stream);
                }
                catch (Exception ex)
                {
                    var exception = new InvalidDataException($"Failed to load configuration from file '{file.PhysicalPath}'.", ex);
                    HandleException(ExceptionDispatchInfo.Capture(exception));
                }
            }
        }

        private static void HandleException(ExceptionDispatchInfo info)
        {
            info.Throw();
        }
    }
}
