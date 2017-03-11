using System.IO;
using Newtonsoft.Json;
using RDerP.Models;

namespace RDerP.IO
{
    public class FileHelper
    {
        public static void GenerateRdpFile(string path, string host)
        {
            var content = $"full address:s:{host}";
            File.WriteAllText(path, content);
        }

        public static void SaveState(ApplicationState appState)
        {
            var json = JsonConvert.SerializeObject(appState);
            File.WriteAllText("RDerP.json", json);
        }
    }
}
