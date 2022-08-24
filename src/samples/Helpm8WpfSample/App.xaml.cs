using System;
using GreenPipes;
using Helpm8;
using Helpm8.Json;
using Helpm8.Wpf;
using Helpm8.Wpf.Markdown;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Helpm8WpfSample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var viewModel = new MainWindowViewModel();
            var pipe = Pipe.New<RequestHelpContext>(cfg =>
            {
                var technicalHelp = new HelpBuilder()
                    .AddJsonFile("help.technical.json", true)
                    .Build();

                cfg.UseFilter(new ProvideAdditionalHelpFilter(technicalHelp));
                cfg.UseFilter(new AppendHelpFilter<IProvideAdditionalHelpContext>());

#if MD
                var popupHelpDisplayFilter = CreateMarkdownFilter(viewModel);
#else
                var popupHelpDisplayFilter = CreatePopupFilter(viewModel);
#endif

                cfg.UseFilter(popupHelpDisplayFilter);
            });
            //var requestHandler = new HelpRequestHandler(new HelpInfoKeyProvider(), new MainWindowViewModelHelpProvider(viewModel), pipe);
            var requestHandler = new HelpRequestHandler(new HelpInfoKeyProvider(), new HelpInfoContextProvider(), pipe);
            var observer = new HelpInfoObserver(requestHandler);
            observer.StartObserving();
            var view = new MainWindow
            {
                DataContext = viewModel
            };
            view.Show();
        }

        private IFilter<RequestHelpContext> CreateMarkdownFilter(MainWindowViewModel viewModel)
        {
            var markdownPopupHelpDisplayFilter = new MarkdownPopupHelpDisplayFilter(PlacementMode.Bottom, CustomCommands.Guidance);
            markdownPopupHelpDisplayFilter.Attach(viewModel);
            return markdownPopupHelpDisplayFilter;
        }

        private IFilter<RequestHelpContext> CreatePopupFilter(MainWindowViewModel viewModel)
        {
            var popupHelpDisplayFilter = new PopupHelpDisplayFilter(PlacementMode.Bottom, CustomCommands.Guidance);
            popupHelpDisplayFilter.Attach(viewModel);
            return popupHelpDisplayFilter;
        }
    }

    public class MainWindowViewModelHelpProvider : IProvideHelpContext
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindowViewModelHelpProvider(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }

        public IHelp GetHelpContextFor(UIElement element)
        {
            return _viewModel.Help;
        }
    }
}
