using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        private ApplicationState _initialState;
        private readonly TreeViewManager _treeViewManager;

        public MainWindow()
        {
            InitializeComponent();

            _initialState = LoadApplicationState();

            _executingDirectory = Directory.GetCurrentDirectory();
            _treeViewManager = new TreeViewManager(rdpTree, _executingDirectory);

            _treeViewManager.AddRootToTreeView(_initialState);

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
                    _treeViewManager.AddRootToTreeView(_initialState);
                    return;
                }

                var folderItem = _treeViewManager.GetFolderItemForPath(parent);

                if (folderItem != null)
                {
                    _treeViewManager.AddChildren(folderItem, _initialState);
                }
            });
        }

        private void AddRdpItem(object sender, RoutedEventArgs e)
        {
            _initialState = GetApplicationState();
            //get the parent treeviewitem
            var folderItem = _treeViewManager.GetCurrentParent();

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
            _initialState = GetApplicationState();

            //get the parent treeviewitem
            var folderItem = _treeViewManager.GetCurrentParent();

            var directoryPath = folderItem != null ? folderItem.Path : _executingDirectory;

            var dialog = new AddDialog(directoryPath, ItemType.Folder);

            SetDialogPosition(dialog, addFolder);

            dialog.ShowDialog();
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
