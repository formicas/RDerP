﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private readonly string _executingDirectory;
        private ApplicationState _initialState;

        public MainWindow()
        {
            InitializeComponent();

            _initialState = LoadApplicationState();

            _executingDirectory = Directory.GetCurrentDirectory();

            AddRootToTreeView();
        }

        private void AddRootToTreeView()
        {
            var subDirPaths = Directory.GetDirectories(_executingDirectory);

            foreach (var sub in subDirPaths)
            {
                var folderItem = new FolderTreeViewItem(CreateDirectoryHeader(Path.GetFileName(sub)), sub)
                {
                    IsExpanded = _initialState.ExpandedPaths.Contains(sub)
                };
                rdpTree.Items.Add(folderItem);
                AddChildren(folderItem);
            }

            var filePaths = Directory.GetFiles(_executingDirectory, "*.rdp");

            foreach (var filePath in filePaths)
            {
                rdpTree.Items.Add(CreateRdpItem(filePath));
            }
        }

        private void AddChildren(FolderTreeViewItem parent)
        {
            var dirPath = parent.Path;
            
            var subDirPaths = Directory.GetDirectories(dirPath);
            foreach (var sub in subDirPaths)
            {
                var newItem = new FolderTreeViewItem(CreateDirectoryHeader(Path.GetFileName(sub)), sub)
                {
                    IsExpanded = _initialState.ExpandedPaths.Contains(sub)
                };
                parent.Items.Add(newItem);
                AddChildren(newItem);
            }

            var filePaths = Directory.GetFiles(dirPath, "*.rdp");
            foreach(var filePath in filePaths)
            {
                parent.Items.Add(CreateRdpItem(filePath));
            }
        }

        private RdpTreeViewItem CreateRdpItem(string path)
        {
            var name = Path.GetFileName(path)?.Replace(".rdp", "");
            var item = new RdpTreeViewItem(CreateRdpHeader(name), path);
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

        private void OnRdpDoubleClick(object sender, MouseButtonEventArgs args)
        {
            var rdpItem = sender as RdpTreeViewItem;
            if (rdpItem == null) return;

            LaunchRDPSession(rdpItem.Path);
        }

        private void LaunchRDPSession(string path)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = MSTSC,
                Arguments = $"\"{path}\""
            };
            Process.Start(startInfo);
        }

        private void LaunchRDPEdit(string path)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = MSTSC,
                Arguments = $"/edit \"{path}\""
            };

            Process.Start(startInfo);
        }

        private void AddRdpItem(object sender, RoutedEventArgs e)
        {
            _initialState = GetApplicationState();
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

                    if (folderItem != null)
                    {
                        folderItem.Items.Clear();
                        AddChildren(folderItem);
                    }
                    else
                    {
                        rdpTree.Items.Clear();
                        AddRootToTreeView();
                    }

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
                //swallow the shit out of this bad boy - nobody likes seeing errors on close
            }

            base.OnClosing(e);
        }

        private void SaveState()
        {
            var json = JsonConvert.SerializeObject(GetApplicationState());
            File.WriteAllText("RDerP.json", json);
        }

        private ApplicationState GetApplicationState()
        {
            return new ApplicationState {ExpandedPaths = GetExpandedFolders()};
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
