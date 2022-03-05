// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Specifies how JSON comments are handled when loading JSON.
/// </summary>
public enum CommentHandling
{
    /// <summary>
    /// Ignore comments.
    /// </summary>
    Ignore = 0,

    /// <summary>
    /// Load comments as a <see cref="JValue" /> with type <see cref="JTokenType.Comment" />.
    /// </summary>
    Load = 1
}