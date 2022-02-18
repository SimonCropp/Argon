class QueryFilter : PathFilter
{
    internal QueryExpression Expression;

    public QueryFilter(QueryExpression expression)
    {
        Expression = expression;
    }

    public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, JsonSelectSettings? settings)
    {
        foreach (var t in current)
        {
            foreach (var v in t)
            {
                if (Expression.IsMatch(root, v, settings))
                {
                    yield return v;
                }
            }
        }
    }
}