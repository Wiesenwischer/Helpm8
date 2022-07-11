using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace Helpm8.Json
{
    public class JsonHelpSource : IHelpSource
    {
        public IHelpProvider Build(IHelpBuilder builder)
        {
            return new JsonHelpProvider(this);
        }

        /// <summary>
        /// Locate a file at the given path by directly mapping path segments to physical directories.
        /// </summary>
        /// <param name="subpath">A path under the root directory</param>
        /// <returns>The file information. Caller must check <see cref="IFileInfo.Exists"/> property. </returns>
        public IFileInfo? GetFileInfo(string subpath)
        {
            if (string.IsNullOrEmpty(subpath) || PathUtils.HasInvalidPathChars(subpath))
            {
                return new NotFoundFileInfo(subpath);
            }

            // Relative paths starting with leading slashes are okay
            subpath = subpath.TrimStart(PathUtils.PathSeparators);

            // Absolute paths not permitted.
            if (Path.IsPathRooted(subpath))
            {
                return new NotFoundFileInfo(subpath);
            }

            string? fullPath = GetFullPath(subpath);
            if (fullPath == null)
            {
                return new NotFoundFileInfo(subpath);
            }

            var fileInfo = new FileInfo(fullPath);

            return new PhysicalFileInfo(fileInfo);
        }

        /// <summary>
        /// The root directory for this instance.
        /// </summary>
        public string? Root { get; set; }

        /// <summary>
        /// The path to the file.
        /// </summary>
        [DisallowNull]
        public string? FileName { get; set; }

        /// <summary>
        /// Determines if loading the file is optional.
        /// </summary>
        public bool Optional { get; set; }

        private string? GetFullPath(string path)
        {
            if (PathUtils.PathNavigatesAboveRoot(path))
            {
                return null;
            }

            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(Path.Combine(Root, path));
            }
            catch
            {
                return null;
            }

            if (!IsUnderneathRoot(fullPath))
            {
                return null;
            }

            return fullPath;
        }

        private bool IsUnderneathRoot(string fullPath)
        {
            return fullPath.StartsWith(Root, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Represents a file in the given file provider.
    /// </summary>
    public interface IFileInfo
    {
        /// <summary>
        /// True if resource exists in the underlying storage system.
        /// </summary>
        bool Exists { get; }

        /// <summary>
        /// The length of the file in bytes, or -1 for a directory or non-existing files.
        /// </summary>
        long Length { get; }

        /// <summary>
        /// The path to the file, including the file name. Return null if the file is not directly accessible.
        /// </summary>
        string? PhysicalPath { get; }

        /// <summary>
        /// The name of the file or directory, not including any path.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// When the file was last modified
        /// </summary>
        DateTimeOffset LastModified { get; }

        /// <summary>
        /// True for the case TryGetDirectoryContents has enumerated a sub-directory
        /// </summary>
        bool IsDirectory { get; }

        /// <summary>
        /// Return file contents as readonly stream. Caller should dispose stream when complete.
        /// </summary>
        /// <returns>The file stream</returns>
        Stream CreateReadStream();
    }

    /// <summary>
    /// Represents a non-existing file.
    /// </summary>
    public class NotFoundFileInfo : IFileInfo
    {
        /// <summary>
        /// Initializes an instance of <see cref="NotFoundFileInfo"/>.
        /// </summary>
        /// <param name="name">The name of the file that could not be found</param>
        public NotFoundFileInfo(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Always false.
        /// </summary>
        public bool Exists => false;

        /// <summary>
        /// Always false.
        /// </summary>
        public bool IsDirectory => false;

        /// <summary>
        /// Returns <see cref="DateTimeOffset.MinValue"/>.
        /// </summary>
        public DateTimeOffset LastModified => DateTimeOffset.MinValue;

        /// <summary>
        /// Always equals -1.
        /// </summary>
        public long Length => -1;

        /// <inheritdoc />
        public string Name { get; }

        /// <summary>
        /// Always null.
        /// </summary>
        public string? PhysicalPath => null;

        /// <summary>
        /// Always throws. A stream cannot be created for non-existing file.
        /// </summary>
        /// <exception cref="FileNotFoundException">Always thrown.</exception>
        /// <returns>Does not return</returns>
        [DoesNotReturn]
        public Stream CreateReadStream()
        {
            throw new FileNotFoundException($"The file {Name} does not exist.");
        }
    }

    /// <summary>
    /// Represents a file on a physical filesystem
    /// </summary>
    public class PhysicalFileInfo : IFileInfo
    {
        private readonly FileInfo _info;

        /// <summary>
        /// Initializes an instance of <see cref="PhysicalFileInfo"/> that wraps an instance of <see cref="System.IO.FileInfo"/>
        /// </summary>
        /// <param name="info">The <see cref="System.IO.FileInfo"/></param>
        public PhysicalFileInfo(FileInfo info)
        {
            _info = info;
        }

        /// <inheritdoc />
        public bool Exists => _info.Exists;

        /// <inheritdoc />
        public long Length => _info.Length;

        /// <inheritdoc />
        public string PhysicalPath => _info.FullName;

        /// <inheritdoc />
        public string Name => _info.Name;

        /// <inheritdoc />
        public DateTimeOffset LastModified => _info.LastWriteTimeUtc;

        /// <summary>
        /// Always false.
        /// </summary>
        public bool IsDirectory => false;

        /// <inheritdoc />
        public Stream CreateReadStream()
        {
            // We are setting buffer size to 1 to prevent FileStream from allocating it's internal buffer
            // 0 causes constructor to throw
            int bufferSize = 1;
            return new FileStream(
                PhysicalPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite,
                bufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
        }
    }

    internal static class PathUtils
    {
        private static readonly char[] _invalidFileNameChars = Path.GetInvalidFileNameChars()
            .Where(c => c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar).ToArray();

        internal static readonly char[] PathSeparators = new[]
            {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar};

        internal static bool HasInvalidPathChars(string path)
        {
            return path.IndexOfAny(_invalidFileNameChars) != -1;
        }


        internal static bool PathNavigatesAboveRoot(string path)
        {
            var tokenizer = new StringTokenizer(path, PathSeparators);
            int depth = 0;

            foreach (StringSegment segment in tokenizer)
            {
                if (segment.Equals(".") || segment.Equals(""))
                {
                    continue;
                }
                else if (segment.Equals(".."))
                {
                    depth--;

                    if (depth == -1)
                    {
                        return true;
                    }
                }
                else
                {
                    depth++;
                }
            }

            return false;
        }
    }
}
