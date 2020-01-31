using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using RDerP.Models;
using RDerP.ViewModels;

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
                return _treeView.Items.Cast<TreeViewItem>().OfType<FolderTreeViewItem>().FirstOrDefault(i => i.Path == path);
            }

            var parentItem = GetFolderItemForPath(parent);

            return parentItem?.Items.Cast<TreeViewItem>().OfType<FolderTreeViewItem>().FirstOrDefault(i => i.Path == path);
        }

        public void AddRootToTreeView(ApplicationState appState)
        {
            _treeView.Items.Clear();
            var subDirPaths = Directory.GetDirectories(_rootDirectory);

            foreach (var sub in subDirPaths)
            {
                var folderItem = new FolderTreeViewItem(CreateDirectoryHeader(Path.GetFileName(sub)), sub)
                {
                    IsExpanded = appState.ExpandedPaths.Contains(sub)
                };
                _treeView.Items.Add(folderItem);
                AddChildren(folderItem, appState);
            }
            
            var filePaths = Directory.GetFiles(_rootDirectory, "*.rdp");

            foreach (var filePath in filePaths)
            {
                _treeView.Items.Add(CreateRdpItem(filePath));
            }
        }

        private StackPanel CreateDirectoryHeader(string name)
        {
            return CreateHeader(name, Properties.Resources.FolderIcon);
        }

        private StackPanel CreateHeader(string name, string imagePath)
        {
            var image = new Image { Source = new BitmapImage(new Uri(imagePath)) };
            var label = new Label { Content = name };

            var stack = new StackPanel { Orientation = Orientation.Horizontal };

            stack.Children.Add(image);
            stack.Children.Add(label);

            return stack;
        }

        private RderpTreeViewItem CreateRdpItem(string path)
        {
            var name = Path.GetFileName(path)?.Replace(".rdp", "");
            var item = new RderpTreeViewItem(CreateRdpHeader(name), path);
            item.MouseDoubleClick += OnRdpDoubleClick;
            return item;
        }

        private StackPanel CreateRdpHeader(string name)
        {
            return CreateHeader(name, Properties.Resources.TerminalIcon);
        }

        private RderpTreeViewItem CreateLnkItem(string path)
        {
            var name = Path.GetFileName(path)?.Replace(".lnk", "");
            var item = new RderpTreeViewItem(CreateLnkHeader(name), path);
            item.MouseDoubleClick += OnLnkDoubleClick;
            return item;
        }

        private StackPanel CreateLnkHeader(string name)
        {
            return CreateHeader(name, Properties.Resources.ExplorerIcon);
        }

        public void AddChildren(FolderTreeViewItem parent, ApplicationState appState)
        {
            parent.Items.Clear();
            var dirPath = parent.Path;

            var subDirPaths = Directory.GetDirectories(dirPath);
            foreach (var sub in subDirPaths)
            {
                var newItem = new FolderTreeViewItem(CreateDirectoryHeader(Path.GetFileName(sub)), sub)
                {
                    IsExpanded = appState.ExpandedPaths.Contains(sub)
                };
                parent.Items.Add(newItem);
                AddChildren(newItem, appState);
            }

            var filePaths = Directory.GetFiles(dirPath, "*.rdp");
            foreach (var filePath in filePaths)
            {
                parent.Items.Add(CreateRdpItem(filePath));
            }

            filePaths = Directory.GetFiles(dirPath, "*.lnk");
            foreach(var filePath in filePaths)
            {
                parent.Items.Add(CreateLnkItem(filePath));
            }
        }

        private void OnRdpDoubleClick(object sender, MouseButtonEventArgs args)
        {
            var rdpItem = sender as RderpTreeViewItem;
            if (rdpItem == null) return;

            LaunchRDPSession(rdpItem.Path);
        }

        private void LaunchRDPSession(string path)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Constants.Mstsc,
                Arguments = $"\"{path}\""
            };
            Process.Start(startInfo);
        }

        private void OnLnkDoubleClick(object sender, MouseButtonEventArgs args)
        {
            var lnkItem = sender as RderpTreeViewItem;
            if (lnkItem == null) return;

            Process.Start(lnkItem.Path);
        }

        public FolderTreeViewItem GetParent(TreeViewItem item)
        {
            FolderTreeViewItem folderItem = null;
            if (item != null)
            {
                var rdpItem = item as RderpTreeViewItem;
                if (rdpItem != null)
                {
                    folderItem = rdpItem.Parent as FolderTreeViewItem;
                }
                else
                {
                    folderItem = item as FolderTreeViewItem;
                }
            }

            return folderItem;
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
                folders.Add(sub.Path);
                GetExpandedFolders(sub, folders);
            }
        }

    }
}
