// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Provides methods for converting between .NET types and JSON types.
/// </summary>
/// <example>
///   <code lang="cs" source="..\src\Tests\Documentation\SerializationTests.cs" region="SerializeObject" title="Serializing and Deserializing JSON with JsonConvert" />
/// </example>
public static class JsonConvert
{
    /// <summary>
    /// Gets or sets a function that creates default <see cref="JsonSerializerSettings"/>.
    /// Default settings are automatically used by serialization methods on <see cref="JsonConvert"/>,
    /// and <see cref="JToken.ToObject{T}()"/> and <see cref="JToken.FromObject(object)"/> on <see cref="JToken"/>.
    /// To serialize without using any default settings create a <see cref="JsonSerializer"/> with
    /// <see cref="JsonSerializer.Create()"/>.
    /// </summary>
    public static Func<JsonSerializerSettings>? DefaultSettings { get; set; }

    /// <summary>
    /// Represents JavaScript's boolean value <c>true</c> as a string. This field is read-only.
    /// </summary>
    public static readonly string True = "true";

    /// <summary>
    /// Represents JavaScript's boolean value <c>false</c> as a string. This field is read-only.
    /// </summary>
    public static readonly string False = "false";

    /// <summary>
    /// Represents JavaScript's <c>null</c> as a string. This field is read-only.
    /// </summary>
    public static readonly string Null = "null";

    /// <summary>
    /// Represents JavaScript's <c>undefined</c> as a string. This field is read-only.
    /// </summary>
    public static readonly string Undefined = "undefined";

    /// <summary>
    /// Represents JavaScript's positive infinity as a string. This field is read-only.
    /// </summary>
    public static readonly string PositiveInfinity = "Infinity";

    /// <summary>
    /// Represents JavaScript's negative infinity as a string. This field is read-only.
    /// </summary>
    public static readonly string NegativeInfinity = "-Infinity";

    /// <summary>
    /// Represents JavaScript's <c>NaN</c> as a string. This field is read-only.
    /// </summary>
    public static readonly string NaN = "NaN";

    /// <summary>
    /// Converts the <see cref="DateTime"/> to its JSON string representation.
    /// </summary>
    public static string ToString(DateTime value)
    {
        return ToString(value, DateTimeZoneHandling.RoundtripKind);
    }

    /// <summary>
    /// Converts the <see cref="DateTime"/> to its JSON string representation using the <see cref="DateTimeZoneHandling"/> specified.
    /// </summary>
    public static string ToString(DateTime value, DateTimeZoneHandling timeZoneHandling)
    {
        var updatedDateTime = DateTimeUtils.EnsureDateTime(value, timeZoneHandling);

        using var writer = StringUtils.CreateStringWriter(64);
        writer.Write('"');
        DateTimeUtils.WriteDateTimeString(writer, updatedDateTime, null, CultureInfo.InvariantCulture);
        writer.Write('"');
        return writer.ToString();
    }

    /// <summary>
    /// Converts the <see cref="DateTimeOffset"/> to its JSON string representation.
    /// </summary>
    public static string ToString(DateTimeOffset value)
    {
        using var writer = StringUtils.CreateStringWriter(64);
        writer.Write('"');
        DateTimeUtils.WriteDateTimeOffsetString(writer, value, null, CultureInfo.InvariantCulture);
        writer.Write('"');
        return writer.ToString();
    }

    /// <summary>
    /// Converts the <see cref="Boolean"/> to its JSON string representation.
    /// </summary>
    public static string ToString(bool value)
    {
        return value ? True : False;
    }

    /// <summary>
    /// Converts the <see cref="Char"/> to its JSON string representation.
    /// </summary>
    public static string ToString(char value)
    {
        return ToString(char.ToString(value));
    }

    /// <summary>
    /// Converts the <see cref="Enum"/> to its JSON string representation.
    /// </summary>
    public static string ToString(Enum value)
    {
        return value.ToString("D");
    }

