namespace Argon;

public struct ItemInterceptResult
{
    public static ItemInterceptResult Default =>
        new(false, false, null);

    public static ItemInterceptResult Ignore =>
        new(true, false, null);

    public static ItemInterceptResult Replace(string? replacement) =>
        new(false, true, replacement);

    private ItemInterceptResult(bool ignore, bool replace, string? replacement)
    {
        ShouldIgnore = ignore;
        ShouldReplace = replace;
        Replacement = replacement;
    }

    public string? Replacement { get; }

    public bool ShouldReplace { get; }

    public bool ShouldIgnore { get; }
}