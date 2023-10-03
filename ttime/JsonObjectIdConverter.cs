using System;
using LiteDB;
using Newtonsoft.Json;
using JsonReader = Newtonsoft.Json.JsonReader;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
using JsonWriter = Newtonsoft.Json.JsonWriter;

namespace ttime;

public class JsonObjectIdConverter : JsonConverter<ObjectId>
{
    public override ObjectId ReadJson(
        JsonReader reader,
        Type objectType,
        ObjectId existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType != JsonToken.String)
        {
            var jsonLineInfo = (IJsonLineInfo) reader;
            throw new JsonSerializationException(
                $"Unexpected token parsing ObjectId. Expected String, got {reader.TokenType}.",
                reader.Path,
                jsonLineInfo.LineNumber,
                jsonLineInfo.LinePosition,
                null);
        }

        var sval = reader.Value?.ToString();
        return string.IsNullOrEmpty(sval) ? null : new ObjectId(sval);
    }

    public override void WriteJson(JsonWriter writer, ObjectId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}