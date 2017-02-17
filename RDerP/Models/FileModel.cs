namespace RDerP.Models
{
    class FileModel
    {
        public string Name { get; }
        public string Path { get; }

        public FileModel(string path)
        {
            Path = path;
            Name = System.IO.Path.GetFileName(path);
        }
    }
}
