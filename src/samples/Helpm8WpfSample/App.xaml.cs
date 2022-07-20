using System.ComponentModel;
using System.IO.Pipes;
using System.Windows;
using System.Windows.Controls.Primitives;
using GreenPipes;
using Helpm8;
using Helpm8.Json;
using Helpm8.Wpf;

namespace Helpm8WpfSample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var help = InitializeHelp();
            var viewModel = new MainWindowViewModel()
            {
                Help = help
            };
            var pipe = Pipe.New<RequestHelpContext>(cfg =>
            {
                var popupHelpDisplayFilter = CreatePopupFilter(viewModel);
                

                cfg.UseFilter(popupHelpDisplayFilter);

            });
            var requestHandler = new HelpRequestHandler(new HelpInfoKeyProvider(), new HelpInfoContextProvider(), pipe);
            var observer = new HelpInfoObserver(requestHandler);
            observer.StartObserving();
            var view = new MainWindow();
            view.DataContext = viewModel;
            view.Show();
        }

        private IHelp InitializeHelp()
        {
            var builder = new HelpBuilder();
            builder.AddJsonFile("help.generated.json");
            builder.AddJsonFile("help.default.json", true);
            builder.AddJsonFile("help.customized.json", true);
            return builder.Build();
        }



        private IFilter<RequestHelpContext> CreatePopupFilter(MainWindowViewModel viewModel)
        {
            var popupHelpDisplayFilter = new PopupHelpDisplayFilter(PlacementMode.Bottom, CustomCommands.Guidance);
            popupHelpDisplayFilter.Attach(viewModel);
            return popupHelpDisplayFilter;
        }
    }

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
