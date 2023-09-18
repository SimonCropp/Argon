// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

static class AttributeCache<T> where T : Attribute
{
    static ThreadSafeStore<ICustomAttributeProvider, T?> TypeAttributeCache = new(JsonTypeReflector.GetAttribute<T>);

    public static T? GetAttribute(ICustomAttributeProvider provider) =>
        TypeAttributeCache.Get(provider);
}


static class AttributeCache2
{
    public class Info
    {
        public JsonContainerAttribute? ContainerAttribute { get; init; }
    }

    static ThreadSafeStore<ICustomAttributeProvider, Info> TypeAttributeCache = new(
        provider =>
        {
            var attributes = JsonTypeReflector.GetAttributes(provider).ToList();
            return new()
            {
                ContainerAttribute = attributes.OfType<JsonContainerAttribute>().SingleOrDefault()
            };
        });

    public static Info Get(ICustomAttributeProvider provider) =>
        TypeAttributeCache.Get(provider);
}