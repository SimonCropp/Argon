namespace Argon;

public struct InterceptResult
{
    public static InterceptResult Default =>
        new(false, false, null);

    public static InterceptResult Ignore =>
        new(true, false, null);

    public static InterceptResult Replace(object? replacement) =>
        new(false, true, replacement);

    private InterceptResult(bool ignore, bool replace, object? replacement)
    {
        ShouldIgnore = ignore;
        ShouldReplace = replace;
        Replacement = replacement;
    }

    public object? Replacement { get; }

    public bool ShouldReplace { get; }

    public bool ShouldIgnore { get; }
}