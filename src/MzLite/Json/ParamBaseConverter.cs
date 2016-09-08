using System;
using MzLite.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MzLite.Json
{
    public sealed class ParamBaseConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            throw new NotSupportedException("JsonConverter.CanConvert()");
        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken jt = JToken.Load(reader);

            if (jt.Type == JTokenType.Null)
            {
                return null;
            }
            else if (jt.Type == JTokenType.Object)
            {

                JObject jo = (JObject)jt;
                JToken jtval;

                if (jo.TryGetValue("Name", out jtval))
                {
                    return jo.ToObject<UserParam>(serializer);
                }
                else if (jo.TryGetValue("CvAccession", out jtval))
                {
                    return jo.ToObject<CvParam>(serializer);
                }
                else
                {
                    throw new JsonSerializationException("Could not determine concrete param type.");
                }

            }
            else
            {
                throw new JsonSerializationException("Object token expected.");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else if (value is CvParam)
            {
                serializer.Serialize(writer, value);
            }
            else if (value is UserParam)
            {
                serializer.Serialize(writer, value);
            }
            else
            {
                throw new JsonSerializationException("Type not supported: " + value.GetType().FullName);
            }
        }
    }
}
