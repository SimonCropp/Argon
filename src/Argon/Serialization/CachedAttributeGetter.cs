// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

static class CachedAttributeGetter<T> where T : Attribute
{
    static readonly ThreadSafeStore<ICustomAttributeProvider, T?> TypeAttributeCache = new(JsonTypeReflector.GetAttribute<T>);

    public static T? GetAttribute(ICustomAttributeProvider provider)
    {
        return TypeAttributeCache.Get(provider);
    }
}