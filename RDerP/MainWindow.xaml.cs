using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RDerP.Properties;
using RDerP.ViewModels;

namespace RDerP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var rootDir=Settings.Default.BaseDirectory;
            var root= new DirectoryViewModel("C:\\dev\\RDP");

            AddItemToTreeView(root, null);

            //base.DataContext = viewModel;
        }

        private void AddItemToTreeView(DirectoryViewModel item, TreeViewItem parent)
        {
            var newItem = new TreeViewItem {Header = CreateDirectoryHeader(item.Name)};
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
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "mstsc.exe";
            startInfo.Arguments = $"\"{path}\"";
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
    }
}
