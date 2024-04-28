using System;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace FX
{
    public class ColourHandler : JsonConverter
    {
        public ColourHandler()
        {
        }


        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Color) || objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(FXParameterData<Color>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject item = JObject.Load(reader);
            if (typeof(Color).IsAssignableFrom(objectType))
            {
                return new Color(
                    (float)item["r"],
                    (float)item["g"],
                    (float)item["b"],
                    (float)item["a"]
                );
            }
            else if (typeof(FXParameterData<Color>).IsAssignableFrom(objectType))
            {
                FXParameterData<Color> data = new FXParameterData<Color>();
                data.key                    = (string)item["key"];
                data.value                  = item["value"].ToObject<Color>(serializer);
                data.defaultValue           = item["defaultValue"].ToObject<Color>(serializer);
                data.minValue               = item["minValue"].ToObject<Color>(serializer);
                data.maxValue               = item["maxValue"].ToObject<Color>(serializer);
                data.hasMinValue            = (bool)item["hasMinValue"];
                data.hasMaxValue            = (bool)item["hasMaxValue"];
                data.isScaled               = (bool)item["isScaled"];
                data.affector               = (AffectorFunction)(int)item["affector"];
                data.isInverted             = (bool)item["isInverted"];
                return data;
            }
            throw new JsonSerializationException("Cannot convert type");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Color color)
            {
            JObject jColor = new JObject {
            { "r", color.r },
            { "g", color.g },
            { "b", color.b },
            { "a", color.a }
            };
                jColor.WriteTo(writer);
            }
            else if (value is FXParameterData<Color> param)
            {
                JObject jObj = new JObject {
            { "key", param.key },
            { "value", JToken.FromObject(param.value, serializer) },
            { "defaultValue", JToken.FromObject(param.defaultValue, serializer) },
            { "minValue", JToken.FromObject(param.minValue, serializer) },
            { "maxValue", JToken.FromObject(param.maxValue, serializer) },
            { "hasMinValue", param.hasMinValue },
            { "hasMaxValue", param.hasMaxValue },
            { "isScaled", param.isScaled },
            { "affector", (int)param.affector },
            { "isInverted", param.isInverted }
        };
                jObj.WriteTo(writer);
            }
        }

    }

}

