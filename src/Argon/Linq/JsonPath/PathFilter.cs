using Argon;

abstract class PathFilter
{
    public abstract IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, JsonSelectSettings? settings);

    protected static JToken? GetTokenIndex(JToken t, JsonSelectSettings? settings, int index)
    {
        if (t is JArray a)
        {
            if (a.Count <= index)
            {
                if (settings?.ErrorWhenNoMatch ?? false)
                {
                    throw new JsonException($"Index {index} outside the bounds of JArray.");
                }

                return null;
            }

            return a[index];
        }
        else if (t is JConstructor c)
        {
            if (c.Count <= index)
            {
                if (settings?.ErrorWhenNoMatch ?? false)
                {
                    throw new JsonException($"Index {index} outside the bounds of JConstructor.");
                }

                return null;
            }

            return c[index];
        }
        else
        {
            if (settings?.ErrorWhenNoMatch ?? false)
            {
                throw new JsonException($"Index {index} not valid on {t.GetType().Name}.");
            }

            return null;
        }
    }

    protected static JToken? GetNextScanValue(JToken originalParent, JToken? container, JToken? value)
    {
        // step into container's values
        if (container is {HasValues: true})
        {
            value = container.First;
        }
        else
        {
            // finished container, move to parent
            while (value != null && value != originalParent && value == value.Parent!.Last)
            {
                value = value.Parent;
            }

            // finished
            if (value == null || value == originalParent)
            {
                return null;
            }

            // move to next value in container
            value = value.Next;
        }

        return value;
    }
}