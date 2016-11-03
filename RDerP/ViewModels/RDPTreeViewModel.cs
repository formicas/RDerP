using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RDerP.ViewModels
{
    class RDPTreeViewModel
    {
        private DirectoryViewModel RootDirectory { get; }

        public RDPTreeViewModel(DirectoryViewModel root)
        {
            RootDirectory = root;
        }
    }
}
