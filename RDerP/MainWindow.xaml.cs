using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        private const string MSTSC = "mstsc.exe";
        private DirectoryModel _root;
        private readonly string _executingDirectory;
        private readonly ApplicationState _initialState;

        public MainWindow()
        {
            InitializeComponent();

            _initialState = LoadApplicationState();

            _executingDirectory = Directory.GetCurrentDirectory();
            _root = new DirectoryModel(_executingDirectory);

            AddRootToTreeView(_root);
        }

        private void AddRootToTreeView(DirectoryModel root)
        {
            foreach (var sub in root.SubDirectories)
            {
                AddItemToTreeView(sub, null);
            }

            foreach (var file in root.Files)
            {
                rdpTree.Items.Add(CreateRdpItem(file.Name, file.Path));
            }
        }

        private void AddItemToTreeView(DirectoryModel item, TreeViewItem parent)
        {
            //var newItem = new TreeViewItem {Header = CreateDirectoryHeader(item.Name)};
            var newItem = new FolderTreeViewItem(CreateDirectoryHeader(item.Name), item.Path)
            {
                IsExpanded = _initialState.ExpandedPaths.Contains(item.Path)
            };

            foreach (var sub in item.SubDirectories)
            {
                AddItemToTreeView(sub, newItem);
            }
            foreach (var file in item.Files)
            {
                newItem.Items.Add(CreateRdpItem(file.Name, file.Path));
            }

            if (parent != null)
                parent.Items.Add(newItem);
            else
                rdpTree.Items.Add(newItem);
        }

        private void OnRdpDoubleClick(object sender, MouseButtonEventArgs args)
        {
            var rdpItem = sender as RdpTreeViewItem;
            if (rdpItem == null) return;

            LaunchRDPSession(rdpItem.Path);
        }

        private void LaunchRDPSession(string path)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = MSTSC,
                Arguments = $"\"{path}\""
            };
            Process.Start(startInfo);
        }

        private void LaunchRDPEdit(string path)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = MSTSC,
                Arguments = $"/edit \"{path}\""
            };

            Process.Start(startInfo);
        }

        private RdpTreeViewItem CreateRdpItem(string name, string path)
        {
            var item = new RdpTreeViewItem(CreateRdpHeader(name.Replace(".rdp", "")), path);
            item.MouseDoubleClick += OnRdpDoubleClick;
            return item;
        }

        private StackPanel CreateDirectoryHeader(string name)
        {
            return CreateHeader(name, Properties.Resources.FolderIcon);
        }

        private StackPanel CreateRdpHeader(string name)
        {
            return CreateHeader(name, Properties.Resources.TerminalIcon);
        }

        private StackPanel CreateHeader(string name, string imagePath)
        {
            var image = new Image {Source = new BitmapImage(new Uri(imagePath))};
            var label = new Label {Content = name};

            var stack = new StackPanel {Orientation = Orientation.Horizontal};

            stack.Children.Add(image);
            stack.Children.Add(label);

            return stack;
        }

        private void AddRdpItem(object sender, RoutedEventArgs e)
        {
            //get the parent treeviewitem
            FolderTreeViewItem folderItem = null;
            var item = rdpTree.SelectedItem;
            if (item != null)
            {
                var rdpItem = item as RdpTreeViewItem;
                if (rdpItem != null)
                {
                    folderItem = rdpItem.Parent as FolderTreeViewItem;
                }
                else
                {
                    folderItem = item as FolderTreeViewItem;
                }
            }

            var directoryPath = folderItem != null ? folderItem.Path : _executingDirectory;

            var dialog = new AddDialog(directoryPath);

            var addLocation = GetElementPoint(add);
            dialog.Owner = this;
            dialog.Left = Left + addLocation.X + add.ActualWidth + 7;

            dialog.Top = Top + addLocation.Y + HORRIBLE_CONSTANT;

            var result = dialog.ShowDialog();
            if (result.HasValue&&result.Value)
            {
                var name = dialog.NewName;
                var host = dialog.Host;


                var filePath = Path.Combine(directoryPath, $"{name}.rdp");

                if (!File.Exists(filePath))
                {
                    FileHelper.GenerateRdpFile(filePath, host);

                    var newRdpItem = CreateRdpItem(name, filePath);

                    if (folderItem != null)
                    {
                        folderItem.Items.Add(newRdpItem);
                        folderItem.Items.Refresh();
                    }
                    else
                    {
                        rdpTree.Items.Add(newRdpItem);
                        rdpTree.Items.Refresh();
                    }
                    
                    _root = new DirectoryModel(_executingDirectory);

                    LaunchRDPEdit(filePath);
                }
                else
                {
                    //todo handle
                }
            }
        }

        private Point GetElementPoint(Visual element)
        {
            return element.TransformToAncestor(this)
                .Transform(new Point(0, 0));
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //on close, we want to find all the expanded folders and write them to our 'persistence' file
            try
            {
                SaveState();
            }
            catch
            {
                //swallow the shit out of this bad boy
            }

            base.OnClosing(e);
        }

        private void SaveState()
        {
            var appState = new ApplicationState {ExpandedPaths = GetExpandedFolders()};
            var json = JsonConvert.SerializeObject(appState);
            File.WriteAllText("RDerP.json", json);
        }

        private IEnumerable<string> GetExpandedFolders()
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
                    rdpTree.Items.Cast<TreeViewItem>()
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
