﻿// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.ComponentModel;

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

    static readonly ThreadSafeStore<Type, Func<object[]?, object>> CreatorCache = new(GetCreator);

    public static T? GetCachedAttribute<T>(ICustomAttributeProvider provider) where T : Attribute
    {
        return CachedAttributeGetter<T>.GetAttribute(provider);
    }

    public static bool CanTypeDescriptorConvertString(Type type, out TypeConverter typeConverter)
    {
        typeConverter = TypeDescriptor.GetConverter(type);

        // use the type's TypeConverter if it has one and can convert to a string
        if (typeConverter != null)
        {
            var converterType = typeConverter.GetType();

            if (!string.Equals(converterType.FullName, "System.ComponentModel.ComponentConverter", StringComparison.Ordinal)
                && !string.Equals(converterType.FullName, "System.ComponentModel.ReferenceConverter", StringComparison.Ordinal)
                && !string.Equals(converterType.FullName, "System.Windows.Forms.Design.DataSourceConverter", StringComparison.Ordinal)
                && converterType != typeof(TypeConverter))
            {
                return typeConverter.CanConvertTo(typeof(string));
            }
        }

        return false;
    }

    public static DataContractAttribute? GetDataContractAttribute(Type type)
    {
        // DataContractAttribute does not have inheritance
        var currentType = type;

        while (currentType != null)
        {
            var result = CachedAttributeGetter<DataContractAttribute>.GetAttribute(currentType);
            if (result != null)
            {
                return result;
            }

            currentType = currentType.BaseType;
        }

        return null;
    }

    public static DataMemberAttribute? GetDataMemberAttribute(MemberInfo member)
    {
        // DataMemberAttribute does not have inheritance

        // can't override a field
        if (member.MemberType == MemberTypes.Field)
        {
            return CachedAttributeGetter<DataMemberAttribute>.GetAttribute(member);
        }

        // search property and then search base properties if nothing is returned and the property is virtual
        var property = (PropertyInfo) member;
        var result = CachedAttributeGetter<DataMemberAttribute>.GetAttribute(property);
        if (result == null)
        {
            if (property.IsVirtual())
            {
                var currentType = property.DeclaringType;

                while (result == null && currentType != null)
                {
                    var baseProperty = (PropertyInfo) ReflectionUtils.GetMemberInfoFromType(currentType, property);
                    if (baseProperty != null && baseProperty.IsVirtual())
                    {
                        result = CachedAttributeGetter<DataMemberAttribute>.GetAttribute(baseProperty);
                    }

                    currentType = currentType.BaseType;
                }
            }
        }

        return result;
    }

    public static MemberSerialization GetObjectMemberSerialization(Type type)
    {
        var objectAttribute = GetCachedAttribute<JsonObjectAttribute>(type);
        if (objectAttribute != null)
        {
            return objectAttribute.MemberSerialization;
        }

        var dataContractAttribute = GetDataContractAttribute(type);
        if (dataContractAttribute != null)
        {
            return MemberSerialization.OptIn;
        }

        // the default
        return MemberSerialization.OptOut;
    }

    public static JsonConverter? GetJsonConverter(ICustomAttributeProvider attributeProvider)
    {
        var converterAttribute = GetCachedAttribute<JsonConverterAttribute>(attributeProvider);

        if (converterAttribute != null)
        {
            var creator = CreatorCache.Get(converterAttribute.ConverterType);
            if (creator != null)
            {
                return (JsonConverter) creator(converterAttribute.ConverterParameters);
            }
        }

        return null;
    }

    /// <summary>
    /// Lookup and create an instance of the <see cref="JsonConverter" /> type described by the argument.
    /// </summary>
    /// <param name="converterType">The <see cref="JsonConverter" /> type to create.</param>
    /// <param name="args">
    /// Optional arguments to pass to an initializing constructor of the JsonConverter.
    /// If <c>null</c>, the default constructor is used.
    /// </param>
    public static JsonConverter CreateJsonConverterInstance(Type converterType, object[]? args)
    {
        var converterCreator = CreatorCache.Get(converterType);
        return (JsonConverter) converterCreator(args);
    }

    public static NamingStrategy CreateNamingStrategyInstance(Type namingStrategyType, object[]? args)
    {
        var converterCreator = CreatorCache.Get(namingStrategyType);
        return (NamingStrategy) converterCreator(args);
    }

    public static NamingStrategy? GetContainerNamingStrategy(JsonContainerAttribute containerAttribute)
    {
        if (containerAttribute.NamingStrategyInstance == null)
        {
            if (containerAttribute.NamingStrategyType == null)
            {
                return null;
            }

            containerAttribute.NamingStrategyInstance = CreateNamingStrategyInstance(containerAttribute.NamingStrategyType, containerAttribute.NamingStrategyParameters);
        }

        return containerAttribute.NamingStrategyInstance;
    }

    static Func<object[]?, object> GetCreator(Type type)
    {
        var defaultConstructor = type.HasDefaultConstructor(false)
            ? ReflectionDelegateFactory.CreateDefaultConstructor<object>(type)
            : null;

        return parameters =>
        {
            try
            {
                if (parameters != null)
                {
                    var paramTypes = parameters.Select(param =>
                    {
                        if (param == null)
                        {
                            throw new InvalidOperationException("Cannot pass a null parameter to the constructor.");
                        }

                        return param.GetType();
                    }).ToArray();
                    var parameterizedConstructorInfo = type.GetConstructor(paramTypes);

                    if (parameterizedConstructorInfo != null)
                    {
                        var parameterizedConstructor = ReflectionDelegateFactory.CreateParameterizedConstructor(parameterizedConstructorInfo);
                        return parameterizedConstructor(parameters);
                    }

                    throw new JsonException($"No matching parameterized constructor found for '{type}'.");
                }

                if (defaultConstructor == null)
                {
                    throw new JsonException($"No parameterless constructor defined for '{type}'.");
                }

                return defaultConstructor();
            }
            catch (Exception exception)
            {
                throw new JsonException($"Error creating '{type}'.", exception);
            }
        };
    }

    static T? GetAttribute<T>(Type type) where T : Attribute
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

    static T? GetAttribute<T>(MemberInfo member) where T : Attribute
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

    public static T? GetAttribute<T>(ICustomAttributeProvider provider) where T : Attribute
    {
        if (provider is Type type)
        {
            return GetAttribute<T>(type);
        }

        if (provider is MemberInfo member)
        {
            return GetAttribute<T>(member);
        }

        return ReflectionUtils.GetAttribute<T>(provider, true);
    }

    public static ReflectionDelegateFactory ReflectionDelegateFactory
    {
        get
        {
#if !NETSTANDARD2_0
            return DynamicReflectionDelegateFactory.Instance;
#else
            return ExpressionReflectionDelegateFactory.Instance;
#endif
        }
    }
}