namespace Argon;

public class EncodingConverter :
    JsonConverter<Encoding>
{
    public override void WriteJson(JsonWriter writer, Encoding value, JsonSerializer serializer) =>
        writer.WriteValue(value.WebName);

    public override Encoding? ReadJson(JsonReader reader, Type type, Encoding? existingValue, bool hasExisting, JsonSerializer serializer)
    {
        if (reader.Value is string value)
        {
            return Encoding.GetEncoding(value);
        }

        return null;
    }
}