// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Converts a <see cref="DateTime"/> to and from the ISO 8601 date format (e.g. <c>"2008-04-12T12:53Z"</c>).
/// </summary>
public class IsoDateTimeConverter : DateTimeConverterBase
{
    const string DefaultDateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";

    string? dateTimeFormat;
    CultureInfo? culture;

    /// <summary>
    /// Gets or sets the date time styles used when converting a date to and from JSON.
    /// </summary>
    public DateTimeStyles DateTimeStyles { get; set; } = DateTimeStyles.RoundtripKind;

    /// <summary>
    /// Gets or sets the date time format used when converting a date to and from JSON.
    /// </summary>
    public string? DateTimeFormat
    {
        get => dateTimeFormat ?? string.Empty;
        set => dateTimeFormat = StringUtils.IsNullOrEmpty(value) ? null : value;
    }

    /// <summary>
    /// Gets or sets the culture used when converting a date to and from JSON.
    /// </summary>
    public CultureInfo Culture
    {
        get => culture ?? CultureInfo.CurrentCulture;
        set => culture = value;
    }

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        string text;

        if (value is DateTime dateTime)
        {
            if ((DateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal
                || (DateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
            {
                dateTime = dateTime.ToUniversalTime();
            }

            text = dateTime.ToString(dateTimeFormat ?? DefaultDateTimeFormat, Culture);
        }
        else if (value is DateTimeOffset dateTimeOffset)
        {
            if ((DateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal
                || (DateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
            {
                dateTimeOffset = dateTimeOffset.ToUniversalTime();
            }

            text = dateTimeOffset.ToString(dateTimeFormat ?? DefaultDateTimeFormat, Culture);
        }
        else
        {
            throw new JsonSerializationException($"Unexpected value when converting date. Expected DateTime or DateTimeOffset, got {ReflectionUtils.GetObjectType(value)!}.");
        }

        writer.WriteValue(text);
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    public override object? ReadJson(JsonReader reader, Type type, object? existingValue, JsonSerializer serializer)
    {
        var nullable = type.IsNullableType();
        if (reader.TokenType == JsonToken.Null)
        {
            if (!nullable)
            {
                throw JsonSerializationException.Create(reader, $"Cannot convert null value to {type}.");
            }

            return null;
        }

        var t = nullable
            ? Nullable.GetUnderlyingType(type)
            : type;

        if (reader.TokenType == JsonToken.Date)
        {
            if (t == typeof(DateTimeOffset))
            {
                return reader.Value is DateTimeOffset ? reader.Value : new DateTimeOffset((DateTime)reader.Value!);
            }

            // converter is expected to return a DateTime
            if (reader.Value is DateTimeOffset offset)
            {
                return offset.DateTime;
            }

            return reader.Value;
        }

        if (reader.TokenType != JsonToken.String)
        {
            throw JsonSerializationException.Create(reader, $"Unexpected token parsing date. Expected String, got {reader.TokenType}.");
        }

        var dateText = reader.Value?.ToString()!;

        if (StringUtils.IsNullOrEmpty(dateText) && nullable)
        {
            return null;
        }

        if (t == typeof(DateTimeOffset))
        {
            if (StringUtils.IsNullOrEmpty(dateTimeFormat))
            {
                return DateTimeOffset.Parse(dateText, Culture, DateTimeStyles);
            }

            return DateTimeOffset.ParseExact(dateText, dateTimeFormat, Culture, DateTimeStyles);
        }

        if (StringUtils.IsNullOrEmpty(dateTimeFormat))
        {
            return DateTime.Parse(dateText, Culture, DateTimeStyles);
        }

        return DateTime.ParseExact(dateText, dateTimeFormat, Culture, DateTimeStyles);
    }
}