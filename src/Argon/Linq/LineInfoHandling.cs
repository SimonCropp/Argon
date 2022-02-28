// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Specifies how line information is handled when loading JSON.
/// </summary>
public enum LineInfoHandling
{
    /// <summary>
    /// Ignore line information.
    /// </summary>
    Ignore = 0,

    /// <summary>
    /// Load line information.
    /// </summary>
    Load = 1
}