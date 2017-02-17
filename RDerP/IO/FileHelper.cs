using System.IO;

namespace RDerP.IO
{
    public class FileHelper
    {
        public static void GenerateRdpFile(string path, string host)
        {
            var content = $"full address:s:{host}";
            File.WriteAllText(path, content);
        }
    }
}