    /// <summary>
    /// Converts the <see cref="Int32"/> to its JSON string representation.
    /// </summary>
    public static string ToString(int value)
    {
        return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="Int16"/> to its JSON string representation.
    /// </summary>
    public static string ToString(short value)
    {
        return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="UInt16"/> to its JSON string representation.
    /// </summary>
    public static string ToString(ushort value)
    {
        return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="UInt32"/> to its JSON string representation.
    /// </summary>
    public static string ToString(uint value)
    {
        return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="Int64"/>  to its JSON string representation.
    /// </summary>
    public static string ToString(long value)
    {
        return value.ToString(null, CultureInfo.InvariantCulture);
    }

    static string ToStringInternal(BigInteger value)
    {
        return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="UInt64"/> to its JSON string representation.
    /// </summary>
    public static string ToString(ulong value)
    {
        return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="Single"/> to its JSON string representation.
    /// </summary>
    public static string ToString(float value)
    {
        return EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture));
    }

    internal static string ToString(float value, FloatFormatHandling floatFormatHandling, char quoteChar, bool nullable)
    {
        return EnsureFloatFormat(value, EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture)), floatFormatHandling, quoteChar, nullable);
    }

    static string EnsureFloatFormat(double value, string text, FloatFormatHandling floatFormatHandling, char quoteChar, bool nullable)
    {
        if (floatFormatHandling == FloatFormatHandling.Symbol ||
            !(double.IsInfinity(value) || double.IsNaN(value)))
        {
            return text;
        }

        if (floatFormatHandling == FloatFormatHandling.DefaultValue)
        {
            if (nullable)
            {
                return Null;
            }

            return "0.0";
        }

        return quoteChar + text + quoteChar;
    }

    /// <summary>
    /// Converts the <see cref="Double"/> to its JSON string representation.
    /// </summary>
    /// <returns>A JSON string representation of the <see cref="Double"/>.</returns>
    public static string ToString(double value)
    {
        return EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture));
    }

