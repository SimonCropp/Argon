// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Converts a <see cref="Version"/> to and from a string (e.g. <c>"1.2.3.4"</c>).
/// </summary>
public class VersionConverter : JsonConverter
{
    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        if (value is Version)
        {
            writer.WriteValue(value.ToString());
            return;
        }

        throw new JsonSerializationException("Expected Version object value");
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    public override object? ReadJson(JsonReader reader, Type type, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonToken.String)
        {
            try
            {
                return new Version((string)reader.Value!);
            }
            catch (Exception ex)
            {
                throw JsonSerializationException.Create(reader, $"Error parsing version string: {reader.Value}", ex);
            }
        }

        throw JsonSerializationException.Create(reader, $"Unexpected token or value when parsing version. Token: {reader.TokenType}, Value: {reader.Value}");
    }

    /// <summary>
    /// Determines whether this instance can convert the specified object type.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
    /// </returns>
    public override bool CanConvert(Type type)
    {
        return type == typeof(Version);
    }
}