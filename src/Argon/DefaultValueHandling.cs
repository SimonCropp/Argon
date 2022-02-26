// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.ComponentModel;

namespace Argon;

/// <summary>
/// Specifies default value handling options for the <see cref="JsonSerializer"/>.
/// </summary>
/// <example>
///   <code lang="cs" source="..\src\Tests\Documentation\SerializationTests.cs" region="ReducingSerializedJsonSizeDefaultValueHandlingObject" title="DefaultValueHandling Class" />
///   <code lang="cs" source="..\src\Tests\Documentation\SerializationTests.cs" region="ReducingSerializedJsonSizeDefaultValueHandlingExample" title="DefaultValueHandling Ignore Example" />
/// </example>
[Flags]
public enum DefaultValueHandling
{
    /// <summary>
    /// Include members where the member value is the same as the member's default value when serializing objects.
    /// Included members are written to JSON. Has no effect when deserializing.
    /// </summary>
    Include = 0,

    /// <summary>
    /// Ignore members where the member value is the same as the member's default value when serializing objects
    /// so that it is not written to JSON.
    /// This option will ignore all default values (e.g. <c>null</c> for objects and nullable types; <c>0</c> for integers,
    /// decimals and floating point numbers; and <c>false</c> for booleans). The default value ignored can be changed by
    /// placing the <see cref="DefaultValueAttribute"/> on the property.
    /// </summary>
    Ignore = 1,

    /// <summary>
    /// Members with a default value but no JSON will be set to their default value when deserializing.
    /// </summary>
    Populate = 2,

    /// <summary>
    /// Ignore members where the member value is the same as the member's default value when serializing objects
    /// and set members to their default value when deserializing.
    /// </summary>
    IgnoreAndPopulate = Ignore | Populate
}