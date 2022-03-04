// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.Dynamic;

namespace Argon;

/// <summary>
/// Converts an <see cref="ExpandoObject"/> to and from JSON.
/// </summary>
public class ExpandoObjectConverter : JsonConverter
{
    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        // can write is set to false
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    public override object? ReadJson(JsonReader reader, Type type, object? existingValue, JsonSerializer serializer)
    {
        return ReadValue(reader);
    }

    object? ReadValue(JsonReader reader)
    {
        if (!reader.MoveToContent())
        {
            throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
        }

        switch (reader.TokenType)
        {
            case JsonToken.StartObject:
                return ReadObject(reader);
            case JsonToken.StartArray:
                return ReadList(reader);
            default:
                if (JsonTokenUtils.IsPrimitiveToken(reader.TokenType))
                {
                    return reader.Value;
                }

                throw JsonSerializationException.Create(reader, $"Unexpected token when converting ExpandoObject: {reader.TokenType}");
        }
    }

    object ReadList(JsonReader reader)
    {
        var list = new List<object?>();

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonToken.Comment:
                    break;
                default:
                    var v = ReadValue(reader);

                    list.Add(v);
                    break;
                case JsonToken.EndArray:
                    return list;
            }
        }

        throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
    }

    object ReadObject(JsonReader reader)
    {
        IDictionary<string, object?> expandoObject = new ExpandoObject();

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonToken.PropertyName:
                    var propertyName = reader.Value!.ToString()!;

                    if (!reader.Read())
                    {
                        throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
                    }

                    expandoObject[propertyName] = ReadValue(reader);
                    break;
                case JsonToken.Comment:
                    break;
                case JsonToken.EndObject:
                    return expandoObject;
            }
        }

        throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
    }

    /// <summary>
    /// Determines whether this instance can convert the specified object type.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
    /// </returns>
    public override bool CanConvert(Type type)
    {
        return type == typeof(ExpandoObject);
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="JsonConverter"/> can write JSON.
    /// </summary>
    public override bool CanWrite => false;
}