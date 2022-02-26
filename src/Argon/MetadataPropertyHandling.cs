// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Specifies metadata property handling options for the <see cref="JsonSerializer"/>.
/// </summary>
public enum MetadataPropertyHandling
{
    /// <summary>
    /// Read metadata properties located at the start of a JSON object.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Read metadata properties located anywhere in a JSON object. Note that this setting will impact performance.
    /// </summary>
    ReadAhead = 1,

    /// <summary>
    /// Do not try to read metadata properties.
    /// </summary>
    Ignore = 2
}