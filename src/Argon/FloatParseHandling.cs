// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Specifies how floating point numbers, e.g. 1.0 and 9.9, are parsed when reading JSON text.
/// </summary>
public enum FloatParseHandling
{
    /// <summary>
    /// Floating point numbers are parsed to <see cref="Double"/>.
    /// </summary>
    Double = 0,

    /// <summary>
    /// Floating point numbers are parsed to <see cref="Decimal"/>.
    /// </summary>
    Decimal = 1
}