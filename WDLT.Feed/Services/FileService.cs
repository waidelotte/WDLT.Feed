using System.IO;
using Newtonsoft.Json;

namespace WDLT.Feed.Services
{
    public static class FileService
    {
        public static T Deserialize<T>(string path)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
        }

        public static void Serialize(object data, string path, string fileName)
        {
            File.WriteAllText(Path.Combine(path, fileName + ".json"), JsonConvert.SerializeObject(data));
        }
    }
}