class FileInfoConverter :
    JsonConverter<FileInfo>
{
    public override void WriteJson(JsonWriter writer, FileInfo? value, JsonSerializer serializer)
    {
        if (value != null)
        {
            writer.WriteValue(value.ToString().Replace('\\', '/'));
        }
    }

    public override FileInfo? ReadJson(JsonReader reader, Type type, FileInfo? existingValue, bool hasExisting, JsonSerializer serializer)
    {
        if (reader.Value is string value)
        {
            return new(value.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
        }

        return null;
    }
}