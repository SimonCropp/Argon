// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

static class AttributeCache<T> where T : Attribute
{
    static ThreadSafeStore<ICustomAttributeProvider, T?> cache = new(JsonTypeReflector.GetAttribute<T>);

    public static T? GetAttribute(ICustomAttributeProvider provider) =>
        cache.Get(provider);
}

static class TypeAttributeCache
{
    public class Info
    {
        public required JsonContainerAttribute? Container { get; init; }
        public required JsonConverterAttribute? Converter { get; init; }
        public required JsonObjectAttribute? Object { get; init; }
        public required DataContractAttribute? DataContract { get; init; }
        public required MemberSerialization MemberSerialization { get; init; }
    }

    static ThreadSafeStore<Type, Info> cache = new(
        provider =>
        {
            var attributes = provider.GetAttributes().ToList();
            var dataContractAttribute = GetAttribute<DataContractAttribute>(attributes);

            var jsonObjectAttribute = GetAttribute<JsonObjectAttribute>(attributes);
            return new()
            {
                Container = GetAttribute<JsonContainerAttribute>(attributes),
                Converter = GetAttribute<JsonConverterAttribute>(attributes),
                Object = jsonObjectAttribute,
                DataContract = dataContractAttribute,
                MemberSerialization = GetObjectMemberSerialization(jsonObjectAttribute, dataContractAttribute)
            };
        });

    static MemberSerialization GetObjectMemberSerialization(JsonObjectAttribute? objectAttribute, DataContractAttribute? dataContract)
    {
        if (objectAttribute != null)
        {
            return objectAttribute.MemberSerialization;
        }

        if (dataContract == null)
        {
            return MemberSerialization.OptOut;
        }

        return MemberSerialization.OptIn;
    }
    static T? GetAttribute<T>(List<Attribute> attributes) =>
        attributes.OfType<T>().SingleOrDefault();

    public static Info Get(Type type) =>
        cache.Get(type);
}