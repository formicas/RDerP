using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json;
using RDerP.IO;
using RDerP.Models;
using RDerP.ViewModels;

namespace RDerP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        //the extra height from the title bar that's not factored into any height calculations
        private const int HORRIBLE_CONSTANT = 30;
        private readonly string _executingDirectory;
        private readonly TreeViewManager _treeViewManager;
        private Point _dragStart;

        public MainWindow()
        {
            InitializeComponent();

            _executingDirectory = Directory.GetCurrentDirectory();
            _treeViewManager = new TreeViewManager(rdpTree, _executingDirectory);

            _treeViewManager.AddRootToTreeView(LoadApplicationState());

            var watcher = new FileSystemWatcher(_executingDirectory)
            {
                IncludeSubdirectories = true
            };

            watcher.Created += FileSystemChange;
            watcher.Renamed += FileSystemChange;
            watcher.Deleted += FileSystemChange;

            watcher.EnableRaisingEvents = true;
        }

        private void FileSystemChange(object sender, FileSystemEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var fullPath = e.FullPath;

                //on deletion we can't differentiate between a directory path or a file path with no extension
                //just have to assume it's relevant and force a refresh
                if (e.ChangeType != WatcherChangeTypes.Deleted)
                {
                    var attributes = File.GetAttributes(fullPath);
                    //if the change isn't to a directory or .rdp file, ignore it.
                    if ((attributes & FileAttributes.Directory) != FileAttributes.Directory &&
                        !fullPath.EndsWith(".rdp", StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                }

                var parent = Directory.GetParent(fullPath).FullName;

                if (parent == _executingDirectory)
                {
                    _treeViewManager.AddRootToTreeView(GetApplicationState());
                    return;
                }

                var folderItem = _treeViewManager.GetFolderItemForPath(parent);

                if (folderItem != null)
                {
                    _treeViewManager.AddChildren(folderItem, GetApplicationState());
                }
            });
        }

        private void AddRdpItem(object sender, RoutedEventArgs e)
        {
            //get the parent treeviewitem
            var folderItem = _treeViewManager.GetParent(rdpTree.SelectedItem as TreeViewItem);

            var directoryPath = folderItem != null ? folderItem.Path : _executingDirectory;

            var dialog = new AddDialog(directoryPath);

            SetDialogPosition(dialog, add);

            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                LaunchRDPEdit(dialog.FullPath);
            }
        }

        private void AddFolder(object sender, RoutedEventArgs e)
        {
            //get the parent treeviewitem
            var folderItem = _treeViewManager.GetParent(rdpTree.SelectedItem as TreeViewItem);

            var directoryPath = folderItem != null ? folderItem.Path : _executingDirectory;

            var dialog = new AddDialog(directoryPath, ItemType.Folder);

            SetDialogPosition(dialog, addFolder);

            dialog.ShowDialog();
        }

        private void TreeView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //should probably make this nicer but for now it works
            //if we detect a click which isn't on an item, deselect the currently selected item
            var treeViewItem = FindTreeViewAncestor(e.OriginalSource as DependencyObject);
            if (treeViewItem == null)
            {
                var item = rdpTree.SelectedItem as TreeViewItem;
                if (item != null)
                    item.IsSelected = false;
            }
        }

        private void TreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStart = e.GetPosition(null);
        }

        private void TreeView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            var current = e.GetPosition(null);
            var diff = _dragStart - current;

            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                var treeView = sender as TreeView;
                var treeViewItem = FindTreeViewAncestor(e.OriginalSource as DependencyObject);

                if (treeViewItem == null || treeView == null)
                    return;

                //DataObject dragData;
                //var draggedFolder = rdpTree.SelectedItem as FolderTreeViewItem;
                //var draggedRdp = rdpTree.SelectedItem as RdpTreeViewItem;
                //if (draggedFolder != null)
                //{
                //    dragData = new DataObject(draggedFolder);
                //}
                //else if (draggedRdp != null)
                //{
                //    dragData = new DataObject(draggedRdp);
                //}
                //else
                //{
                //    return;
                //}
                var item = rdpTree.SelectedItem as TreeViewItem;
                if (item == null)
                {
                    return;
                }
                var dragData = new DataObject(item);
                DragDrop.DoDragDrop(treeViewItem, dragData, DragDropEffects.Move);
            }
        }

        private void TreeView_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(TreeViewItem)))
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void TreeView_Drop(object sender, DragEventArgs e)
        {
            var oldPath = string.Empty;
            if (e.Data.GetDataPresent(typeof(FolderTreeViewItem)) &&
                e.Data.GetData(typeof(FolderTreeViewItem)) is FolderTreeViewItem folderItem)
            {
                oldPath = folderItem.Path;
            }else if (e.Data.GetDataPresent(typeof(RdpTreeViewItem)) &&
                      e.Data.GetData(typeof(RdpTreeViewItem)) is RdpTreeViewItem rdpItem)
            {
                oldPath = rdpItem.Path;
            }

            if (string.IsNullOrEmpty(oldPath))
            {
                return;
            }

            var targetTreeViewItem = FindTreeViewAncestor(e.OriginalSource as DependencyObject);

            var newParentPath = string.Empty;
            if (targetTreeViewItem != null)
            {
                var newParent = _treeViewManager.GetParent(targetTreeViewItem);
                newParentPath = newParent?.Path ?? _executingDirectory;
            }else if (sender is TreeView)
            {
                newParentPath = _executingDirectory;
            }

            if (string.IsNullOrEmpty(newParentPath))
            {
                return;
            }

            var newPath = Path.Combine(newParentPath, Path.GetFileName(oldPath));

            if (string.Equals(newPath, oldPath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                Directory.Move(oldPath, newPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Constants.ErrorMessageTitle);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //on close, we want to find all the expanded folders and write them to our 'persistence' file
            try
            {
                FileHelper.SaveState(GetApplicationState());
            }
            catch
            {
                //todo write to event log
                //swallow the shit out of this bad boy - nobody likes seeing errors on close
            }

            base.OnClosing(e);
        }

        private static TreeViewItem FindTreeViewAncestor(DependencyObject current)
        {
            if (current == null)
                return null;

            var treeViewItem = current as TreeViewItem;
            if (treeViewItem != null)
                return treeViewItem;

            return FindTreeViewAncestor(VisualTreeHelper.GetParent(current));
        }

        private void LaunchRDPEdit(string path)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Constants.Mstsc,
                Arguments = $"/edit \"{path}\""
            };

            Process.Start(startInfo);
        }

        private void SetDialogPosition(AddDialog dialog, Button button)
        {
            var buttonLocation = GetElementPoint(button);
            dialog.Owner = this;
            dialog.Left = Left + buttonLocation.X + button.ActualWidth + 7;

            dialog.Top = Top + buttonLocation.Y + HORRIBLE_CONSTANT;
        }

        private Point GetElementPoint(Visual element)
        {
            return element.TransformToAncestor(this)
                .Transform(new Point(0, 0));
        }

        private ApplicationState GetApplicationState()
        {
            return new ApplicationState {ExpandedPaths = _treeViewManager.GetExpandedFolders()};
        }

        private ApplicationState LoadApplicationState()
        {
            try
            {
                var json = File.ReadAllText("RDerP.json");
                return JsonConvert.DeserializeObject<ApplicationState>(json);
            }
            catch
            {
                //if we fail we care not, just give an empty buggery back
                return new ApplicationState();
            }
        }
    }
}
