// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

static class AttributeCache<T> where T : Attribute
{
    static ThreadSafeStore<ICustomAttributeProvider, T?> TypeAttributeCache = new(JsonTypeReflector.GetAttribute<T>);

    public static T? GetAttribute(ICustomAttributeProvider provider) =>
        TypeAttributeCache.Get(provider);
}


static class TypeAttributeCache
{
    public class Info
    {
        public required JsonContainerAttribute? Container { get; init; }
        public required JsonConverterAttribute? Converter { get; init; }
        public required JsonObjectAttribute? Object { get; init; }
    }

    static ThreadSafeStore<ICustomAttributeProvider, Info> cache = new(
        provider =>
        {
            var attributes = JsonTypeReflector.GetAttributes(provider).ToList();
            return new()
            {
                Container = GetAttribute<JsonContainerAttribute>(attributes),
                Converter = GetAttribute<JsonConverterAttribute>(attributes),
                Object = GetAttribute<JsonObjectAttribute>(attributes)
            };
        });


    static T? GetAttribute<T>(List<Attribute> attributes) =>
        attributes.OfType<T>().SingleOrDefault();

    public static Info Get(ICustomAttributeProvider provider) =>
        cache.Get(provider);
}
static class MemberAttributeCache
{
    public class Info
    {
        public required JsonConverterAttribute? Converter { get; init; }
        public required JsonPropertyAttribute? Property { get; init; }
        public required JsonRequiredAttribute? Required { get; init; }
        public required DefaultValueAttribute? DefaultValue { get; init; }
        public required JsonIgnoreAttribute? Ignore { get; init; }
        public required JsonExtensionDataAttribute? ExtensionData { get; init; }
        public required IgnoreDataMemberAttribute? IgnoreDataMember { get; init; }


    }

    static ThreadSafeStore<ICustomAttributeProvider, Info> cache = new(
        provider =>
        {
            var attributes = JsonTypeReflector.GetAttributes(provider).ToList();
            return new()
            {
                Converter = GetAttribute<JsonConverterAttribute>(attributes),
                Property = GetAttribute<JsonPropertyAttribute>(attributes),
                Required = GetAttribute<JsonRequiredAttribute>(attributes),
                DefaultValue = GetAttribute<DefaultValueAttribute>(attributes),
                Ignore = GetAttribute<JsonIgnoreAttribute>(attributes),
                ExtensionData = GetAttribute<JsonExtensionDataAttribute>(attributes),
                IgnoreDataMember = GetAttribute<IgnoreDataMemberAttribute>(attributes),
            };
        });

    static T? GetAttribute<T>(List<Attribute> attributes) =>
        attributes.OfType<T>().SingleOrDefault();

    public static Info Get(ICustomAttributeProvider provider) =>
        cache.Get(provider);
}