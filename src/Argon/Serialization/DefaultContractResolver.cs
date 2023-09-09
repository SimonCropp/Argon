// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Used by <see cref="JsonSerializer" /> to resolve a <see cref="JsonContract" /> for a given <see cref="System.Type" />.
/// </summary>
public class DefaultContractResolver : IContractResolver
{
    // Json.NET Schema requires a property
    internal static IContractResolver Instance { get; } = new DefaultContractResolver();

    readonly DefaultJsonNameTable nameTable = new();

    readonly ThreadSafeStore<Type, JsonContract> contractCache;

    /// <summary>
    /// Gets or sets a value indicating whether compiler generated members should be serialized.
    /// </summary>
    public bool SerializeCompilerGeneratedMembers { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to ignore IsSpecified members when serializing and deserializing types.
    /// </summary>
    public bool IgnoreIsSpecifiedMembers { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to ignore ShouldSerialize members when serializing and deserializing types.
    /// </summary>
    public bool IgnoreShouldSerializeMembers { get; set; }

    /// <summary>
    /// Gets or sets the naming strategy used to resolve how property names and dictionary keys are serialized.
    /// </summary>
    public NamingStrategy? NamingStrategy { get; set; }

    public static List<JsonConverter> Converters { get; } =
        new()
        {
            new StringBuilderConverter(),
            new ExpandoObjectConverter(),
            new KeyValuePairConverter(),
            new DriveInfoConverter(),
            new EncodingConverter(),
            new PathInfoConverter(),
            new RegexConverter(),
            new EncodingConverter(),
            new TimeZoneInfoConverter(),
            new VersionConverter(),
            new StringWriterConverter()
        };

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultContractResolver" /> class.
    /// </summary>
    public DefaultContractResolver() =>
        contractCache = new(CreateContract);

    /// <summary>
    /// Resolves the contract for a given type.
    /// </summary>
    /// <param name="type">The type to resolve a contract for.</param>
    /// <returns>The contract for a given type.</returns>
    public virtual JsonContract ResolveContract(Type type) =>
        contractCache.Get(type);

    static bool FilterMembers(MemberInfo member)
    {
        if (member is PropertyInfo property)
        {
            if (property.IsIndexedProperty())
            {
                return false;
            }

            return !property.PropertyType.IsByRefLikeType();
        }

        if (member is FieldInfo field)
        {
            return !field.FieldType.IsByRefLikeType();
        }

        return true;
    }

    /// <summary>
    /// Gets the serializable members for the type.
    /// </summary>
    /// <param name="type">The type to get serializable members for.</param>
    /// <returns>The serializable members for the type.</returns>
    protected virtual IEnumerable<MemberInfo> GetSerializableMembers(Type type)
    {
        var memberSerialization = JsonTypeReflector.GetObjectMemberSerialization(type);

        if (memberSerialization == MemberSerialization.Fields)
        {
            // Do not filter ByRef types here because accessing FieldType/PropertyType can trigger additional assembly loads
            return type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        var serializableMembers = new List<MemberInfo>();
        var dataContractAttribute = JsonTypeReflector.GetDataContractAttribute(type);

        // Exclude index properties and ByRef types
        var defaultMembers = type.GetFieldsAndProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(FilterMembers).ToList();

        // Do not filter ByRef types here because accessing FieldType/PropertyType can trigger additional assembly loads
        foreach (var member in type.GetFieldsAndProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
        {
            if (!(member is not PropertyInfo p || !p.IsIndexedProperty()))
            {
                continue;
            }

            if (!ShouldSerialize(member, defaultMembers, dataContractAttribute))
            {
                continue;
            }

            serializableMembers.Add(member);
        }

        // don't include TargetSite on non-serializable exceptions
        // MemberBase is problematic to serialize. Large, self referencing instances, etc
        if (typeof(Exception).IsAssignableFrom(type))
        {
            return serializableMembers.Where(m => !string.Equals(m.Name, "TargetSite", StringComparison.Ordinal));
        }

        return serializableMembers;
    }

    bool ShouldSerialize(MemberInfo member, List<MemberInfo> defaultMembers, DataContractAttribute? dataContractAttribute)
    {
        // exclude members that are compiler generated if set
        if (!SerializeCompilerGeneratedMembers && member.IsDefined(typeof(CompilerGeneratedAttribute), true))
        {
            return false;
        }

        if (defaultMembers.Contains(member))
        {
            // add all members that are found by default member search
            return true;
        }

        // add members that are explicitly marked with JsonProperty/DataMember attribute
        // or are a field if serializing just fields
        if (member.GetAttribute<JsonPropertyAttribute>() != null)
        {
            return true;
        }

        if (member.GetAttribute<JsonRequiredAttribute>() != null)
        {
            return true;
        }

        if (dataContractAttribute != null && member.GetAttribute<DataMemberAttribute>() != null)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Creates a <see cref="JsonObjectContract" /> for the given type.
    /// </summary>
    /// <returns>A <see cref="JsonObjectContract" /> for the given type.</returns>
    protected virtual JsonObjectContract CreateObjectContract(Type type)
    {
        var contract = new JsonObjectContract(type);
        InitializeContract(contract);

        contract.MemberSerialization = JsonTypeReflector.GetObjectMemberSerialization(contract.NonNullableUnderlyingType);
        contract.Properties.AddRange(CreateProperties(contract.NonNullableUnderlyingType, contract.MemberSerialization));

        DictionaryKeyResolver? extensionDataNameResolver = null;

        var attribute = AttributeCache<JsonObjectAttribute>.GetAttribute(contract.NonNullableUnderlyingType);
        if (attribute != null)
        {
            contract.ItemRequired = attribute.itemRequired;
            contract.ItemNullValueHandling = attribute.itemNullValueHandling;
        }

        extensionDataNameResolver ??= ResolveExtensionDataName;

        contract.ExtensionDataNameResolver = extensionDataNameResolver;

        if (contract.IsInstantiable)
        {
            var overrideConstructor = GetAttributeConstructor(contract.NonNullableUnderlyingType);

            // check if a JsonConstructorAttribute has been defined and use that
            if (overrideConstructor != null)
            {
                contract.OverrideCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(overrideConstructor);
                contract.CreatorParameters.AddRange(CreateConstructorParameters(overrideConstructor, contract.Properties));
            }
            else if (contract.MemberSerialization == MemberSerialization.Fields)
            {
                // mimic DataContractSerializer behaviour when populating fields by overriding default creator to create an uninitialized object
                contract.DefaultCreator = contract.GetUninitializedObject;
            }
            else if (contract.DefaultCreator == null || contract.DefaultCreatorNonPublic)
            {
                var constructor = GetParameterizedConstructor(contract.NonNullableUnderlyingType);
                if (constructor != null)
                {
                    contract.ParameterizedCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(constructor);
                    contract.CreatorParameters.AddRange(CreateConstructorParameters(constructor, contract.Properties));
                }
            }
            else if (contract.NonNullableUnderlyingType.IsValueType)
            {
                // value types always have default constructor
                // check whether there is a constructor that matches with non-writable properties on value type
                var constructor = GetImmutableConstructor(contract.NonNullableUnderlyingType, contract.Properties);
                if (constructor != null)
                {
                    contract.OverrideCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(constructor);
                    contract.CreatorParameters.AddRange(CreateConstructorParameters(constructor, contract.Properties));
                }
            }
        }

        var extensionDataMember = GetExtensionDataMemberForType(contract.NonNullableUnderlyingType);
        if (extensionDataMember != null)
        {
            SetExtensionDataDelegates(contract, extensionDataMember);
        }

        return contract;
    }

    static MemberInfo? GetExtensionDataMemberForType(Type type)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        var current = type;
        while (current != null && current != typeof(object))
        {
            foreach (var field in current.GetFields(flags))
            {
                if (IsExtensionDataMember(field))
                {
                    return field;
                }
            }

            foreach (var property in current.GetProperties(flags))
            {
                if (IsExtensionDataMember(property))
                {
                    return property;
                }
            }

            current = current.BaseType;
        }

        return null;
    }

    static bool IsExtensionDataMember(MemberInfo member)
    {
        // last instance of attribute wins on type if there are multiple
        if (!member.IsDefined(typeof(JsonExtensionDataAttribute), false))
        {
            return false;
        }

        if (!member.CanReadMemberValue(true))
        {
            throw new JsonException($"Invalid extension data attribute on '{GetClrTypeFullName(member.DeclaringType!)}'. Member '{member.Name}' must have a getter.");
        }

        var memberType = member.GetMemberUnderlyingType();

        if (memberType.ImplementsGenericDefinition(typeof(IDictionary<,>), out var dictionaryType))
        {
            var genericArguments = dictionaryType.GetGenericArguments();
            var keyType = genericArguments[0];
            var valueType = genericArguments[1];

            if (keyType.IsAssignableFrom(typeof(string)) &&
                valueType.IsAssignableFrom(typeof(JToken)))
            {
                return true;
            }
        }

        throw new JsonException($"Invalid extension data attribute on '{GetClrTypeFullName(member.DeclaringType!)}'. Member '{member.Name}' type must implement IDictionary<string, JToken>.");
    }

    static void SetExtensionDataDelegates(JsonObjectContract contract, MemberInfo member)
    {
        var extensionDataAttribute = member.GetCustomAttribute<JsonExtensionDataAttribute>(true);
        if (extensionDataAttribute == null)
        {
            return;
        }

        var type = member.GetMemberUnderlyingType();

        if (!type.ImplementsGenericDefinition(typeof(IDictionary<,>), out var dictionaryType))
        {
            throw new JsonSerializationException($"Cannot use '{member.Name}' for extension data. It must be a IDictionary<,>.");
        }

        var keyType = dictionaryType.GetGenericArguments()[0];
        var valueType = dictionaryType.GetGenericArguments()[1];

        var getExtensionDataDictionary = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(member);

        if (extensionDataAttribute.ReadData)
        {
            Type createdType;

            // change type to a class if it is the base interface so it can be instantiated if needed
            if (ReflectionUtils.IsGenericDefinition(type, typeof(IDictionary<,>)))
            {
                createdType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            }
            else
            {
                createdType = type;
            }

            var setExtensionDataDictionary = BuildSetExtensionDataDictionary(member);
            var createExtensionDataDictionary = JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(createdType);
            var flags = BindingFlags.Public | BindingFlags.Instance;
            var setMethod = type.GetProperty(
                "Item",
                flags, null, valueType,
                new[]
                {
                    keyType
                },
                null)?.SetMethod;

            if (setMethod == null)
            {
                // Item is explicitly implemented and non-public
                // get from dictionary interface
                setMethod = dictionaryType.GetProperty(
                    "Item",
                    flags,
                    null,
                    valueType,
                    new[]
                    {
                        keyType
                    },
                    null)?.SetMethod;
            }

            var setExtensionDataDictionaryValue = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(setMethod!);

            contract.ExtensionDataSetter = (o, key, value) =>
            {
                var dictionary = getExtensionDataDictionary(o);
                if (dictionary == null)
                {
                    if (setExtensionDataDictionary == null)
                    {
                        throw new JsonSerializationException($"Cannot set value onto extension data member '{member.Name}'. The extension data collection is null and it cannot be set.");
                    }

                    dictionary = createExtensionDataDictionary();
                    setExtensionDataDictionary(o, dictionary);
                }

                setExtensionDataDictionaryValue(dictionary, key, value);
            };
        }

        if (extensionDataAttribute.WriteData)
        {
            var enumerableWrapper = typeof(EnumerableDictionaryWrapper<,>).MakeGenericType(keyType, valueType);
            var constructors = enumerableWrapper.GetConstructors()[0];
            var createEnumerableWrapper = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(constructors);

            contract.ExtensionDataGetter = o =>
            {
                var dictionary = getExtensionDataDictionary(o);
                if (dictionary == null)
                {
                    return null;
                }

                return (IEnumerable<KeyValuePair<object, object>>) createEnumerableWrapper(dictionary);
            };
        }

        contract.ExtensionDataValueType = valueType;
    }

    static Action<object, object?>? BuildSetExtensionDataDictionary(MemberInfo member)
    {
        if (member.CanSetMemberValue(true, false))
        {
            return JsonTypeReflector.ReflectionDelegateFactory.CreateSet<object>(member);
        }

        return null;
    }

    // leave as class instead of struct
    // will be always return as an interface and boxed
    class EnumerableDictionaryWrapper<TEnumeratorKey, TEnumeratorValue>(IEnumerable<KeyValuePair<TEnumeratorKey, TEnumeratorValue>> e)
        : IEnumerable<KeyValuePair<object, object>>
    {
        public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
        {
            foreach (var item in e)
            {
                yield return new(item.Key!, item.Value!);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }

    static ConstructorInfo? GetAttributeConstructor(Type type)
    {
        using var enumerator = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(_ => _.IsDefined(typeof(JsonConstructorAttribute), true))
            .GetEnumerator();
        if (enumerator.MoveNext())
        {
            var conInfo = enumerator.Current;
            if (enumerator.MoveNext())
            {
                throw new JsonException("Multiple constructors with the JsonConstructorAttribute.");
            }

            return conInfo;
        }

        // little hack to get Version objects to deserialize correctly
        if (type == typeof(Version))
        {
            return type.GetConstructor(new[]
            {
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(int)
            });
        }

        return null;
    }

    static ConstructorInfo? GetImmutableConstructor(Type type, JsonPropertyCollection memberProperties)
    {
        foreach (var constructor in type.GetConstructors())
        {
            var parameters = constructor.GetParameters();
            if (parameters.Length <= 0)
            {
                continue;
            }

            foreach (var parameter in parameters)
            {
                var memberProperty = MatchProperty(memberProperties, parameter.Name, parameter.ParameterType);
                if (memberProperty == null || memberProperty.Writable)
                {
                    return null;
                }
            }

            return constructor;
        }

        return null;
    }

    static ConstructorInfo? GetParameterizedConstructor(Type type)
    {
        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        if (constructors.Length == 1)
        {
            return constructors[0];
        }

        return null;
    }

    /// <summary>
    /// Creates the constructor parameters.
    /// </summary>
    /// <param name="constructor">The constructor to create properties for.</param>
    /// <param name="memberProperties">The type's member properties.</param>
    /// <returns>Properties for the given <see cref="ConstructorInfo" />.</returns>
    protected virtual IList<JsonProperty> CreateConstructorParameters(ConstructorInfo constructor, JsonPropertyCollection memberProperties)
    {
        var constructorParameters = constructor.GetParameters();

        var parameterCollection = new JsonPropertyCollection(constructor.DeclaringType!);

        foreach (var parameterInfo in constructorParameters)
        {
            var matchingMemberProperty = MatchProperty(memberProperties, parameterInfo.Name, parameterInfo.ParameterType);

            // ensure that property will have a name from matching property or from parameterInfo
            // parameterInfo could have no name if generated by a proxy (I'm looking at you Castle)
            if (matchingMemberProperty == null &&
                parameterInfo.Name == null)
            {
                continue;
            }

            var property = CreatePropertyFromConstructorParameter(matchingMemberProperty, parameterInfo);
            parameterCollection.AddProperty(property);
        }

        return parameterCollection;
    }

    static JsonProperty? MatchProperty(JsonPropertyCollection properties, string? name, Type type)
    {
        // it is possible to generate a member with a null name using Reflection.Emit
        // protect against an ArgumentNullException from GetClosestMatchProperty by testing for null here
        if (name == null)
        {
            return null;
        }

        return properties.GetClosestMatchProperty(name, type);
    }

    /// <summary>
    /// Creates a <see cref="JsonProperty" /> for the given <see cref="ParameterInfo" />.
    /// </summary>
    /// <param name="matchingMemberProperty">The matching member property.</param>
    /// <param name="parameterInfo">The constructor parameter.</param>
    /// <returns>A created <see cref="JsonProperty" /> for the given <see cref="ParameterInfo" />.</returns>
    protected virtual JsonProperty CreatePropertyFromConstructorParameter(JsonProperty? matchingMemberProperty, ParameterInfo parameterInfo)
    {
        var declaringType = parameterInfo.Member.DeclaringType!;
        var property = new JsonProperty(parameterInfo.ParameterType, declaringType);

        SetPropertySettingsFromAttributes(property, parameterInfo, parameterInfo.Name!, declaringType, MemberSerialization.OptOut, out _);

        property.Readable = false;
        property.Writable = true;

        // "inherit" values from matching member property if unset on parameter
        if (matchingMemberProperty != null)
        {
            property.PropertyName = property.PropertyName != parameterInfo.Name ? property.PropertyName : matchingMemberProperty.PropertyName;
            property.Converter ??= matchingMemberProperty.Converter;

            if (!property.hasExplicitDefaultValue && matchingMemberProperty.hasExplicitDefaultValue)
            {
                property.DefaultValue = matchingMemberProperty.DefaultValue;
            }

            property.required ??= matchingMemberProperty.required;
            property.IsReference ??= matchingMemberProperty.IsReference;
            property.NullValueHandling ??= matchingMemberProperty.NullValueHandling;
            property.DefaultValueHandling ??= matchingMemberProperty.DefaultValueHandling;
            property.ReferenceLoopHandling ??= matchingMemberProperty.ReferenceLoopHandling;
            property.ObjectCreationHandling ??= matchingMemberProperty.ObjectCreationHandling;
            property.TypeNameHandling ??= matchingMemberProperty.TypeNameHandling;
        }

        return property;
    }

    /// <summary>
    /// Resolves the default <see cref="JsonConverter" /> for the contract.
    /// </summary>
    /// <returns>The contract's default <see cref="JsonConverter" />.</returns>
    protected virtual JsonConverter? ResolveContractConverter(Type type) =>
        JsonTypeReflector.GetJsonConverter(type);

    static Func<object> GetDefaultCreator(Type createdType) =>
        JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(createdType);

    void InitializeContract(JsonContract contract)
    {
        var nonNullableUnderlyingType = contract.NonNullableUnderlyingType;
        var containerAttribute = AttributeCache<JsonContainerAttribute>.GetAttribute(nonNullableUnderlyingType);
        if (containerAttribute == null)
        {
            var dataContractAttribute = JsonTypeReflector.GetDataContractAttribute(nonNullableUnderlyingType);
            // doesn't have a null value
            if (dataContractAttribute is {IsReference: true})
            {
                contract.IsReference = true;
            }
        }
        else
        {
            contract.IsReference = containerAttribute.isReference;
        }

        contract.Converter = ResolveContractConverter(nonNullableUnderlyingType);

        // then see whether object is compatible with any of the built in converters
        contract.InternalConverter = JsonSerializer.GetMatchingConverter(Converters, nonNullableUnderlyingType);

        var createdType = contract.CreatedType;
        if (!contract.IsInstantiable)
        {
            return;
        }

        if (createdType.IsValueType)
        {
            contract.DefaultCreator = GetDefaultCreator(createdType);
            contract.DefaultCreatorNonPublic = false;
            return;
        }

        var constructor = createdType.GetDefaultConstructor(nonPublic: true);
        if (constructor != null)
        {
            contract.DefaultCreator = GetDefaultCreator(createdType);
            contract.DefaultCreatorNonPublic = !constructor.IsPublic;
        }
    }

    /// <summary>
    /// Creates a <see cref="JsonDictionaryContract" /> for the given type.
    /// </summary>
    /// <returns>A <see cref="JsonDictionaryContract" /> for the given type.</returns>
    protected virtual JsonDictionaryContract CreateDictionaryContract(Type type)
    {
        var contract = new JsonDictionaryContract(type);
        InitializeContract(contract);

        contract.DictionaryKeyResolver = ResolveDictionaryKey;

        var overrideConstructor = GetAttributeConstructor(contract.NonNullableUnderlyingType);

        if (overrideConstructor != null)
        {
            var parameters = overrideConstructor.GetParameters();
            var expectedParameterType = contract is {DictionaryKeyType: { }, DictionaryValueType: { }}
                ? typeof(IEnumerable<>).MakeGenericType(typeof(KeyValuePair<,>).MakeGenericType(contract.DictionaryKeyType, contract.DictionaryValueType))
                : typeof(IDictionary);

            if (parameters.Length == 0)
            {
                contract.HasParameterizedCreator = false;
            }
            else if (parameters.Length == 1 && expectedParameterType.IsAssignableFrom(parameters[0].ParameterType))
            {
                contract.HasParameterizedCreator = true;
            }
            else
            {
                throw new JsonException($"Constructor for '{contract.UnderlyingType}' must have no parameters or a single parameter that implements '{expectedParameterType}'.");
            }

            contract.OverrideCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(overrideConstructor);
        }

        return contract;
    }

    /// <summary>
    /// Creates a <see cref="JsonArrayContract" /> for the given type.
    /// </summary>
    /// <returns>A <see cref="JsonArrayContract" /> for the given type.</returns>
    protected virtual JsonArrayContract CreateArrayContract(Type type)
    {
        var contract = new JsonArrayContract(type);
        InitializeContract(contract);

        var overrideConstructor = GetAttributeConstructor(contract.NonNullableUnderlyingType);

        if (overrideConstructor != null)
        {
            var parameters = overrideConstructor.GetParameters();
            var expectedParameterType = contract.CollectionItemType == null
                ? typeof(IEnumerable)
                : typeof(IEnumerable<>).MakeGenericType(contract.CollectionItemType);

            if (parameters.Length == 0)
            {
                contract.HasParameterizedCreator = false;
            }
            else if (parameters.Length == 1 && expectedParameterType.IsAssignableFrom(parameters[0].ParameterType))
            {
                contract.HasParameterizedCreator = true;
            }
            else
            {
                throw new JsonException($"Constructor for '{contract.UnderlyingType}' must have no parameters or a single parameter that implements '{expectedParameterType}'.");
            }

            contract.OverrideCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(overrideConstructor);
        }

        return contract;
    }

    /// <summary>
    /// Creates a <see cref="JsonPrimitiveContract" /> for the given type.
    /// </summary>
    /// <returns>A <see cref="JsonPrimitiveContract" /> for the given type.</returns>
    protected virtual JsonPrimitiveContract CreatePrimitiveContract(Type type)
    {
        var contract = new JsonPrimitiveContract(type);
        InitializeContract(contract);

        return contract;
    }

    /// <summary>
    /// Creates a <see cref="JsonLinqContract" /> for the given type.
    /// </summary>
    /// <returns>A <see cref="JsonLinqContract" /> for the given type.</returns>
    protected virtual JsonLinqContract CreateLinqContract(Type type)
    {
        var contract = new JsonLinqContract(type);
        InitializeContract(contract);

        return contract;
    }

    /// <summary>
    /// Creates a <see cref="JsonDynamicContract" /> for the given type.
    /// </summary>
    /// <returns>A <see cref="JsonDynamicContract" /> for the given type.</returns>
    protected virtual JsonDynamicContract CreateDynamicContract(Type type)
    {
        var contract = new JsonDynamicContract(type);
        InitializeContract(contract);

        contract.PropertyNameResolver = name => ResolveDictionaryKey(name, name);

        contract.Properties.AddRange(CreateProperties(type, MemberSerialization.OptOut));

        return contract;
    }

    /// <summary>
    /// Creates a <see cref="JsonStringContract" /> for the given type.
    /// </summary>
    /// <returns>A <see cref="JsonStringContract" /> for the given type.</returns>
    protected virtual JsonStringContract CreateStringContract(Type type)
    {
        var contract = new JsonStringContract(type);
        InitializeContract(contract);

        return contract;
    }

    /// <summary>
    /// Determines which contract type is created for the given type.
    /// </summary>
    /// <returns>A <see cref="JsonContract" /> for the given type.</returns>
    protected virtual JsonContract CreateContract(Type type)
    {
        var t = type.EnsureNotByRefType();

        if (IsJsonPrimitiveType(t))
        {
            return CreatePrimitiveContract(type);
        }

        t = t.EnsureNotNullableType();
        var containerAttribute = AttributeCache<JsonContainerAttribute>.GetAttribute(t);

        if (containerAttribute is JsonObjectAttribute)
        {
            return CreateObjectContract(type);
        }

        if (containerAttribute is JsonArrayAttribute)
        {
            return CreateArrayContract(type);
        }

        if (containerAttribute is JsonDictionaryAttribute)
        {
            return CreateDictionaryContract(type);
        }

        if (t == typeof(JToken) || t.IsSubclassOf(typeof(JToken)))
        {
            return CreateLinqContract(type);
        }

        if (t.IsDictionary())
        {
            return CreateDictionaryContract(type);
        }

        if (typeof(IEnumerable).IsAssignableFrom(t))
        {
            return CreateArrayContract(type);
        }

        if (CanConvertToString(t))
        {
            return CreateStringContract(type);
        }

        if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(t))
        {
            return CreateDynamicContract(type);
        }

        // tested last because it is not possible to automatically deserialize custom IConvertible types
        if (IsIConvertible(t))
        {
            return CreatePrimitiveContract(t);
        }

        return CreateObjectContract(type);
    }

    static bool IsJsonPrimitiveType(Type type)
    {
        var typeCode = ConvertUtils.GetTypeCode(type);

        return typeCode != PrimitiveTypeCode.Empty && typeCode != PrimitiveTypeCode.Object;
    }

    static bool IsIConvertible(Type t)
    {
        if (typeof(IConvertible).IsAssignableFrom(t)
            || (t.IsNullableType() && typeof(IConvertible).IsAssignableFrom(Nullable.GetUnderlyingType(t))))
        {
            return !typeof(JToken).IsAssignableFrom(t);
        }

        return false;
    }

    static bool CanConvertToString(Type type)
    {
        if (JsonTypeReflector.TryGetStringConverter(type, out _))
        {
            return true;
        }
#if NET6_0_OR_GREATER
        if (type == typeof(Date) ||
            type == typeof(Time))
        {
            return true;
        }
#endif
        return type == typeof(Type) ||
               type.IsSubclassOf(typeof(Type));
    }

    static string GetClrTypeFullName(Type type)
    {
        if (type.IsGenericTypeDefinition ||
            !type.ContainsGenericParameters)
        {
            return type.FullName!;
        }

        return $"{type.Namespace}.{type.Name}";
    }

    /// <summary>
    /// Creates properties for the given <see cref="JsonContract" />.
    /// </summary>
    /// <param name="type">The type to create properties for.</param>
    /// <param name="memberSerialization">The member serialization mode for the type.</param>
    /// <returns>Properties for the given <see cref="JsonContract" />.</returns>
    protected virtual IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var members = GetSerializableMembers(type);
        if (members == null)
        {
            throw new JsonSerializationException("Null collection of serializable members returned.");
        }

        var nameTable = GetNameTable();

        var properties = new JsonPropertyCollection(type);

        foreach (var member in members)
        {
            var property = CreateProperty(member, memberSerialization);

            // nametable is not thread-safe for multiple writers
            lock (nameTable)
            {
                property.PropertyName = nameTable.Add(property.PropertyName!);
            }

            properties.AddProperty(property);
        }

        return properties.OrderBy(p => p.Order ?? -1).ToList();
    }

    internal virtual DefaultJsonNameTable GetNameTable() =>
        nameTable;

    /// <summary>
    /// Creates the <see cref="IValueProvider" /> used by the serializer to get and set values from a member.
    /// </summary>
    /// <returns>The <see cref="IValueProvider" /> used by the serializer to get and set values from a member.</returns>
    protected virtual IValueProvider CreateMemberValueProvider(MemberInfo member)
    {
        // warning - this method use to cause errors with Intellitrace. Retest in VS Ultimate after changes

#if !(NETSTANDARD2_0)
        return new DynamicValueProvider(member);
#else
        return new ExpressionValueProvider(member);
#endif
    }

    /// <summary>
    /// Creates a <see cref="JsonProperty" /> for the given <see cref="MemberInfo" />.
    /// </summary>
    /// <param name="memberSerialization">The member's parent <see cref="MemberSerialization" />.</param>
    /// <param name="member">The member to create a <see cref="JsonProperty" /> for.</param>
    /// <returns>A created <see cref="JsonProperty" /> for the given <see cref="MemberInfo" />.</returns>
    protected virtual JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var declaringType = member.DeclaringType!;
        var property = new JsonProperty(member.GetMemberUnderlyingType(), declaringType)
        {
            ValueProvider = CreateMemberValueProvider(member)
        };

        SetPropertySettingsFromAttributes(property, member, member.Name, declaringType, memberSerialization, out var allowNonPublicAccess);

        if (memberSerialization == MemberSerialization.Fields)
        {
            // write to readonly fields
            property.Readable = true;
            property.Writable = true;
        }
        else
        {
            property.Readable = member.CanReadMemberValue(allowNonPublicAccess);
            property.Writable = member.CanSetMemberValue(allowNonPublicAccess, property.HasMemberAttribute);
        }

        if (!IgnoreShouldSerializeMembers)
        {
            property.ShouldSerialize = CreateShouldSerializeTest(member);
        }

        if (!IgnoreIsSpecifiedMembers)
        {
            SetIsSpecifiedActions(property, member, allowNonPublicAccess);
        }

        return property;
    }

    void SetPropertySettingsFromAttributes(JsonProperty property, ICustomAttributeProvider attributeProvider, string name, Type declaringType, MemberSerialization memberSerialization, out bool allowNonPublicAccess)
    {
        var dataContractAttribute = JsonTypeReflector.GetDataContractAttribute(declaringType);

        var member = attributeProvider as MemberInfo;

        DataMemberAttribute? dataMemberAttribute;
        if (dataContractAttribute == null || member == null)
        {
            dataMemberAttribute = null;
        }
        else
        {
            dataMemberAttribute = JsonTypeReflector.GetDataMemberAttribute(member);
        }

        var propertyAttribute = JsonTypeReflector.GetAttribute<JsonPropertyAttribute>(attributeProvider);
        var requiredAttribute = JsonTypeReflector.GetAttribute<JsonRequiredAttribute>(attributeProvider);

        string mappedName;
        bool hasSpecifiedName;
        if (propertyAttribute?.PropertyName != null)
        {
            mappedName = propertyAttribute.PropertyName;
            hasSpecifiedName = true;
        }
        else if (dataMemberAttribute?.Name != null)
        {
            mappedName = dataMemberAttribute.Name;
            hasSpecifiedName = true;
        }
        else
        {
            mappedName = name;
            hasSpecifiedName = false;
        }

        NamingStrategy? namingStrategy;
        if (propertyAttribute?.NamingStrategyType == null)
        {
            namingStrategy = NamingStrategy;
        }
        else
        {
            namingStrategy = JsonTypeReflector.CreateNamingStrategyInstance(propertyAttribute.NamingStrategyType, propertyAttribute.NamingStrategyParameters);
        }

        if (namingStrategy == null)
        {
            property.PropertyName = ResolvePropertyName(mappedName);
        }
        else
        {
            property.PropertyName = namingStrategy.GetPropertyName(mappedName, hasSpecifiedName);
        }

        property.UnderlyingName = name;

        var hasMemberAttribute = false;
        if (propertyAttribute == null)
        {
            property.NullValueHandling = null;
            property.ReferenceLoopHandling = null;
            property.ObjectCreationHandling = null;
            property.TypeNameHandling = null;
            property.IsReference = null;
            property.ItemIsReference = null;
            property.ItemConverter = null;
            property.ItemReferenceLoopHandling = null;
            property.ItemTypeNameHandling = null;
            if (dataMemberAttribute != null)
            {
                property.required = dataMemberAttribute.IsRequired ? Required.AllowNull : Required.Default;
                property.Order = dataMemberAttribute.Order == -1 ? null : dataMemberAttribute.Order;
                property.DefaultValueHandling = !dataMemberAttribute.EmitDefaultValue ? DefaultValueHandling.Ignore : null;
                hasMemberAttribute = true;
            }
        }
        else
        {
            property.required = propertyAttribute.required;
            property.Order = propertyAttribute.order;
            property.DefaultValueHandling = propertyAttribute.defaultValueHandling;
            hasMemberAttribute = true;
            property.NullValueHandling = propertyAttribute.nullValueHandling;
            property.ReferenceLoopHandling = propertyAttribute.referenceLoopHandling;
            property.ObjectCreationHandling = propertyAttribute.objectCreationHandling;
            property.TypeNameHandling = propertyAttribute.typeNameHandling;
            property.IsReference = propertyAttribute.isReference;

            property.ItemIsReference = propertyAttribute.itemIsReference;
            property.ItemConverter = propertyAttribute.ItemConverterType == null ? null : JsonTypeReflector.CreateJsonConverterInstance(propertyAttribute.ItemConverterType, propertyAttribute.ItemConverterParameters);
            property.ItemReferenceLoopHandling = propertyAttribute.itemReferenceLoopHandling;
            property.ItemTypeNameHandling = propertyAttribute.itemTypeNameHandling;
        }

        if (requiredAttribute != null)
        {
            property.required = Required.Always;
            hasMemberAttribute = true;
        }

        property.HasMemberAttribute = hasMemberAttribute;

        var hasJsonIgnoreAttribute =
            JsonTypeReflector.GetAttribute<JsonIgnoreAttribute>(attributeProvider) != null
            // automatically ignore extension data dictionary property if it is public
            || JsonTypeReflector.GetAttribute<JsonExtensionDataAttribute>(attributeProvider) != null;

        if (memberSerialization == MemberSerialization.OptIn)
        {
            // ignored if it has JsonIgnore/NonSerialized or does not have DataMember or JsonProperty attributes
            property.Ignored = hasJsonIgnoreAttribute || !hasMemberAttribute;
        }
        else
        {
            var hasIgnoreDataMemberAttribute = JsonTypeReflector.GetAttribute<IgnoreDataMemberAttribute>(attributeProvider) != null;

            // ignored if it has JsonIgnore or NonSerialized or IgnoreDataMember attributes
            property.Ignored = hasJsonIgnoreAttribute || hasIgnoreDataMemberAttribute;
        }

        // resolve converter for property
        // the class type might have a converter but the property converter takes precedence
        property.Converter = JsonTypeReflector.GetJsonConverter(attributeProvider);

        var defaultValueAttribute = JsonTypeReflector.GetAttribute<DefaultValueAttribute>(attributeProvider);
        if (defaultValueAttribute != null)
        {
            property.DefaultValue = defaultValueAttribute.Value;
        }

        allowNonPublicAccess = hasMemberAttribute ||
                               memberSerialization == MemberSerialization.Fields;
    }

    static Predicate<object>? CreateShouldSerializeTest(MemberInfo member)
    {
        var shouldSerializeMethod = member.DeclaringType!.GetMethod(JsonTypeReflector.ShouldSerializePrefix + member.Name, Type.EmptyTypes);

        if (shouldSerializeMethod == null || shouldSerializeMethod.ReturnType != typeof(bool))
        {
            return null;
        }

        var shouldSerializeCall =
            JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(shouldSerializeMethod);

        return o => (bool) shouldSerializeCall(o)!;
    }

    static void SetIsSpecifiedActions(JsonProperty property, MemberInfo member, bool allowNonPublicAccess)
    {
        var declaringType = member.DeclaringType!;
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        MemberInfo? specifiedMember = declaringType.GetProperty(member.Name + JsonTypeReflector.SpecifiedPostfix, flags);
        if (specifiedMember == null)
        {
            specifiedMember = declaringType.GetField(member.Name + JsonTypeReflector.SpecifiedPostfix, flags);
        }

        if (specifiedMember == null ||
            specifiedMember.GetMemberUnderlyingType() != typeof(bool))
        {
            return;
        }

        Func<object, object> specifiedPropertyGet = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(specifiedMember)!;

        property.GetIsSpecified = o => (bool) specifiedPropertyGet(o);

        if (specifiedMember.CanSetMemberValue(allowNonPublicAccess, false))
        {
            property.SetIsSpecified = JsonTypeReflector.ReflectionDelegateFactory.CreateSet<object>(specifiedMember);
        }
    }

    /// <summary>
    /// Resolves the name of the property.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Resolved name of the property.</returns>
    protected virtual string ResolvePropertyName(string propertyName)
    {
        if (NamingStrategy == null)
        {
            return propertyName;
        }

        return NamingStrategy.GetPropertyName(propertyName, false);
    }

    /// <summary>
    /// Resolves the name of the extension data. By default no changes are made to extension data names.
    /// </summary>
    /// <param name="extensionDataName">Name of the extension data.</param>
    /// <returns>Resolved name of the extension data.</returns>
    protected virtual string ResolveExtensionDataName(string extensionDataName, object original)
    {
        if (NamingStrategy == null)
        {
            return extensionDataName;
        }

        return NamingStrategy.GetExtensionDataName(extensionDataName);
    }

    /// <summary>
    /// Resolves the key of the dictionary. By default <see cref="ResolvePropertyName" /> is used to resolve dictionary keys.
    /// </summary>
    /// <param name="name">Key of the dictionary.</param>
    /// <returns>Resolved key of the dictionary.</returns>
    protected virtual string ResolveDictionaryKey(string name, object original)
    {
        if (NamingStrategy == null)
        {
            return ResolvePropertyName(name);
        }

        return NamingStrategy.GetDictionaryKey(name, original);
    }

    /// <summary>
    /// Gets the resolved name of the property.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Name of the property.</returns>
    public string GetResolvedPropertyName(string propertyName) =>
        // this is a new method rather than changing the visibility of ResolvePropertyName to avoid
        // a breaking change for anyone who has overridden the method
        ResolvePropertyName(propertyName);
}