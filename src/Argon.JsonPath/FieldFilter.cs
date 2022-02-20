class FieldFilter : PathFilter
{
    internal string? Name;

    public FieldFilter(string? name)
    {
        Name = name;
    }

    public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, JsonSelectSettings settings)
    {
        foreach (var t in current)
        {
            if (t is JObject o)
            {
                if (Name != null)
                {
                    var v = o[Name];

                    if (v != null)
                    {
                        yield return v;
                    }
                    else if (settings?.ErrorWhenNoMatch ?? false)
                    {
                        throw new JsonException($"Property '{Name}' does not exist on JObject.");
                    }
                }
                else
                {
                    foreach (var p in o)
                    {
                        yield return p.Value!;
                    }
                }
            }
            else
            {
                if (settings?.ErrorWhenNoMatch ?? false)
                {
                    throw new JsonException($"Property '{Name ?? "*"}' not valid on {t.GetType().Name}.");
                }
            }
        }
    }
}