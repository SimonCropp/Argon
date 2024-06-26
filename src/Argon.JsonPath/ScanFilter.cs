class ScanFilter(string? name) :
    PathFilter
{
    internal string? Name = name;

    public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, JsonSelectSettings settings)
    {
        foreach (var c in current)
        {
            if (Name == null)
            {
                yield return c;
            }

            var value = c;

            while (true)
            {
                var container = value as JContainer;

                value = GetNextScanValue(c, container, value);
                if (value == null)
                {
                    break;
                }

                if (value is JProperty property)
                {
                    if (property.Name == Name)
                    {
                        yield return property.Value;
                    }
                }
                else
                {
                    if (Name == null)
                    {
                        yield return value;
                    }
                }
            }
        }
    }
}