using System.Windows.Controls;

namespace RDerP.ViewModels
{
    public class RderpTreeViewItem : TreeViewItem
    {
        public string Path { get; }

        public RderpTreeViewItem(object header, string path)
        {
            Header = header;
            Path = path;
        }
    }
}
