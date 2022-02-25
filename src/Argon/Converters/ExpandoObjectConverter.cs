#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

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
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
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

                    var v = ReadValue(reader);

                    expandoObject[propertyName] = v;
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