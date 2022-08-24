using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Helpm8;
using Helpm8.Json;
using Helpm8.Wpf;

namespace Helpm8WpfSample
{
    public class MainWindowViewModel : IHelpCommandObserver, INotifyPropertyChanged
    {
        private bool _isHelpActive;
        private IHelp _help;
        private bool _includeDefaultHelp = true;
        private bool _includeCustomizedHelp = true;

        public MainWindowViewModel()
        {
            Help = BuildHelp();
        }

        public IHelp Help
        {
            get => _help;
            set
            {
                if (value != _help)
                {
                    _help = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string Title { get; } = "Helpm8 - WPF Sample App";

        public bool IsHelpActive
        {
            get => _isHelpActive;
            set
            {
                if (value != _isHelpActive)
                {
                    _isHelpActive = value;
                    RaisePropertyChanged();
                }
            }
        }

        // ReSharper disable once UnusedMember.Global
        public bool IncludeDefaultHelp
        {
            get => _includeDefaultHelp;
            set 
            {
                if (value != _includeDefaultHelp)
                {
                    _includeDefaultHelp = value;
                    RaisePropertyChanged();
                    Help = BuildHelp();
                }
            }
        }

        // ReSharper disable once UnusedMember.Global
        public bool IncludeCustomizedHelp
        {
            get => _includeCustomizedHelp;
            set
            {
                if (value != _includeCustomizedHelp)
                {
                    _includeCustomizedHelp = value;
                    RaisePropertyChanged();
                    Help = BuildHelp();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private IHelp BuildHelp()
        {
            var builder = new HelpBuilder();
            builder.AddJsonFile("help.generated.json");
            if (IncludeDefaultHelp)
            {
                builder.AddJsonFile("help.default.json");
            }
            if (IncludeCustomizedHelp)
            {
                builder.AddJsonFile("help.customized.json");
            }
            return builder.Build();
        }
        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}