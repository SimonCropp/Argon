namespace Argon;

/// <summary>
/// Specifies the settings used when selecting JSON.
/// </summary>
public class JsonSelectSettings
{
    /// <summary>
    /// Gets or sets a timeout that will be used when executing regular expressions.
    /// </summary>
    public TimeSpan? RegexMatchTimeout { get; set; }

    /// <summary>
    /// Gets or sets a flag that indicates whether an error should be thrown if
    /// no tokens are found when evaluating part of the expression.
    /// </summary>
    public bool ErrorWhenNoMatch { get; set; }
}