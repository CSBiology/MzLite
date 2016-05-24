using System;
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

        public static void SaveJsonFile(object obj, string path)
        {
            if (File.Exists(path))
                File.Delete(path);

            using (StreamWriter writer = File.CreateText(path))
            using (JsonWriter jsonWriter = new JsonTextWriter(writer))
            {
                JsonSerializer serializer = JsonSerializer.Create(jsonSettings);
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(jsonWriter, obj);
            }
        }

        public static T ReadJsonFile<T>(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            using (StreamReader reader = File.OpenText(path))
            using (JsonReader jsonReader = new JsonTextReader(reader))
            {
                JsonSerializer serializer = JsonSerializer.Create(jsonSettings);
                return serializer.Deserialize<T>(jsonReader);
            }
        }

        public static string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, jsonSettings);
        }

        public static T FromJson<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentNullException("json");
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T JsonCloneModelItem<T>(string newID, T obj) where T : ModelItem
        {
            T cloned = FromJson<T>(ToJson(obj));
            cloned.ID = newID;
            return cloned;
        }

        public static T JsonCloneNamedItem<T>(string newName, T obj) where T : NamedItem
        {
            T cloned = FromJson<T>(ToJson(obj));
            cloned.Name = newName;
            return cloned;
        }
    }
}
