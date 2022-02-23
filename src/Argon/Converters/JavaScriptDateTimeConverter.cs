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
            if (!ReflectionUtils.IsNullable(type))
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

        var t = ReflectionUtils.IsNullableType(type)
            ? Nullable.GetUnderlyingType(type)
            : type;
        if (t == typeof(DateTimeOffset))
        {
            return new DateTimeOffset(d);
        }
        return d;
    }
}