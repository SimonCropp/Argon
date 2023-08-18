static class XmlUtils
{
    public static string? GetPrefix(string qualifiedName)
    {
        GetQualifiedNameParts(qualifiedName, out var prefix, out _);

        return prefix;
    }

    public static string GetLocalName(string qualifiedName)
    {
        GetQualifiedNameParts(qualifiedName, out _, out var localName);

        return localName;
    }

    public static void GetQualifiedNameParts(string qualifiedName, out string? prefix, out string localName)
    {
        var colonPosition = qualifiedName.IndexOf(':');

        if (colonPosition is -1 or 0 ||
            qualifiedName.Length - 1 == colonPosition)
        {
            prefix = null;
            localName = qualifiedName;
        }
        else
        {
            prefix = qualifiedName[..colonPosition];
            localName = qualifiedName[(colonPosition + 1)..];
        }
    }
}