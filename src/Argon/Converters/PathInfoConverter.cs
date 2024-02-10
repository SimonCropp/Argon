// ReSharper disable NullableWarningSuppressionIsUsed
// ReSharper disable RedundantSuppressNullableWarningExpression
class PathInfoConverter :
    JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
        writer.WriteValue(value.ToString()!.Replace('\\', '/'));

    public override object? ReadJson(JsonReader reader, Type type, object? existingValue, JsonSerializer serializer)
    {
        if (reader.Value is not string value)
        {
            return null;
        }

        var path = value.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

        if (type == typeof(DirectoryInfo))
        {
            return new DirectoryInfo(path);
        }

        return new FileInfo(path);
    }

    public override bool CanConvert(Type type) =>
        type == typeof(FileInfo) ||
        type == typeof(DirectoryInfo);
}