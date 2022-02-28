// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.Data.SqlTypes;

namespace Argon;

/// <summary>
/// Converts a binary value to and from a base 64 string value.
/// </summary>
public class BinaryConverter : JsonConverter
{
    const string binaryTypeName = "System.Data.Linq.Binary";
    const string binaryToArrayName = "ToArray";
    static ReflectionObject? reflectionObject;

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var data = GetByteArray(value);

        writer.WriteValue(data);
    }

    static byte[] GetByteArray(object value)
    {
        if (value.GetType().FullName == binaryTypeName)
        {
            EnsureReflectionObject(value.GetType());
            MiscellaneousUtils.Assert(reflectionObject != null);

            return (byte[])reflectionObject.GetValue(value, binaryToArrayName)!;
        }
        if (value is SqlBinary binary)
        {
            return binary.Value;
        }

        throw new JsonSerializationException($"Unexpected value type when writing binary: {value.GetType()}");
    }

    static void EnsureReflectionObject(Type type)
    {
        reflectionObject ??= ReflectionObject.Create(type, type.GetConstructor(new[] {typeof(byte[])}), binaryToArrayName);
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

        byte[] data;

        if (reader.TokenType == JsonToken.StartArray)
        {
            data = ReadByteArray(reader);
        }
        else if (reader.TokenType == JsonToken.String)
        {
            // current token is already at base64 string
            // unable to call ReadAsBytes so do it the old fashion way
            var encodedData = reader.Value!.ToString()!;
            data = Convert.FromBase64String(encodedData);
        }
        else
        {
            throw JsonSerializationException.Create(reader, $"Unexpected token parsing binary. Expected String or StartArray, got {reader.TokenType}.");
        }

        var underlyingType = type.IsNullableType()
            ? Nullable.GetUnderlyingType(type)!
            : type;

        if (underlyingType.FullName == binaryTypeName)
        {
            EnsureReflectionObject(underlyingType);
            MiscellaneousUtils.Assert(reflectionObject != null);

            return reflectionObject.Creator!(data);
        }

        if (underlyingType == typeof(SqlBinary))
        {
            return new SqlBinary(data);
        }

        throw JsonSerializationException.Create(reader, $"Unexpected object type when writing binary: {type}");
    }

    static byte[] ReadByteArray(JsonReader reader)
    {
        var byteList = new List<byte>();

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                    byteList.Add(Convert.ToByte(reader.Value, CultureInfo.InvariantCulture));
                    break;
                case JsonToken.EndArray:
                    return byteList.ToArray();
                case JsonToken.Comment:
                    // skip
                    break;
                default:
                    throw JsonSerializationException.Create(reader, $"Unexpected token when reading bytes: {reader.TokenType}");
            }
        }

        throw JsonSerializationException.Create(reader, "Unexpected end when reading bytes.");
    }

    /// <summary>
    /// Determines whether this instance can convert the specified object type.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
    /// </returns>
    public override bool CanConvert(Type type)
    {
        return type.FullName == binaryTypeName ||
               type == typeof(SqlBinary) ||
               type == typeof(SqlBinary?);
    }
}