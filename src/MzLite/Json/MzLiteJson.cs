#region license
// The MIT License (MIT)

// MzLiteJson.cs

// Copyright (c) 2016 Alexander Lüdemann
// alexander.luedemann@outlook.com
// luedeman@rhrk.uni-kl.de

// Computational Systems Biology, Technical University of Kaiserslautern, Germany
 

// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

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
