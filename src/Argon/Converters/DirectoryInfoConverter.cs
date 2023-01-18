class DirectoryInfoConverter :
    JsonConverter<DirectoryInfo>
{
    public override void WriteJson(JsonWriter writer, DirectoryInfo? value, JsonSerializer serializer)
    {
        if (value != null)
        {
            writer.WriteValue(value.ToString().Replace('\\', '/'));
        }
    }

    public override DirectoryInfo? ReadJson(JsonReader reader, Type type, DirectoryInfo? existingValue, bool hasExisting, JsonSerializer serializer)
    {
        if (reader.Value is string value)
        {
            return new(value.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
        }

        return null;
    }
}