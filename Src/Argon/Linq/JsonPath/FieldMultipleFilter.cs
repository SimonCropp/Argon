using Argon;

class FieldMultipleFilter : PathFilter
{
    internal List<string> Names;

    public FieldMultipleFilter(List<string> names)
    {
        Names = names;
    }

    public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, JsonSelectSettings? settings)
    {
        foreach (var t in current)
        {
            if (t is JObject o)
            {
                foreach (var name in Names)
                {
                    var v = o[name];

                    if (v != null)
                    {
                        yield return v;
                    }

                    if (settings?.ErrorWhenNoMatch ?? false)
                    {
                        throw new JsonException($"Property '{name}' does not exist on JObject.");
                    }
                }
            }
            else
            {
                if (settings?.ErrorWhenNoMatch ?? false)
                {
                    throw new JsonException($"Properties {string.Join(", ", Names.Select(n => $"'{n}'"))} not valid on {t.GetType().Name}.");
                }
            }
        }
    }
}