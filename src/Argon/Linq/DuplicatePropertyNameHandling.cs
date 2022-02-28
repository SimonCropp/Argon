// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Specifies how duplicate property names are handled when loading JSON.
/// </summary>
public enum DuplicatePropertyNameHandling
{
    /// <summary>
    /// Replace the existing value when there is a duplicate property. The value of the last property in the JSON object will be used.
    /// </summary>
    Replace = 0,
    /// <summary>
    /// Ignore the new value when there is a duplicate property. The value of the first property in the JSON object will be used.
    /// </summary>
    Ignore = 1,
    /// <summary>
    /// Throw a <see cref="JsonReaderException"/> when a duplicate property is encountered.
    /// </summary>
    Error = 2
}