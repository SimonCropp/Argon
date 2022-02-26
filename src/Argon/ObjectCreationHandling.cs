// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Specifies how object creation is handled by the <see cref="JsonSerializer"/>.
/// </summary>
public enum ObjectCreationHandling
{
    /// <summary>
    /// Reuse existing objects, create new objects when needed.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// Only reuse existing objects.
    /// </summary>
    Reuse = 1,

    /// <summary>
    /// Always create new objects.
    /// </summary>
    Replace = 2
}