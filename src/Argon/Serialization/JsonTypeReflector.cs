﻿// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

static class JsonTypeReflector
{
    public const string IdPropertyName = "$id";
    public const string RefPropertyName = "$ref";
    public const string TypePropertyName = "$type";
    public const string ValuePropertyName = "$value";
    public const string ArrayValuesPropertyName = "$values";

    public const string ShouldSerializePrefix = "ShouldSerialize";
    public const string SpecifiedPostfix = "Specified";

    public const string ConcurrentDictionaryTypeName = "System.Collections.Concurrent.ConcurrentDictionary`2";

    static ThreadSafeStore<Type, JsonConverter> creatorCache = new(GetCreator);

    public static bool TryGetStringConverter(Type type, [NotNullWhen(true)] out TypeConverter? typeConverter)
    {
        typeConverter = TypeDescriptor.GetConverter(type);

        // use the type's TypeConverter can convert to a string
        var converterType = typeConverter.GetType();

        var converterName = converterType.FullName;
        if (converterType == typeof(TypeConverter) ||
            string.Equals(converterName, "System.ComponentModel.ComponentConverter", StringComparison.Ordinal) ||
            string.Equals(converterName, "System.ComponentModel.ReferenceConverter", StringComparison.Ordinal) ||
            string.Equals(converterName, "System.Windows.Forms.Design.DataSourceConverter", StringComparison.Ordinal))
        {
            return false;
        }

        if (typeConverter.CanConvertTo(typeof(string)))
        {
            return true;
        }

        typeConverter = null;
        return false;
    }

    public static DataContractAttribute? GetDataContractAttribute(Type type) =>
        AttributeCache<DataContractAttribute>.GetAttribute(type);

    public static DataMemberAttribute? GetDataMemberAttribute(MemberInfo member) =>
        AttributeCache<DataMemberAttribute>.GetAttribute(member);

    public static MemberSerialization GetObjectMemberSerialization(Type type)
    {
        var objectAttribute = AttributeCache<JsonObjectAttribute>.GetAttribute(type);
        if (objectAttribute != null)
        {
            return objectAttribute.MemberSerialization;
        }

        var dataContractAttribute = GetDataContractAttribute(type);
        if (dataContractAttribute == null)
        {
            // the default
            return MemberSerialization.OptOut;
        }

        return MemberSerialization.OptIn;
    }

    public static JsonConverter? GetJsonConverter(ICustomAttributeProvider attributeProvider)
    {
        var attribute = AttributeCache<JsonConverterAttribute>.GetAttribute(attributeProvider);

        if (attribute == null)
        {
            return null;
        }

        return creatorCache.Get(attribute.ConverterType);
    }

    /// <summary>
    /// Lookup and create an instance of the <see cref="JsonConverter" /> type described by the argument.
    /// </summary>
    /// <param name="converterType">The <see cref="JsonConverter" /> type to create.</param>
    public static JsonConverter CreateJsonConverterInstance(Type converterType) =>
        creatorCache.Get(converterType);

    static JsonConverter GetCreator(Type type)
    {
        var constructor = ReflectionDelegateFactory.CreateDefaultConstructor<JsonConverter>(type);
        return constructor();
    }

    public static T? GetAttribute<T>(this Type type)
        where T : Attribute
    {
        var attribute = type.GetCustomAttribute<T>(true);
        if (attribute != null)
        {
            return attribute;
        }

        foreach (var typeInterface in type.GetInterfaces())
        {
            attribute = typeInterface.GetCustomAttribute<T>(true);
            if (attribute != null)
            {
                return attribute;
            }
        }

        return null;
    }

    public static T? GetAttribute<T>(this MemberInfo member)
        where T : Attribute
    {
        var attribute = member.GetCustomAttribute<T>(true);
        if (attribute != null)
        {
            return attribute;
        }

        if (member.DeclaringType != null)
        {
            foreach (var typeInterface in member.DeclaringType.GetInterfaces())
            {
                var interfaceTypeMemberInfo = ReflectionUtils.GetMemberInfoFromType(typeInterface, member);

                if (interfaceTypeMemberInfo != null)
                {
                    attribute = interfaceTypeMemberInfo.GetCustomAttribute<T>(true);
                    if (attribute != null)
                    {
                        return attribute;
                    }
                }
            }
        }

        return null;
    }

    public static T? GetAttribute<T>(ICustomAttributeProvider provider)
        where T : Attribute
    {
        if (provider is Type type)
        {
            return GetAttribute<T>(type);
        }

        if (provider is MemberInfo member)
        {
            return GetAttribute<T>(member);
        }

        if (provider is ParameterInfo parameter)
        {
            return parameter.GetCustomAttribute<T>();
        }

        throw new($"Bad provider: {provider.GetType().FullName}");
    }

    public static ReflectionDelegateFactory ReflectionDelegateFactory
    {
        get
        {
#if NETSTANDARD2_0
            return ExpressionReflectionDelegateFactory.Instance;
#else
            return DynamicReflectionDelegateFactory.Instance;
#endif
        }
    }
}