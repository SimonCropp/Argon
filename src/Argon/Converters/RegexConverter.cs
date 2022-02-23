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

using System.Text.RegularExpressions;

namespace Argon;

/// <summary>
/// Converts a <see cref="Regex"/> to and from JSON.
/// </summary>
public class RegexConverter : JsonConverter
{
    const string patternName = "Pattern";
    const string optionsName = "Options";

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        var regex = (Regex) value;

        WriteJson(writer, regex, serializer);
    }

    static void WriteJson(JsonWriter writer, Regex regex, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        var value = regex.ToString();
        if (serializer.ContractResolver is DefaultContractResolver resolver)
        {
            writer.WritePropertyName(resolver.GetResolvedPropertyName(patternName));
            writer.WriteValue(value);
            writer.WritePropertyName(resolver.GetResolvedPropertyName(optionsName));
        }
        else
        {
            writer.WritePropertyName(patternName);
            writer.WriteValue(value);
            writer.WritePropertyName(optionsName);
        }

        serializer.Serialize(writer, regex.Options);
        writer.WriteEndObject();
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="type">Type of the object.</param>
    /// <param name="existingValue">The existing value of object being read.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override object? ReadJson(JsonReader reader, Type type, object? existingValue, JsonSerializer serializer)
    {
        switch (reader.TokenType)
        {
            case JsonToken.StartObject:
                return ReadRegexObject(reader, serializer);
            case JsonToken.String:
                return ReadRegexString(reader);
            case JsonToken.Null:
                return null;
        }

        throw JsonSerializationException.Create(reader, "Unexpected token when reading Regex.");
    }

    static object ReadRegexString(JsonReader reader)
    {
        var regexText = (string)reader.Value!;

        if (regexText.Length > 0 && regexText[0] == '/')
        {
            var patternOptionDelimiterIndex = regexText.LastIndexOf('/');

            if (patternOptionDelimiterIndex > 0)
            {
                var patternText = regexText.Substring(1, patternOptionDelimiterIndex - 1);
                var optionsText = regexText.Substring(patternOptionDelimiterIndex + 1);

                var options = MiscellaneousUtils.GetRegexOptions(optionsText);

                return new Regex(patternText, options);
            }
        }

        throw JsonSerializationException.Create(reader, "Regex pattern must be enclosed by slashes.");
    }

    static Regex ReadRegexObject(JsonReader reader, JsonSerializer serializer)
    {
        string? pattern = null;
        RegexOptions? options = null;

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonToken.PropertyName:
                    var propertyName = reader.Value!.ToString();

                    if (!reader.Read())
                    {
                        throw JsonSerializationException.Create(reader, "Unexpected end when reading Regex.");
                    }

                    if (string.Equals(propertyName, patternName, StringComparison.OrdinalIgnoreCase))
                    {
                        pattern = (string?)reader.Value;
                    }
                    else if (string.Equals(propertyName, optionsName, StringComparison.OrdinalIgnoreCase))
                    {
                        options = serializer.Deserialize<RegexOptions>(reader);
                    }
                    else
                    {
                        reader.Skip();
                    }
                    break;
                case JsonToken.Comment:
                    break;
                case JsonToken.EndObject:
                    if (pattern == null)
                    {
                        throw JsonSerializationException.Create(reader, "Error deserializing Regex. No pattern found.");
                    }

                    return new Regex(pattern, options ?? RegexOptions.None);
            }
        }

        throw JsonSerializationException.Create(reader, "Unexpected end when reading Regex.");
    }

    /// <summary>
    /// Determines whether this instance can convert the specified object type.
    /// </summary>
    /// <param name="type">Type of the object.</param>
    /// <returns>
    /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
    /// </returns>
    public override bool CanConvert(Type type)
    {
        return type.Name == nameof(Regex) && IsRegex(type);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static bool IsRegex(Type type)
    {
        return type == typeof(Regex);
    }
}