// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Specifies the type of token.
/// </summary>
public enum JTokenType
{
    /// <summary>
    /// No token type has been set.
    /// </summary>
    None = 0,

    /// <summary>
    /// A JSON object.
    /// </summary>
    Object = 1,

    /// <summary>
    /// A JSON array.
    /// </summary>
    Array = 2,

    /// <summary>
    /// A JSON object property.
    /// </summary>
    Property = 3,

    /// <summary>
    /// A comment.
    /// </summary>
    Comment = 4,

    /// <summary>
    /// An integer value.
    /// </summary>
    Integer = 5,

    /// <summary>
    /// A float value.
    /// </summary>
    Float = 6,

    /// <summary>
    /// A string value.
    /// </summary>
    String = 7,

    /// <summary>
    /// A boolean value.
    /// </summary>
    Boolean = 8,

    /// <summary>
    /// A null value.
    /// </summary>
    Null = 9,

    /// <summary>
    /// An undefined value.
    /// </summary>
    Undefined = 10,

    /// <summary>
    /// A date value.
    /// </summary>
    Date = 11,

    /// <summary>
    /// A raw JSON value.
    /// </summary>
    Raw = 12,

    /// <summary>
    /// A collection of bytes value.
    /// </summary>
    Bytes = 13,

    /// <summary>
    /// A Guid value.
    /// </summary>
    Guid = 14,

    /// <summary>
    /// A Uri value.
    /// </summary>
    Uri = 15,

    /// <summary>
    /// A TimeSpan value.
    /// </summary>
    TimeSpan = 16
}