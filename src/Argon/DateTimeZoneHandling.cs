// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Specifies how to treat the time value when converting between string and <see cref="DateTime" />.
/// </summary>
public enum DateTimeZoneHandling
{
    /// <summary>
    /// Time zone information should be preserved when converting.
    /// </summary>
    RoundtripKind,

    /// <summary>
    /// Treat as local time. If the <see cref="DateTime" /> object represents a Coordinated Universal Time (UTC), it is converted to the local time.
    /// </summary>
    Local,

    /// <summary>
    /// Treat as a UTC. If the <see cref="DateTime" /> object represents a local time, it is converted to a UTC.
    /// </summary>
    Utc,

    /// <summary>
    /// Treat as a local time if a <see cref="DateTime" /> is being converted to a string.
    /// If a string is being converted to <see cref="DateTime" />, convert to a local time if a time zone is specified.
    /// </summary>
    Unspecified
}