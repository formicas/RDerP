using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RDerP.ViewModels
{
    class TreeViewItemModelBase:TreeViewItem
    {
        private bool isSelected;
        private bool isExpanded;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    OnPropertyChanged(new DependencyPropertyChangedEventArgs());
                }
            }
        }
    }
}
