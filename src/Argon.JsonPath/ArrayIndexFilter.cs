class ArrayIndexFilter :
    PathFilter
{
    public int? Index { get; set; }

    public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, JsonSelectSettings settings)
    {
        foreach (var t in current)
        {
            if (Index == null)
            {
                if (t is JArray)
                {
                    foreach (var v in t)
                    {
                        yield return v;
                    }
                }
                else
                {
                    if (settings.ErrorWhenNoMatch)
                    {
                        throw new JsonException($"Index * not valid on {t.GetType().Name}.");
                    }
                }
            }
            else
            {
                var v = GetTokenIndex(t, settings, Index.GetValueOrDefault());

                if (v != null)
                {
                    yield return v;
                }
            }
        }
    }
}