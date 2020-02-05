using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace RDerP.ViewModels
{
    public class RdpTreeViewItem : RderpTreeViewItem
    {
        public RdpTreeViewItem(string path) : base(path)
        {
            var name = Path.GetFileName(path)?.Replace(".rdp", "");
            Header = CreateHeader(name, Properties.Resources.TerminalIcon);

            MouseDoubleClick += OnRdpDoubleClick;
        }

        private static void OnRdpDoubleClick(object sender, MouseButtonEventArgs args)
        {
            var rdpItem = sender as RdpTreeViewItem;
            if (rdpItem == null) return;

            //https://stackoverflow.com/questions/3101392/starting-remote-desktop-client-no-control-over-pid-kill-pid-changes-after-star
            var startInfo = new ProcessStartInfo
            {
                FileName = Constants.Mstsc,
                Arguments = $"\"{rdpItem.FullPath}\""
            };
            Process.Start(startInfo);
        }
    }
}
