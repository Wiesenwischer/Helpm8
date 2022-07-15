using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MdXaml;

namespace Helpm8.Wpf.Markdown
{
    public class MarkdownPopupHelpDisplayFilter : PopupHelpDisplayFilter
    {
        public MarkdownPopupHelpDisplayFilter(PlacementMode placement, ICommand command) : base(placement, command)
        {
        }

        protected override object CreatePopupContent(RequestHelpContext helpContext)
        {
            MdXaml.Markdown engine = new MdXaml.Markdown();
            string markdowntext = $@"
# %{{color:blue}}Feldname%
{helpContext.HelpText}";
            var control = new MarkdownScrollViewer();
            control.MarkdownStyleName = "GithubLike";
            control.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            control.Markdown = markdowntext;
            var document = engine.Transform(markdowntext);
            return control;

        }
    }
}