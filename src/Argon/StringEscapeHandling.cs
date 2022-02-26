// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Specifies how strings are escaped when writing JSON text.
/// </summary>
public enum StringEscapeHandling
{
    /// <summary>
    /// Only control characters (e.g. newline) are escaped.
    /// </summary>
    Default = 0,

    /// <summary>
    /// All non-ASCII and control characters (e.g. newline) are escaped.
    /// </summary>
    EscapeNonAscii = 1,

    /// <summary>
    /// HTML (&lt;, &gt;, &amp;, &apos;, &quot;) and control characters (e.g. newline) are escaped.
    /// </summary>
    EscapeHtml = 2
}