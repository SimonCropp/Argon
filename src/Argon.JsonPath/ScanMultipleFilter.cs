class ScanMultipleFilter : PathFilter
{
    List<string> names;

    public ScanMultipleFilter(List<string> names) =>
        this.names = names;

    public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, JsonSelectSettings settings)
    {
        foreach (var c in current)
        {
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
                    foreach (var name in names)
                    {
                        if (property.Name == name)
                        {
                            yield return property.Value;
                        }
                    }
                }
            }
        }
    }
}