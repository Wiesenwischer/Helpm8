using System.ComponentModel;
using Helpm8;
using Helpm8.Wpf;

namespace Helpm8WpfSample
{
    public class MainWindowViewModel : IHelpCommandObserver, INotifyPropertyChanged
    {
        private bool _isHelpActive;
        public MainWindowViewModel()
        {
            
        }

        public IHelp Help { get; set; }

        public string Title { get; } = "Helpm8 - WPF Sample App";

        public bool IsHelpActive
        {
            get => _isHelpActive;
            set
            {
                if (value != _isHelpActive)
                {
                    _isHelpActive = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsHelpActive)));
                }
            }
            
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}