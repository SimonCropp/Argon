class TimeZoneInfoConverter :
    JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var info = (TimeZoneInfo) value;
        writer.WriteValue(info.Id);
    }

    public override object? ReadJson(JsonReader reader, Type type, object? existingValue, JsonSerializer serializer)
    {
        if (reader.Value is string value)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(value);
        }

        return null;
    }

    public override bool CanConvert(Type type) =>
        type == typeof(TimeZoneInfo);
}