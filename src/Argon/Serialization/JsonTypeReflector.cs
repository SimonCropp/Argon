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

    static ThreadSafeStore<Type, Func<object[]?, object>> creatorCache = new(GetCreator);

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

    public static DataContractAttribute? GetDataContractAttribute(Type type)
    {
        // DataContractAttribute does not have inheritance
        var currentType = type;

        while (currentType != null)
        {
            var result = AttributeCache<DataContractAttribute>.GetAttribute(currentType);
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
            return AttributeCache<DataMemberAttribute>.GetAttribute(member);
        }

        // search property and then search base properties if nothing is returned and the property is virtual
        var property = (PropertyInfo) member;
        var result = AttributeCache<DataMemberAttribute>.GetAttribute(property);
        if (result == null)
        {
            if (property.IsVirtual())
            {
                var currentType = property.DeclaringType;

                while (result == null && currentType != null)
                {
                    var baseProperty = (PropertyInfo?) ReflectionUtils.GetMemberInfoFromType(currentType, property);
                    if (baseProperty != null && baseProperty.IsVirtual())
                    {
                        result = AttributeCache<DataMemberAttribute>.GetAttribute(baseProperty);
                    }

                    currentType = currentType.BaseType;
                }
            }
        }

        return result;
    }

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

        var creator = creatorCache.Get(attribute.ConverterType);
        return (JsonConverter) creator(attribute.ConverterParameters);
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
        var creator = creatorCache.Get(converterType);
        return (JsonConverter) creator(args);
    }

    public static NamingStrategy CreateNamingStrategyInstance(Type namingStrategyType, object[]? args)
    {
        var creator = creatorCache.Get(namingStrategyType);
        return (NamingStrategy) creator(args);
    }

    static Func<object[]?, object> GetCreator(Type type)
    {
        var defaultConstructor = type.HasDefaultConstructor()
            ? ReflectionDelegateFactory.CreateDefaultConstructor<object>(type)
            : null;

        return parameters =>
        {
            try
            {
                if (parameters != null)
                {
                    var paramTypes = new Type[parameters.Length];
                    for (var index = 0; index < parameters.Length; index++)
                    {
                        var parameter = parameters[index];
                        if (parameter == null)
                        {
                            throw new InvalidOperationException("Cannot pass a null parameter to the constructor.");
                        }
                        paramTypes[index] = parameter.GetType();
                    }

                    var constructorInfo = type.GetConstructor(paramTypes);

                    if (constructorInfo == null)
                    {
                        throw new JsonException($"No matching parameterized constructor found for '{type}'.");
                    }

                    var constructor = ReflectionDelegateFactory.CreateParameterizedConstructor(constructorInfo);
                    return constructor(parameters);
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