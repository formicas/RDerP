using System.IO;

namespace RDerP.ViewModels
{
    public class FolderTreeViewItem : RderpTreeViewItem
    {
        public FolderTreeViewItem(string path) : base(path)
        {
            var name = Path.GetFileName(path);
            Header = CreateHeader(name, Properties.Resources.FolderIcon);
        }
    }
}
