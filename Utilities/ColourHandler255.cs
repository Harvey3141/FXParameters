using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace FX
{
    public class ColourHandler255 : JsonConverter
    {
        public ColourHandler255()
        {
        }
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Color);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject item = JObject.Load(reader);
            if (typeof(Color).IsAssignableFrom(objectType))
            {
                return new Color(
                    (float)item["r"] / 255f,
                    (float)item["g"] / 255f,
                    (float)item["b"] / 255f,
                    (float)item["a"]
                );
            }
            throw new JsonSerializationException("Cannot convert type");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Color color)
            {
                JObject jColor = new JObject {
                    { "r", (int)(color.r * 255) },
                    { "g", (int)(color.g * 255) },
                    { "b", (int)(color.b * 255) },
                    { "a", (int)(color.a)}
                };
                jColor.WriteTo(writer);
            }
        }
    }

}
