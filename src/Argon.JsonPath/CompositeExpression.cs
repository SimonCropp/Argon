class CompositeExpression : QueryExpression
{
    public List<QueryExpression> Expressions { get; set; }

    public CompositeExpression(QueryOperator @operator) : base(@operator)
    {
        Expressions = new List<QueryExpression>();
    }

    public override bool IsMatch(JToken root, JToken t, JsonSelectSettings? settings)
    {
        switch (Operator)
        {
            case QueryOperator.And:
                foreach (var e in Expressions)
                {
                    if (!e.IsMatch(root, t, settings))
                    {
                        return false;
                    }
                }
                return true;
            case QueryOperator.Or:
                foreach (var e in Expressions)
                {
                    if (e.IsMatch(root, t, settings))
                    {
                        return true;
                    }
                }
                return false;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}