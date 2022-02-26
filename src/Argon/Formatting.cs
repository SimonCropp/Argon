// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Specifies formatting options for the <see cref="JsonTextWriter"/>.
/// </summary>
public enum Formatting
{
    /// <summary>
    /// No special formatting is applied. This is the default.
    /// </summary>
    None = 0,

    /// <summary>
    /// Causes child objects to be indented according to the <see cref="JsonTextWriter.Indentation"/> and <see cref="JsonTextWriter.IndentChar"/> settings.
    /// </summary>
    Indented = 1
}