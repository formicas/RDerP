using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace RDerP
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
