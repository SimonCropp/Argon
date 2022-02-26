// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

static class TypeExtensions
{
    public static bool AssignableToTypeName(this Type type, string fullTypeName, bool searchInterfaces, [NotNullWhen(true)]out Type? match)
    {
        var current = type;

        while (current != null)
        {
            if (string.Equals(current.FullName, fullTypeName, StringComparison.Ordinal))
            {
                match = current;
                return true;
            }

            current = current.BaseType;
        }

        if (searchInterfaces)
        {
            foreach (var i in type.GetInterfaces())
            {
                if (string.Equals(i.Name, fullTypeName, StringComparison.Ordinal))
                {
                    match = type;
                    return true;
                }
            }
        }

        match = null;
        return false;
    }

    public static bool AssignableToTypeName(this Type type, string fullTypeName, bool searchInterfaces)
    {
        return type.AssignableToTypeName(fullTypeName, searchInterfaces, out _);
    }

    public static bool ImplementInterface(this Type type, Type interfaceType)
    {
        for (var currentType = type; currentType != null; currentType = currentType.BaseType)
        {
            IEnumerable<Type> interfaces = currentType.GetInterfaces();
            foreach (var i in interfaces)
            {
                if (i == interfaceType || (i != null && i.ImplementInterface(interfaceType)))
                {
                    return true;
                }
            }
        }

        return false;
    }
}