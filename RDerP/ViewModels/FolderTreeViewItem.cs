using System;
using System.Windows.Controls;

namespace RDerP.ViewModels
{
    class FolderTreeViewItem : TreeViewItem
    {
        public string Path { get; }

        public FolderTreeViewItem(object header, string path)
        {
            Path = path;
            Header = header;
        }
    }
}
