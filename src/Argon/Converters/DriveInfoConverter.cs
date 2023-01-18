class DriveInfoConverter :
    JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value is DriveInfo info)
        {
            writer.WriteValue(info.Name.Replace('\\', '/'));
        }
    }

    public override object? ReadJson(JsonReader reader, Type type, object? existingValue, JsonSerializer serializer)
    {
        if (reader.Value is string value)
        {
            return new DriveInfo(value);
        }

        return null;
    }

    public override bool CanConvert(Type type) =>
        type == typeof(DriveInfo);
}