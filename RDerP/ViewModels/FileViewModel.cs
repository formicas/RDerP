using System.Windows.Controls;

namespace RDerP.ViewModels
{
    class FileViewModel
    {
        public string Name { get; }
        public string Path { get; }

        public FileViewModel(string path)
        {
            Path = path;
            Name = System.IO.Path.GetFileName(path);
        }
    }
}
