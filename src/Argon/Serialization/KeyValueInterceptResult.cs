namespace Argon;

public struct KeyValueInterceptResult
{
    readonly string? replacementKey;

    public static KeyValueInterceptResult Default =>
        new(false, false, null, false, null);

    public static KeyValueInterceptResult Ignore =>
        new(true, false, null, false, null);

    public static KeyValueInterceptResult ReplaceKeyAndValue(string replacementKey, string? replacementValue) =>
        new(false, true, replacementKey, true, replacementValue);

    public static KeyValueInterceptResult ReplaceValue(string? replacementValue) =>
        new(false, false, null, true, replacementValue);

    public static KeyValueInterceptResult ReplaceKey(string replacementKey) =>
        new(false, true, replacementKey, false, null);

    private KeyValueInterceptResult(bool ignore, bool replaceKey, string? replacementKey, bool replaceValue, string? replacementValue)
    {
        ShouldIgnore = ignore;
        ShouldReplaceKey = replaceKey;
        ShouldReplaceValue = replaceValue;
        this.replacementKey = replacementKey;
        ReplacementValue = replacementValue;
    }

    public readonly string ReplacementKey
    {
        get
        {
            if (ShouldReplaceKey)
            {
                return replacementKey!;
            }

            throw new("ReplacementKey not defined");
        }
    }

    public string? ReplacementValue { get; }

    public bool ShouldReplaceKey { get; }
    public bool ShouldReplaceValue { get; }

    public bool ShouldReplaceAndValue => ShouldReplaceKey && ShouldReplaceValue;

    public bool ShouldIgnore { get; }
}