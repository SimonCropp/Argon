﻿namespace Argon;

/// <summary>
/// Specifies the settings used when loading JSON.
/// </summary>
public class JsonLoadSettings
{
    /// <summary>
    /// Gets or sets how JSON comments are handled when loading JSON.
    /// The default value is <see cref="Argon.CommentHandling.Ignore" />.
    /// </summary>
    public CommentHandling CommentHandling { get; set; }

    /// <summary>
    /// Gets or sets how JSON line info is handled when loading JSON.
    /// The default value is <see cref="Argon.LineInfoHandling.Load" />.
    /// </summary>
    public LineInfoHandling LineInfoHandling { get; set; } = LineInfoHandling.Load;
}