// Copyright 2012 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NodaTime.Calendars;

/// <summary>
/// Json.NET converter for <see cref="YearMonth"/> using a compound representation.
/// </summary>
sealed class NodaYearMonthConverter :
    NodaConverterBase<YearMonth>
{
    /// <summary>
    /// Reads properties of a YearMonth, converting them to YearMonth
    /// using the given serializer.
    /// </summary>
    /// <param name="reader">The JSON reader to fetch data from.</param>
    /// <param name="serializer">The serializer for embedded serialization.</param>
    /// <returns>The <see cref="Interval"/> identified in the JSON.</returns>
    protected override YearMonth ReadJsonImpl(JsonReader reader, JsonSerializer serializer)
    {
        Era? era = null;
        int? year = null;
        int? month = null;
        CalendarSystem? calendar = null;
        while (reader.Read())
        {
            if (reader.TokenType != JsonToken.PropertyName)
            {
                break;
            }

            var propertyName = reader.StringValue;
            // If we haven't got a property value, that's pretty weird. Break out of the loop,
            // and let JSON.NET fail appropriately...
            if (!reader.Read())
            {
                break;
            }

            var eraPropertyName = serializer.ResolvePropertyName(nameof(YearMonth.Era));
            if (string.Equals(propertyName, eraPropertyName, StringComparison.OrdinalIgnoreCase))
            {
                era = serializer.TryDeserialize<Era>(reader);
            }

            var yearPropertyName = serializer.ResolvePropertyName(nameof(YearMonth.Year));
            if (string.Equals(propertyName, yearPropertyName, StringComparison.OrdinalIgnoreCase))
            {
                year = serializer.TryDeserialize<int>(reader);
            }

            var monthPropertyName = serializer.ResolvePropertyName(nameof(YearMonth.Month));
            if (string.Equals(propertyName, monthPropertyName, StringComparison.OrdinalIgnoreCase))
            {
                month = serializer.TryDeserialize<int>(reader);
            }

            var calendarPropertyName = serializer.ResolvePropertyName(nameof(YearMonth.Calendar));
            if (string.Equals(propertyName, calendarPropertyName, StringComparison.OrdinalIgnoreCase))
            {
                calendar = serializer.TryDeserialize<CalendarSystem>(reader);
            }
        }

        if (era == null)
        {
            throw new SerializationException("Expected property for YearMonth `era` is missing.");
        }

        if (year == null)
        {
            throw new SerializationException("Expected property for YearMonth `year` is missing.");
        }

        if (month == null)
        {
            throw new SerializationException("Expected property for YearMonth `month` is missing.");
        }

        if (calendar == null)
        {
            throw new SerializationException("Expected property for YearMonth `calendar` is missing.");
        }

        return new(era, year.Value, month.Value, calendar);
    }

    /// <summary>
    /// Serializes the YearMonth.
    /// </summary>
    /// <param name="writer">The writer to write JSON to</param>
    /// <param name="value">The interval to serialize</param>
    /// <param name="serializer">The serializer for embedded serialization.</param>
    protected override void WriteJsonImpl(JsonWriter writer, YearMonth value, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        var eraPropertyName = serializer.ResolvePropertyName(nameof(YearMonth.Era));
        writer.WritePropertyName(eraPropertyName);
        serializer.Serialize(writer, value.Era);

        var yearPropertyName = serializer.ResolvePropertyName(nameof(YearMonth.Year));
        writer.WritePropertyName(yearPropertyName);
        serializer.Serialize(writer, value.Year);

        var monthPropertyName = serializer.ResolvePropertyName(nameof(YearMonth.Month));
        writer.WritePropertyName(monthPropertyName);
        serializer.Serialize(writer, value.Month);

        var calendarPropertyName = serializer.ResolvePropertyName(nameof(YearMonth.Calendar));
        writer.WritePropertyName(calendarPropertyName);
        serializer.Serialize(writer, value.Calendar);

        writer.WriteEndObject();
    }
}