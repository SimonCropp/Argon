namespace Argon;

/// <summary>
/// Specifies the settings used when loading JSON.
/// </summary>
public class JsonLoadSettings
{
    /// <summary>
    /// Gets or sets how JSON comments are handled when loading JSON.
    /// The default value is <see cref="Argon.Linq.CommentHandling.Ignore" />.
    /// </summary>
    public CommentHandling CommentHandling { get; set; }

    /// <summary>
    /// Gets or sets how JSON line info is handled when loading JSON.
    /// The default value is <see cref="Argon.Linq.LineInfoHandling.Load" />.
    /// </summary>
    public LineInfoHandling LineInfoHandling { get; set; } = LineInfoHandling.Load;

    /// <summary>
    /// Gets or sets how duplicate property names in JSON objects are handled when loading JSON.
    /// The default value is <see cref="Argon.Linq.DuplicatePropertyNameHandling.Replace" />.
    /// </summary>
    public DuplicatePropertyNameHandling DuplicatePropertyNameHandling { get; set; }
}