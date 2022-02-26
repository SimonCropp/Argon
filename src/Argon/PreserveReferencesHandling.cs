// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Specifies reference handling options for the <see cref="JsonSerializer"/>.
/// </summary>
/// <example>
///   <code lang="cs" source="..\src\Tests\Documentation\SerializationTests.cs" region="PreservingObjectReferencesOn" title="Preserve Object References" />
/// </example>
[Flags]
public enum PreserveReferencesHandling
{
    /// <summary>
    /// Do not preserve references when serializing types.
    /// </summary>
    None = 0,

    /// <summary>
    /// Preserve references when serializing into a JSON object structure.
    /// </summary>
    Objects = 1,

    /// <summary>
    /// Preserve references when serializing into a JSON array structure.
    /// </summary>
    Arrays = 2,

    /// <summary>
    /// Preserve references when serializing.
    /// </summary>
    All = Objects | Arrays
}