using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace RDerP.ViewModels
{
    class DirectoryViewModel
    {
        public ReadOnlyCollection<DirectoryViewModel> SubDirectories { get; }
        public ReadOnlyCollection<FileViewModel> Files { get; }
        public string Name { get; }
        public string Path { get; }

        public DirectoryViewModel(string path)
        {
            Path = path;
            var subDirPaths = System.IO.Directory.GetDirectories(path);
            var filePaths = System.IO.Directory.GetFiles(path, "*.rdp");
            Name = System.IO.Path.GetFileName(path);

            SubDirectories = new ReadOnlyCollection<DirectoryViewModel>(subDirPaths.Select(s => new DirectoryViewModel(s)).ToList());
            Files = new ReadOnlyCollection<FileViewModel>(filePaths.Select(f => new FileViewModel(f)).ToList());
        }
    }
}
