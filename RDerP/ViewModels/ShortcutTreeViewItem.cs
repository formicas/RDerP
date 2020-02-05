using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace RDerP.ViewModels
{
    public class ShortcutTreeViewItem : RderpTreeViewItem
    {
        public ShortcutTreeViewItem(string path) : base(path)
        {
            var name = Path.GetFileName(path)?.Replace(".lnk", "");
            Header = CreateHeader(name, Properties.Resources.ExplorerIcon);

            MouseDoubleClick += OnLnkDoubleClick;
        }

        private static void OnLnkDoubleClick(object sender, MouseButtonEventArgs args)
        {
            var lnkItem = sender as ShortcutTreeViewItem;
            if (lnkItem == null) return;

            Process.Start(lnkItem.FullPath);
        }

    }
}
