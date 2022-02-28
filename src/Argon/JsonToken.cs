// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Specifies the type of JSON token.
/// </summary>
public enum JsonToken
{
    /// <summary>
    /// This is returned by the <see cref="JsonReader"/> if a read method has not been called.
    /// </summary>
    None = 0,

    /// <summary>
    /// An object start token.
    /// </summary>
    StartObject = 1,

    /// <summary>
    /// An array start token.
    /// </summary>
    StartArray = 2,

    /// <summary>
    /// An object property name.
    /// </summary>
    PropertyName = 3,

    /// <summary>
    /// A comment.
    /// </summary>
    Comment = 4,

    /// <summary>
    /// Raw JSON.
    /// </summary>
    Raw = 5,

    /// <summary>
    /// An integer.
    /// </summary>
    Integer = 6,

    /// <summary>
    /// A float.
    /// </summary>
    Float = 7,

    /// <summary>
    /// A string.
    /// </summary>
    String = 8,

    /// <summary>
    /// A boolean.
    /// </summary>
    Boolean = 9,

    /// <summary>
    /// A null token.
    /// </summary>
    Null = 10,

    /// <summary>
    /// An undefined token.
    /// </summary>
    Undefined = 11,

    /// <summary>
    /// An object end token.
    /// </summary>
    EndObject = 12,

    /// <summary>
    /// An array end token.
    /// </summary>
    EndArray = 13,

    /// <summary>
    /// A Date.
    /// </summary>
    Date = 14,

    /// <summary>
    /// Byte data.
    /// </summary>
    Bytes = 15
}