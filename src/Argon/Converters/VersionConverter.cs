// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Converts a <see cref="Version"/> to and from a string (e.g. <c>"1.2.3.4"</c>).
/// </summary>
public class VersionConverter :
    JsonConverter<Version>
{
    public override void WriteJson(JsonWriter writer, Version value, JsonSerializer serializer) =>
        writer.WriteValue(value.ToString());

    public override Version? ReadJson(JsonReader reader, Type type, Version? existingValue, bool hasExisting, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonToken.String)
        {
            var value = reader.StringValue;
            try
            {
                return new(value);
            }
            catch (Exception exception)
            {
                throw JsonSerializationException.Create(reader, $"Error parsing version string: {value}", exception);
            }
        }

        throw JsonSerializationException.Create(reader, $"Unexpected token or value when parsing version. Token: {reader.TokenType}, Value: {reader.Value}");
    }
}