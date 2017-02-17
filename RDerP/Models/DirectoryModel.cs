using System.Collections.ObjectModel;
using System.Linq;

namespace RDerP.Models
{
    class DirectoryModel
    {
        public ReadOnlyCollection<DirectoryModel> SubDirectories { get; }
        public ReadOnlyCollection<FileModel> Files { get; }
        public string Name { get; }
        public string Path { get; }

        public DirectoryModel(string path)
        {
            Path = path;
            var subDirPaths = System.IO.Directory.GetDirectories(path);
            var filePaths = System.IO.Directory.GetFiles(path, "*.rdp");
            Name = System.IO.Path.GetFileName(path);

            SubDirectories = new ReadOnlyCollection<DirectoryModel>(subDirPaths.Select(s => new DirectoryModel(s)).ToList());
            Files = new ReadOnlyCollection<FileModel>(filePaths.Select(f => new FileModel(f)).ToList());
        }
    }
}
