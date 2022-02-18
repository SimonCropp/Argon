class RootFilter : PathFilter
{
    public static readonly RootFilter Instance = new();

    private RootFilter()
    {
    }

    public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, JsonSelectSettings? settings)
    {
        return new[] { root };
    }
}