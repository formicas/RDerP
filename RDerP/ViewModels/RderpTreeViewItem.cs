using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace RDerP.ViewModels
{
    public class RderpTreeViewItem : TreeViewItem
    {
        public string FullPath { get; }

        public RderpTreeViewItem(string path)
        {
            FullPath = path;
        }

        protected StackPanel CreateHeader(string name, string imagePath)
        {
            var image = new Image { Source = new BitmapImage(new Uri(imagePath)) };
            var label = new Label { Content = name };

            var stack = new StackPanel { Orientation = Orientation.Horizontal };

            stack.Children.Add(image);
            stack.Children.Add(label);

            return stack;
        }
    }
}

