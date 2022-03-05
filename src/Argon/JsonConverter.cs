// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Converts an object to and from JSON.
/// </summary>
public abstract class JsonConverter
{
    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    public abstract void WriteJson(JsonWriter writer, object value, JsonSerializer serializer);

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    public abstract object? ReadJson(JsonReader reader, Type type, object? existingValue, JsonSerializer serializer);

    /// <summary>
    /// Determines whether this instance can convert the specified object type.
    /// </summary>
    /// <returns>
    /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
    /// </returns>
    public abstract bool CanConvert(Type type);

    /// <summary>
    /// Gets a value indicating whether this <see cref="JsonConverter" /> can read JSON.
    /// </summary>
    public virtual bool CanRead => true;

    /// <summary>
    /// Gets a value indicating whether this <see cref="JsonConverter" /> can write JSON.
    /// </summary>
    public virtual bool CanWrite => true;
}

/// <summary>
/// Converts an object to and from JSON.
/// </summary>
public abstract class JsonConverter<T> : JsonConverter
{
    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    public sealed override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (!IsValidType(value))
        {
            throw new JsonSerializationException($"Converter cannot write specified value to JSON. {typeof(T)} is required.");
        }

        WriteJson(writer, (T?) value, serializer);
    }

    static bool IsValidType(object? value)
    {
        if (value == null)
        {
            return typeof(T).IsNullable();
        }

        return value is T;
    }

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    public abstract void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer);

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    public sealed override object? ReadJson(JsonReader reader, Type type, object? existingValue, JsonSerializer serializer)
    {
        var existingIsNull = existingValue == null;
        if (!(existingIsNull || existingValue is T))
        {
            throw new JsonSerializationException($"Converter cannot read JSON with the specified existing value. {typeof(T)} is required.");
        }

        return ReadJson(reader, type, existingIsNull ? default : (T?) existingValue, !existingIsNull, serializer);
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    public abstract T? ReadJson(JsonReader reader, Type type, T? existingValue, bool hasExistingValue, JsonSerializer serializer);

    /// <summary>
    /// Determines whether this instance can convert the specified object type.
    /// </summary>
    /// <returns>
    /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
    /// </returns>
    public sealed override bool CanConvert(Type type)
    {
        return typeof(T).IsAssignableFrom(type);
    }
}