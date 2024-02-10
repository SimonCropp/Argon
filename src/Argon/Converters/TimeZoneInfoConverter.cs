class TimeZoneInfoConverter :
    JsonConverter<TimeZoneInfo>
{
    public override void WriteJson(JsonWriter writer, TimeZoneInfo value, JsonSerializer serializer) =>
        writer.WriteValue(value.Id);

    public override TimeZoneInfo? ReadJson(JsonReader reader, Type type, TimeZoneInfo? existingValue, bool hasExisting, JsonSerializer serializer)
    {
        if (reader.Value is string value)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(value);
        }

        return null;
    }
}