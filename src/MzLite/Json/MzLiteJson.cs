using System.Globalization;
using System.IO;
using MzLite.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MzLite.Json
{

    public static class MzLiteJson
    {

        static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver(),
            Culture = new CultureInfo("en-US")            
        };

        public static void SaveModel(MzLiteModel model, string path)
        {
            if (File.Exists(path))
                File.Delete(path);

            using (StreamWriter writer = File.CreateText(path))
            using (JsonWriter jsonWriter = new JsonTextWriter(writer))
            {
                JsonSerializer serializer = JsonSerializer.Create(jsonSettings);
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(jsonWriter, model);
            }
        }

        public static MzLiteModel LoadModel(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(path);
            using (StreamReader reader = File.OpenText(path))
            using (JsonReader jsonReader = new JsonTextReader(reader))
            {
                JsonSerializer serializer = JsonSerializer.Create(jsonSettings);
                return serializer.Deserialize<MzLiteModel>(jsonReader);
            }
        }

        public static string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, jsonSettings);
        }
    }
}
