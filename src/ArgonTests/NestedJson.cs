public static class NestedJson
{
    public static string Build(int depth)
    {
        var root = new JObject();
        var current = root;
        for (var i = 0; i < depth - 1; i++)
        {
            var nested = new JObject();
            current[i.ToString()] = nested;

            current = nested;
        }

        return root.ToString();
    }
}