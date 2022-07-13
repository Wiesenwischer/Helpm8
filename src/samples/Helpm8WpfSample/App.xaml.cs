using System.Windows;
using Helpm8;
using Helpm8.Json;
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

            var view = new MainWindow();
            view.DataContext = viewModel;
            view.Show();
        }

        private IHelp InitializeHelp()
        {
            var builder = new HelpBuilder();
            builder.AddJsonFile("help.generated.json");
            builder.AddJsonFile("help.json", true);
            builder.AddJsonFile("help.customized.json", true);
            return builder.Build();
        }
    }

    public class MainWindowViewModel 
    {
        public IHelp Help { get; set; }

        public string Title { get; } = "Helpm8 - WPF Sample App";
    }
}
