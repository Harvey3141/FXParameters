using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

public class SerializedTagConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(FX.FXManager.SerializedTag).IsAssignableFrom(objectType);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value is FX.FXManager.SerializedTag tag)
        {
            JObject jsonObject = new JObject
            {
                { "name", tag.Name },
                { "type", tag.Type },
                { "value", JToken.FromObject(tag.Value, serializer) }
            };

            jsonObject.WriteTo(writer);
        }
        else
        {
            throw new InvalidCastException($"Cannot cast {value.GetType()} to SerializedTag");
        }
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jsonObject = JObject.Load(reader);

        string name = jsonObject["name"].Value<string>();
        string type = jsonObject["type"].Value<string>();
        JToken valueToken = jsonObject["value"];

        object value = type switch
        {
            "string" => valueToken.ToObject<string>(serializer),
            "int32" => valueToken.ToObject<int>(serializer),
            "single" => valueToken.ToObject<float>(serializer),
            "boolean" => valueToken.ToObject<bool>(serializer),
            _ => throw new NotImplementedException($"Unsupported tag type: {type}"),
        };

        return new FX.FXManager.SerializedTag
        {
            Name = name,
            Type = type,
            Value = value
        };
    }
}
