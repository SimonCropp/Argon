class DriveInfoConverter :
    JsonConverter<DriveInfo>
{
    public override void WriteJson(JsonWriter writer, DriveInfo? value, JsonSerializer serializer)
    {
        if (value != null)
        {
            writer.WriteValue(value.Name.Replace('\\', '/'));
        }
    }

    public override DriveInfo? ReadJson(JsonReader reader, Type type, DriveInfo? existingValue, bool hasExisting, JsonSerializer serializer)
    {
        if (reader.Value is string value)
        {
            return new(value);
        }

        return null;
    }
}