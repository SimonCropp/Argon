namespace Argon;

public class StringWriterConverter :
    JsonConverter<StringWriter>
{
    public override void WriteJson(JsonWriter writer, StringWriter value, JsonSerializer serializer) =>
        writer.WriteValue(value.ToString());

    public override StringWriter ReadJson(JsonReader reader, Type type, StringWriter? existingValue, bool hasExisting, JsonSerializer serializer) =>
        new(new StringBuilder(reader.StringValue));
}