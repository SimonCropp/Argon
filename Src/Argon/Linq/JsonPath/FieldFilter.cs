using Argon;

class FieldFilter : PathFilter
{
    internal string? Name;

    public FieldFilter(string? name)
    {
        Name = name;
    }

    public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, JsonSelectSettings? settings)
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
                        throw new JsonException(string.Format("Property '{0}' does not exist on JObject.", Name));
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
                    throw new JsonException(string.Format("Property '{0}' not valid on {1}.", Name ?? "*", t.GetType().Name));
                }
            }
        }
    }
}