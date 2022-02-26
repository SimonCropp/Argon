// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Converts a <see cref="DateTime"/> to and from a JavaScript <c>Date</c> constructor (e.g. <c>new Date(52231943)</c>).
/// </summary>
public class JavaScriptDateTimeConverter : DateTimeConverterBase
{
    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        long ticks;

        if (value is DateTime dateTime)
        {
            var utcDateTime = dateTime.ToUniversalTime();
            ticks = DateTimeUtils.ConvertDateTimeToJavaScriptTicks(utcDateTime);
        }
        else if (value is DateTimeOffset dateTimeOffset)
        {
            var utcDateTimeOffset = dateTimeOffset.ToUniversalTime();
            ticks = DateTimeUtils.ConvertDateTimeToJavaScriptTicks(utcDateTimeOffset.UtcDateTime);
        }
        else
        {
            throw new JsonSerializationException("Expected date object value.");
        }

        writer.WriteStartConstructor("Date");
        writer.WriteValue(ticks);
        writer.WriteEndConstructor();
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    public override object? ReadJson(JsonReader reader, Type type, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            if (!type.IsNullable())
            {
                throw JsonSerializationException.Create(reader, $"Cannot convert null value to {type}.");
            }

            return null;
        }

        if (reader.TokenType != JsonToken.StartConstructor || !string.Equals(reader.Value?.ToString(), "Date", StringComparison.Ordinal))
        {
            throw JsonSerializationException.Create(reader, $"Unexpected token or value when parsing date. Token: {reader.TokenType}, Value: {reader.Value}");
        }

        if (!JavaScriptUtils.TryGetDateFromConstructorJson(reader, out var d, out var errorMessage))
        {
            throw JsonSerializationException.Create(reader, errorMessage);
        }

        var t = type.IsNullableType()
            ? Nullable.GetUnderlyingType(type)
            : type;
        if (t == typeof(DateTimeOffset))
        {
            return new DateTimeOffset(d);
        }
        return d;
    }
}