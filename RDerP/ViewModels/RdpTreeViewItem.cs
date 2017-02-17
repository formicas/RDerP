using System.Windows.Controls;

namespace RDerP.ViewModels
{
    class RdpTreeViewItem : TreeViewItem
    {
        public string Path { get; }

        public RdpTreeViewItem(object header, string path)
        {
            Header = header;
            Path = path;
        }
    }
}
