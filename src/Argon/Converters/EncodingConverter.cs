namespace Argon;

public class EncodingConverter :
    JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var info = (Encoding) value;
        writer.WriteValue(info.WebName);
    }

    public override object? ReadJson(JsonReader reader, Type type, object? existingValue, JsonSerializer serializer)
    {
        if (reader.Value is string value)
        {
            return Encoding.GetEncoding(value);
        }

        return null;
    }

    public override bool CanConvert(Type type) =>
        typeof(Encoding).IsAssignableFrom(type);
}