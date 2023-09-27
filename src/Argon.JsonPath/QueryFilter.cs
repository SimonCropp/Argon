class QueryFilter(QueryExpression expression) :
    PathFilter
{
    internal QueryExpression Expression = expression;

    public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, JsonSelectSettings settings)
    {
        foreach (var token in current)
        {
            foreach (var v in token)
            {
                if (Expression.IsMatch(root, v, settings))
                {
                    yield return v;
                }
            }
        }
    }
}