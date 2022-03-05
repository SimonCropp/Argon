// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Specifies type name handling options for the <see cref="JsonSerializer" />.
/// </summary>
/// <remarks>
/// <see cref="JsonSerializer.TypeNameHandling" /> should be used with caution when your application deserializes JSON from an external source.
/// Incoming types should be validated with a custom <see cref="JsonSerializer.SerializationBinder" />
/// when deserializing with a value other than <see cref="TypeNameHandling.None" />.
/// </remarks>
[Flags]
public enum TypeNameHandling
{
    /// <summary>
    /// Do not include the .NET type name when serializing types.
    /// </summary>
    None = 0,

    /// <summary>
    /// Include the .NET type name when serializing into a JSON object structure.
    /// </summary>
    Objects = 1,

    /// <summary>
    /// Include the .NET type name when serializing into a JSON array structure.
    /// </summary>
    Arrays = 2,

    /// <summary>
    /// Always include the .NET type name when serializing.
    /// </summary>
    All = Objects | Arrays,

    /// <summary>
    /// Include the .NET type name when the type of the object being serialized is not the same as its declared type.
    /// Note that this doesn't include the root serialized object by default. To include the root object's type name in JSON
    /// you must specify a root type object with <see cref="JsonConvert.SerializeObject(object, Type, JsonSerializerSettings)" />
    /// or <see cref="JsonSerializer.Serialize(JsonWriter, object, Type)" />.
    /// </summary>
    Auto = 4
}