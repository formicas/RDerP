using RDerP.Models;
using RDerP.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace RDerP
{
    public class TreeViewManager
    {
        private readonly TreeView _treeView;
        private readonly string _rootDirectory;

        public TreeViewManager(TreeView treeView, string rootDirectory)
        {
            _treeView = treeView;
            _rootDirectory = rootDirectory;
        }

        public FolderTreeViewItem GetFolderItemForPath(string path)
        {
            var parent = Directory.GetParent(path).FullName;

            if (parent == _rootDirectory)
            {
                return _treeView.Items.Cast<TreeViewItem>().OfType<FolderTreeViewItem>().FirstOrDefault(i => i.FullPath == path);
            }

            var parentItem = GetFolderItemForPath(parent);

            return parentItem?.Items.Cast<TreeViewItem>().OfType<FolderTreeViewItem>().FirstOrDefault(i => i.FullPath == path);
        }

        public void AddRootToTreeView(ApplicationState appState)
        {
            _treeView.Items.Clear();
            var subDirPaths = Directory.GetDirectories(_rootDirectory);

            foreach (var sub in subDirPaths)
            {
                var folderItem = new FolderTreeViewItem(sub)
                {
                    IsExpanded = appState.ExpandedPaths.Contains(sub)
                };
                _treeView.Items.Add(folderItem);
                AddChildren(folderItem, appState);
            }
            
            var filePaths = Directory.GetFiles(_rootDirectory, "*.rdp");

            foreach (var filePath in filePaths)
            {
                _treeView.Items.Add(new RdpTreeViewItem(filePath));
            }
        }

        public void AddChildren(FolderTreeViewItem parent, ApplicationState appState)
        {
            parent.Items.Clear();
            var dirPath = parent.FullPath;

            var subDirPaths = Directory.GetDirectories(dirPath);
            foreach (var sub in subDirPaths)
            {
                var newItem = new FolderTreeViewItem(sub)
                {
                    IsExpanded = appState.ExpandedPaths.Contains(sub)
                };
                parent.Items.Add(newItem);
                AddChildren(newItem, appState);
            }

            var filePaths = Directory.GetFiles(dirPath, "*.rdp");
            foreach (var filePath in filePaths)
            {
                parent.Items.Add(new RdpTreeViewItem(filePath));
            }

            filePaths = Directory.GetFiles(dirPath, "*.lnk");
            foreach(var filePath in filePaths)
            {
                parent.Items.Add(new ShortcutTreeViewItem(filePath));
            }
        }

        public FolderTreeViewItem GetParentFolder(TreeViewItem item)
        {
            switch (item)
            {
                //rubbish in, rubbish out
                case null:
                    return null;
                //if it is a folder, return it
                case FolderTreeViewItem folderItem:
                    return folderItem;
                //otherwise it's a shortcut or rpd session, return the parent, which must be a folder
                default:
                    return item.Parent as FolderTreeViewItem;
            }
        }

        public IEnumerable<string> GetExpandedFolders()
        {
            var folders = new List<string>();
            GetExpandedFolders(null, folders);
            return folders;
        }

        private void GetExpandedFolders(FolderTreeViewItem item, IList<string> folders)
        {
            IEnumerable<FolderTreeViewItem> expandedSubs;
            if (item == null)
            {
                expandedSubs =
                    _treeView.Items.Cast<TreeViewItem>()
                        .OfType<FolderTreeViewItem>()
                        .Where(i => i.IsExpanded);
            }
            else
            {
                expandedSubs = item.Items.Cast<TreeViewItem>()
                    .OfType<FolderTreeViewItem>()
                    .Where(i => i.IsExpanded);
            }

            foreach (FolderTreeViewItem sub in expandedSubs)
            {
                folders.Add(sub.FullPath);
                GetExpandedFolders(sub, folders);
            }
        }

    }
}
