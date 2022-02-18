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
                        throw new JsonException(string.Format("Property '{0}' does not exist on JObject.", name));
                    }
                }
            }
            else
            {
                if (settings?.ErrorWhenNoMatch ?? false)
                {
                    throw new JsonException(string.Format("Properties {0} not valid on {1}.", string.Join(", ", Names.Select(n => "'" + n + "'")), t.GetType().Name));
                }
            }
        }
    }
}