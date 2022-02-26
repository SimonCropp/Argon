// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Specifies missing member handling options for the <see cref="JsonSerializer"/>.
/// </summary>
public enum MissingMemberHandling
{
    /// <summary>
    /// Ignore a missing member and do not attempt to deserialize it.
    /// </summary>
    Ignore = 0,

    /// <summary>
    /// Throw a <see cref="JsonSerializationException"/> when a missing member is encountered during deserialization.
    /// </summary>
    Error = 1
}