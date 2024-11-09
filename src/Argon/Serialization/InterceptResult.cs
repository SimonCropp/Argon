namespace Argon;

public struct InterceptResult
{
    public static InterceptResult Default =>
        new(false, false, null);

    public static InterceptResult Ignore =>
        new(true, false, null);

    public static InterceptResult Replace(string? replacement) =>
        new(false, true, replacement);

    private InterceptResult(bool ignore, bool replace, string? replacement)
    {
        ShouldIgnore = ignore;
        ShouldReplace = replace;
        Replacement = replacement;
    }

    public string? Replacement { get; }

    public bool ShouldReplace { get; }

    public bool ShouldIgnore { get; }
}