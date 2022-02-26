// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Specifies null value handling options for the <see cref="JsonSerializer"/>.
/// </summary>
/// <example>
///   <code lang="cs" source="..\src\Tests\Documentation\SerializationTests.cs" region="ReducingSerializedJsonSizeNullValueHandlingObject" title="NullValueHandling Class" />
///   <code lang="cs" source="..\src\Tests\Documentation\SerializationTests.cs" region="ReducingSerializedJsonSizeNullValueHandlingExample" title="NullValueHandling Ignore Example" />
/// </example>
public enum NullValueHandling
{
    /// <summary>
    /// Include null values when serializing and deserializing objects.
    /// </summary>
    Include = 0,

    /// <summary>
    /// Ignore null values when serializing and deserializing objects.
    /// </summary>
    Ignore = 1
}