namespace Argon;

public struct KeyValueInterceptResult
{
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
        ReplacementKey = replacementKey!;
        ReplacementValue = replacementValue;
    }

    [field: AllowNull, MaybeNull]
    public readonly string ReplacementKey
    {
        get
        {
            if (ShouldReplaceKey)
            {
                return field!;
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