    internal static string ToString(double value, FloatFormatHandling floatFormatHandling, char quoteChar, bool nullable)
    {
        var ensureDecimalPlace = EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture));
        return EnsureFloatFormat(value, ensureDecimalPlace, floatFormatHandling, quoteChar, nullable);
    }

    static string EnsureDecimalPlace(double value, string text)
    {
        if (double.IsNaN(value) ||
            double.IsInfinity(value) ||
            text.IndexOf('.') != -1 ||
            text.IndexOf('E') != -1 ||
            text.IndexOf('e') != -1)
        {
            return text;
        }

        return $"{text}.0";
    }

    static string EnsureDecimalPlace(string text)
    {
        if (text.IndexOf('.') != -1)
        {
            return text;
        }

        return $"{text}.0";
    }

    /// <summary>
    /// Converts the <see cref="Byte"/> to its JSON string representation.
    /// </summary>
    /// <returns>A JSON string representation of the <see cref="Byte"/>.</returns>
    public static string ToString(byte value)
    {
        return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="SByte"/> to its JSON string representation.
    /// </summary>
    /// <returns>A JSON string representation of the <see cref="SByte"/>.</returns>
    public static string ToString(sbyte value)
    {
        return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="Decimal"/> to its JSON string representation.
    /// </summary>
    /// <returns>A JSON string representation of the <see cref="Decimal"/>.</returns>
    public static string ToString(decimal value)
    {
        return EnsureDecimalPlace(value.ToString(null, CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Converts the <see cref="Guid"/> to its JSON string representation.
    /// </summary>
    /// <returns>A JSON string representation of the <see cref="Guid"/>.</returns>
    public static string ToString(Guid value)
    {
        var text = value.ToString("D", CultureInfo.InvariantCulture);
        return $"\"{text}\"";
    }

    /// <summary>
    /// Converts the <see cref="TimeSpan"/> to its JSON string representation.
    /// </summary>
    /// <returns>A JSON string representation of the <see cref="TimeSpan"/>.</returns>
    public static string ToString(TimeSpan value)
    {
        return ToString(value.ToString(), '"');
    }

    /// <summary>
    /// Converts the <see cref="Uri"/> to its JSON string representation.
    /// </summary>
    /// <returns>A JSON string representation of the <see cref="Uri"/>.</returns>
    public static string ToString(Uri? value)
    {
        if (value == null)
        {
            return Null;
        }

        return ToString(value, '"');
    }

    internal static string ToString(Uri value, char quoteChar)
    {
        return ToString(value.OriginalString, quoteChar);
    }

    /// <summary>
    /// Converts the <see cref="String"/> to its JSON string representation.
    /// </summary>
    /// <returns>A JSON string representation of the <see cref="String"/>.</returns>
    public static string ToString(string? value)
    {
        return ToString(value, '"');
    }

    /// <summary>
    /// Converts the <see cref="String"/> to its JSON string representation.
    /// </summary>
    /// <param name="delimiter">The string delimiter character.</param>
    /// <returns>A JSON string representation of the <see cref="String"/>.</returns>
    public static string ToString(string? value, char delimiter)
    {
        return ToString(value, delimiter, EscapeHandling.Default);
    }

    /// <summary>
    /// Converts the <see cref="String"/> to its JSON string representation.
    /// </summary>
    /// <returns>A JSON string representation of the <see cref="String"/>.</returns>
    public static string ToString(string? value, char delimiter, EscapeHandling escapeHandling)
    {
        if (delimiter != '"' &&
            delimiter != '\'')
        {
            throw new ArgumentException("Delimiter must be a single or double quote.", nameof(delimiter));
        }

        return JavaScriptUtils.ToEscapedJavaScriptString(value, delimiter, true, escapeHandling);
    }

    /// <summary>
    /// Converts the <see cref="Object"/> to its JSON string representation.
    /// </summary>
    /// <returns>A JSON string representation of the <see cref="Object"/>.</returns>
    public static string ToString(object? value)
    {
        if (value == null)
        {
            return Null;
        }

        var typeCode = ConvertUtils.GetTypeCode(value.GetType());

        switch (typeCode)
        {
            case PrimitiveTypeCode.String:
                return ToString((string)value);
            case PrimitiveTypeCode.Char:
                return ToString((char)value);
            case PrimitiveTypeCode.Boolean:
                return ToString((bool)value);
            case PrimitiveTypeCode.SByte:
                return ToString((sbyte)value);
            case PrimitiveTypeCode.Int16:
                return ToString((short)value);
            case PrimitiveTypeCode.UInt16:
                return ToString((ushort)value);
            case PrimitiveTypeCode.Int32:
                return ToString((int)value);
            case PrimitiveTypeCode.Byte:
                return ToString((byte)value);
            case PrimitiveTypeCode.UInt32:
                return ToString((uint)value);
            case PrimitiveTypeCode.Int64:
                return ToString((long)value);
            case PrimitiveTypeCode.UInt64:
                return ToString((ulong)value);
            case PrimitiveTypeCode.Single:
                return ToString((float)value);
            case PrimitiveTypeCode.Double:
                return ToString((double)value);
            case PrimitiveTypeCode.DateTime:
                return ToString((DateTime)value);
            case PrimitiveTypeCode.Decimal:
                return ToString((decimal)value);
            case PrimitiveTypeCode.DBNull:
                return Null;
            case PrimitiveTypeCode.DateTimeOffset:
                return ToString((DateTimeOffset)value);
            case PrimitiveTypeCode.Guid:
                return ToString((Guid)value);
            case PrimitiveTypeCode.Uri:
                return ToString((Uri)value);
            case PrimitiveTypeCode.TimeSpan:
                return ToString((TimeSpan)value);
            case PrimitiveTypeCode.BigInteger:
                return ToStringInternal((BigInteger)value);
        }

        throw new ArgumentException($"Unsupported type: {value.GetType()}. Use the JsonSerializer class to get the object's JSON representation.");
    }

    #region Serialize
    /// <summary>
    /// Serializes the specified object to a JSON string.
    /// </summary>
    [DebuggerStepThrough]
    public static string SerializeObject(object? value)
    {
        return SerializeObject(value, null, (JsonSerializerSettings?)null);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using formatting.
    /// </summary>
    [DebuggerStepThrough]
    public static string SerializeObject(object? value, Formatting formatting)
    {
        return SerializeObject(value, formatting, (JsonSerializerSettings?)null);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using a collection of <see cref="JsonConverter"/>.
    /// </summary>
    [DebuggerStepThrough]
    public static string SerializeObject(object? value, params JsonConverter[] converters)
    {
        JsonSerializerSettings? settings = null;
        if (converters is {Length: > 0})
        {
            settings = new JsonSerializerSettings {Converters = converters};
        }

        return SerializeObject(value, null, settings);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using formatting and a collection of <see cref="JsonConverter"/>.
    /// </summary>
    [DebuggerStepThrough]
    public static string SerializeObject(object? value, Formatting formatting, params JsonConverter[] converters)
    {
        JsonSerializerSettings? settings = null;
        if (converters is {Length: > 0})
        {
            settings = new JsonSerializerSettings {Converters = converters};
        }

        return SerializeObject(value, null, formatting, settings);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using <see cref="JsonSerializerSettings"/>.
    /// </summary>
    /// <param name="settings">The <see cref="JsonSerializerSettings"/> used to serialize the object.
    /// If this is <c>null</c>, default serialization settings will be used.</param>
    [DebuggerStepThrough]
    public static string SerializeObject(object? value, JsonSerializerSettings? settings)
    {
        return SerializeObject(value, null, settings);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using a type, formatting and <see cref="JsonSerializerSettings"/>.
    /// </summary>
    /// <param name="settings">The <see cref="JsonSerializerSettings"/> used to serialize the object.
    /// If this is <c>null</c>, default serialization settings will be used.</param>
    /// <param name="type">
    /// The type of the value being serialized.
    /// This parameter is used when <see cref="JsonSerializer.TypeNameHandling"/> is <see cref="TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
    /// Specifying the type is optional.
    /// </param>
    [DebuggerStepThrough]
    public static string SerializeObject(object? value, Type? type, JsonSerializerSettings? settings)
    {
        var jsonSerializer = JsonSerializer.CreateDefault(settings);

        return SerializeObjectInternal(value, type, jsonSerializer);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using formatting and <see cref="JsonSerializerSettings"/>.
    /// </summary>
    /// <param name="settings">The <see cref="JsonSerializerSettings"/> used to serialize the object.
    /// If this is <c>null</c>, default serialization settings will be used.</param>
    [DebuggerStepThrough]
    public static string SerializeObject(object? value, Formatting formatting, JsonSerializerSettings? settings)
    {
        return SerializeObject(value, null, formatting, settings);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using a type, formatting and <see cref="JsonSerializerSettings"/>.
    /// </summary>
    /// <param name="settings">The <see cref="JsonSerializerSettings"/> used to serialize the object.
    /// If this is <c>null</c>, default serialization settings will be used.</param>
    /// <param name="type">
    /// The type of the value being serialized.
    /// This parameter is used when <see cref="JsonSerializer.TypeNameHandling"/> is <see cref="TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
    /// Specifying the type is optional.
    /// </param>
    [DebuggerStepThrough]
    public static string SerializeObject(object? value, Type? type, Formatting formatting, JsonSerializerSettings? settings)
    {
        var jsonSerializer = JsonSerializer.CreateDefault(settings);
        jsonSerializer.Formatting = formatting;

        return SerializeObjectInternal(value, type, jsonSerializer);
    }

    static string SerializeObjectInternal(object? value, Type? type, JsonSerializer jsonSerializer)
    {
        var stringBuilder = new StringBuilder(256);
        var stringWriter = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
        using (var jsonWriter = new JsonTextWriter(stringWriter)
               {
                   Formatting = jsonSerializer.Formatting.GetValueOrDefault()
               })
        {
            jsonSerializer.Serialize(jsonWriter, value, type);
        }

        return stringWriter.ToString();
    }
    #endregion

    #region Deserialize
    /// <summary>
    /// Deserializes the JSON to a .NET object.
    /// </summary>
    [DebuggerStepThrough]
    public static object? DeserializeObject(string value)
    {
        return DeserializeObject(value, null, (JsonSerializerSettings?)null);
    }

    /// <summary>
    /// Deserializes the JSON to a .NET object using <see cref="JsonSerializerSettings"/>.
    /// </summary>
    /// <param name="settings">
    /// The <see cref="JsonSerializerSettings"/> used to deserialize the object.
    /// If this is <c>null</c>, default serialization settings will be used.
    /// </param>
    [DebuggerStepThrough]
    public static object? DeserializeObject(string value, JsonSerializerSettings settings)
    {
        return DeserializeObject(value, null, settings);
    }

    /// <summary>
    /// Deserializes the JSON to the specified .NET type.
    /// </summary>
    [DebuggerStepThrough]
    public static object? DeserializeObject(string value, Type type)
    {
        return DeserializeObject(value, type, (JsonSerializerSettings?)null);
    }

    /// <summary>
    /// Deserializes the JSON to the specified .NET type.
    /// </summary>
    [DebuggerStepThrough]
    public static T? DeserializeObject<T>(string value)
    {
        return DeserializeObject<T>(value, (JsonSerializerSettings?)null);
    }

    /// <summary>
    /// Deserializes the JSON to the given anonymous type.
    /// </summary>
    /// <typeparam name="T">
    /// The anonymous type to deserialize to. This can't be specified
    /// traditionally and must be inferred from the anonymous type passed
    /// as a parameter.
    /// </typeparam>
    [DebuggerStepThrough]
    public static T? DeserializeAnonymousType<T>(string value, T anonymousTypeObject)
    {
        return DeserializeObject<T>(value);
    }

    /// <summary>
    /// Deserializes the JSON to the given anonymous type using <see cref="JsonSerializerSettings"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The anonymous type to deserialize to. This can't be specified
    /// traditionally and must be inferred from the anonymous type passed
    /// as a parameter.
    /// </typeparam>
    /// <param name="settings">
    /// The <see cref="JsonSerializerSettings"/> used to deserialize the object.
    /// If this is <c>null</c>, default serialization settings will be used.
    /// </param>
    [DebuggerStepThrough]
    public static T? DeserializeAnonymousType<T>(string value, T anonymousTypeObject, JsonSerializerSettings settings)
    {
        return DeserializeObject<T>(value, settings);
    }

    /// <summary>
    /// Deserializes the JSON to the specified .NET type using a collection of <see cref="JsonConverter"/>.
    /// </summary>
    [DebuggerStepThrough]
    public static T? DeserializeObject<T>(string value, params JsonConverter[] converters)
    {
        return (T?)DeserializeObject(value, typeof(T), converters);
    }

    /// <summary>
    /// Deserializes the JSON to the specified .NET type using <see cref="JsonSerializerSettings"/>.
    /// </summary>
    /// <param name="settings">
    /// The <see cref="JsonSerializerSettings"/> used to deserialize the object.
    /// If this is <c>null</c>, default serialization settings will be used.
    /// </param>
    [DebuggerStepThrough]
    public static T? DeserializeObject<T>(string value, JsonSerializerSettings? settings)
    {
        return (T?)DeserializeObject(value, typeof(T), settings);
    }

    /// <summary>
    /// Deserializes the JSON to the specified .NET type using a collection of <see cref="JsonConverter"/>.
    /// </summary>
    [DebuggerStepThrough]
    public static object? DeserializeObject(string value, Type type, params JsonConverter[] converters)
    {
        JsonSerializerSettings? settings = null;
        if (converters is {Length: > 0})
        {
            settings = new JsonSerializerSettings {Converters = converters};
        }

        return DeserializeObject(value, type, settings);
    }

    /// <summary>
    /// Deserializes the JSON to the specified .NET type using <see cref="JsonSerializerSettings"/>.
    /// </summary>
    /// <param name="settings">
    /// The <see cref="JsonSerializerSettings"/> used to deserialize the object.
    /// If this is <c>null</c>, default serialization settings will be used.
    /// </param>
    public static object? DeserializeObject(string value, Type? type, JsonSerializerSettings? settings)
    {
        var jsonSerializer = JsonSerializer.CreateDefault(settings);

        // by default DeserializeObject should check for additional content
        if (!jsonSerializer.IsCheckAdditionalContentSet())
        {
            jsonSerializer.CheckAdditionalContent = true;
        }

        using var reader = new JsonTextReader(new StringReader(value));
        return jsonSerializer.Deserialize(reader, type);
    }
    #endregion

    #region Populate
    /// <summary>
    /// Populates the object with values from the JSON string.
    /// </summary>
    [DebuggerStepThrough]
    public static void PopulateObject(string value, object target)
    {
        PopulateObject(value, target, null);
    }

    /// <summary>
    /// Populates the object with values from the JSON string using <see cref="JsonSerializerSettings"/>.
    /// </summary>
    /// <param name="settings">
    /// The <see cref="JsonSerializerSettings"/> used to deserialize the object.
    /// If this is <c>null</c>, default serialization settings will be used.
    /// </param>
    public static void PopulateObject(string value, object target, JsonSerializerSettings? settings)
    {
        var jsonSerializer = JsonSerializer.CreateDefault(settings);

        using var jsonReader = new JsonTextReader(new StringReader(value));
        jsonSerializer.Populate(jsonReader, target);

        if (settings is not {CheckAdditionalContent: true})
        {
            return;
        }

        while (jsonReader.Read())
        {
            if (jsonReader.TokenType != JsonToken.Comment)
            {
                throw JsonSerializationException.Create(jsonReader, "Additional text found in JSON string after finishing deserializing object.");
            }
        }
    }
    #endregion
}