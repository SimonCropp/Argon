using Argon;

/// <summary>
/// Extensions to <see cref="JToken"/>.
/// </summary>
public static class JTokenExtensions
{

    /// <summary>
    /// Selects a <see cref="JToken"/> using a JSONPath expression. Selects the token that matches the object path.
    /// </summary>
    /// <param name="path">
    /// A <see cref="String"/> that contains a JSONPath expression.
    /// </param>
    /// <returns>A <see cref="JToken"/>, or <c>null</c>.</returns>
    public static JToken? SelectToken(this JToken token, string path)
    {
        return SelectToken(token, path, settings: null);
    }

    /// <summary>
    /// Selects a <see cref="JToken"/> using a JSONPath expression. Selects the token that matches the object path.
    /// </summary>
    /// <param name="path">
    /// A <see cref="String"/> that contains a JSONPath expression.
    /// </param>
    /// <param name="errorWhenNoMatch">A flag to indicate whether an error should be thrown if no tokens are found when evaluating part of the expression.</param>
    /// <returns>A <see cref="JToken"/>.</returns>
    public static JToken? SelectToken(this JToken token, string path, bool errorWhenNoMatch)
    {
        var settings = errorWhenNoMatch
            ? new JsonSelectSettings {ErrorWhenNoMatch = true}
            : null;

        return SelectToken(token, path, settings);
    }

    /// <summary>
    /// Selects a <see cref="JToken"/> using a JSONPath expression. Selects the token that matches the object path.
    /// </summary>
    /// <param name="path">
    /// A <see cref="String"/> that contains a JSONPath expression.
    /// </param>
    /// <param name="settings">The <see cref="JsonSelectSettings"/> used to select tokens.</param>
    /// <returns>A <see cref="JToken"/>.</returns>
    public static JToken? SelectToken(this JToken token, string path, JsonSelectSettings? settings)
    {
        var p = new JPath(path);

        JToken? result = null;
        foreach (var t in p.Evaluate(token, token, settings))
        {
            if (result != null)
            {
                throw new JsonException("Path returned multiple tokens.");
            }

            result = t;
        }

        return result;
    }

    /// <summary>
    /// Selects a collection of elements using a JSONPath expression.
    /// </summary>
    /// <param name="path">
    /// A <see cref="String"/> that contains a JSONPath expression.
    /// </param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="JToken"/> that contains the selected elements.</returns>
    public static IEnumerable<JToken> SelectTokens(this JToken token, string path)
    {
        return SelectTokens(token, path, settings: null);
    }

    /// <summary>
    /// Selects a collection of elements using a JSONPath expression.
    /// </summary>
    /// <param name="path">
    /// A <see cref="String"/> that contains a JSONPath expression.
    /// </param>
    /// <param name="errorWhenNoMatch">A flag to indicate whether an error should be thrown if no tokens are found when evaluating part of the expression.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="JToken"/> that contains the selected elements.</returns>
    public static IEnumerable<JToken> SelectTokens(this JToken token, string path, bool errorWhenNoMatch)
    {
        var settings = errorWhenNoMatch
            ? new JsonSelectSettings {ErrorWhenNoMatch = true}
            : null;

        return SelectTokens(token, path, settings);
    }

    /// <summary>
    /// Selects a collection of elements using a JSONPath expression.
    /// </summary>
    /// <param name="path">
    /// A <see cref="String"/> that contains a JSONPath expression.
    /// </param>
    /// <param name="settings">The <see cref="JsonSelectSettings"/> used to select tokens.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="JToken"/> that contains the selected elements.</returns>
    public static IEnumerable<JToken> SelectTokens(this JToken token, string path, JsonSelectSettings? settings)
    {
        var p = new JPath(path);
        return p.Evaluate(token, token, settings);
    }
}