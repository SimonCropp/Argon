// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Provides an interface to enable a class to return line and position information.
/// </summary>
public interface IJsonLineInfo
{
    /// <summary>
    /// Gets a value indicating whether the class can return line information.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if <see cref="LineNumber"/> and <see cref="LinePosition"/> can be provided; otherwise, <c>false</c>.
    /// </returns>
    bool HasLineInfo();

    /// <summary>
    /// Gets the current line number.
    /// </summary>
    int LineNumber { get; }

    /// <summary>
    /// Gets the current line position.
    /// </summary>
    int LinePosition { get; }
}