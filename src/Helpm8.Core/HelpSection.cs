using System;
using System.Collections.Generic;
using System.Text;

namespace Helpm8
{
    public class HelpSection : IHelpSection
    {
        private readonly IHelpRoot _root;
        private readonly string _path;
        private string? _key;

        public HelpSection(IHelpRoot root, string path)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public string? this[string key]
        {
            get
            {
                return _root[HelpPath.Combine(Path, key)];
            }
            set
            {
                _root[HelpPath.Combine(Path, key)] = value;
            }
        }

        public IHelpSection GetSection(string key) => _root.GetSection(HelpPath.Combine(Path, key));

        public IEnumerable<IHelpSection> GetChildren() => _root.GetChildrenImplementation(Path);

        public string Key
        {
            get { return _key ??= HelpPath.GetSectionKey(_path); }
        }

        public string Path => _path;

        public string? Value
        {
            get
            {
                return _root[Path];
            }
            set
            {
                _root[Path] = value;
            }
        }
    }
}
