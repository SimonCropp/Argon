#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System.ComponentModel;
using System.Dynamic;

namespace Argon;

/// <summary>
/// Used by <see cref="JsonSerializer"/> to resolve a <see cref="JsonContract"/> for a given <see cref="System.Type"/>.
/// </summary>
public class DefaultContractResolver : IContractResolver
{
    // Json.NET Schema requires a property
    internal static IContractResolver Instance { get; } = new DefaultContractResolver();

    static readonly string[] BlacklistedTypeNames =
    {
        "System.IO.DriveInfo",
        "System.IO.FileInfo",
        "System.IO.DirectoryInfo"
    };

    static readonly JsonConverter[] BuiltInConverters =
    {
        new ExpandoObjectConverter(),
        new BinaryConverter(),
        new DiscriminatedUnionConverter(),
        new KeyValuePairConverter(),
        new RegexConverter()
    };

    readonly DefaultJsonNameTable _nameTable = new();

    readonly ThreadSafeStore<Type, JsonContract> _contractCache;

    /// <summary>
    /// Gets or sets a value indicating whether compiler generated members should be serialized.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if serialized compiler generated members; otherwise, <c>false</c>.
    /// </value>
    public bool SerializeCompilerGeneratedMembers { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to ignore IsSpecified members when serializing and deserializing types.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the IsSpecified members will be ignored when serializing and deserializing types; otherwise, <c>false</c>.
    /// </value>
    public bool IgnoreIsSpecifiedMembers { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to ignore ShouldSerialize members when serializing and deserializing types.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the ShouldSerialize members will be ignored when serializing and deserializing types; otherwise, <c>false</c>.
    /// </value>
    public bool IgnoreShouldSerializeMembers { get; set; }

    /// <summary>
    /// Gets or sets the naming strategy used to resolve how property names and dictionary keys are serialized.
    /// </summary>
    /// <value>The naming strategy used to resolve how property names and dictionary keys are serialized.</value>
    public NamingStrategy? NamingStrategy { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultContractResolver"/> class.
    /// </summary>
    public DefaultContractResolver()
    {
        _contractCache = new ThreadSafeStore<Type, JsonContract>(CreateContract);
    }

    /// <summary>
    /// Resolves the contract for a given type.
    /// </summary>
    /// <param name="type">The type to resolve a contract for.</param>
    /// <returns>The contract for a given type.</returns>
    public virtual JsonContract ResolveContract(Type type)
    {
        return _contractCache.Get(type);
    }

    static bool FilterMembers(MemberInfo member)
    {
        if (member is PropertyInfo property)
        {
            if (ReflectionUtils.IsIndexedProperty(property))
            {
                return false;
            }

            return !ReflectionUtils.IsByRefLikeType(property.PropertyType);
        }

        if (member is FieldInfo field)
        {
            return !ReflectionUtils.IsByRefLikeType(field.FieldType);
        }

        return true;
    }

    /// <summary>
    /// Gets the serializable members for the type.
    /// </summary>
    /// <param name="type">The type to get serializable members for.</param>
    /// <returns>The serializable members for the type.</returns>
    protected virtual List<MemberInfo> GetSerializableMembers(Type type)
    {
        var memberSerialization = JsonTypeReflector.GetObjectMemberSerialization(type);

        // Exclude index properties
        // Do not filter ByRef types here because accessing FieldType/PropertyType can trigger additional assembly loads
        var allMembers = ReflectionUtils.GetFieldsAndProperties(type, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .Where(m => m is not PropertyInfo p || !ReflectionUtils.IsIndexedProperty(p));

        var serializableMembers = new List<MemberInfo>();

        if (memberSerialization != MemberSerialization.Fields)
        {
            var dataContractAttribute = JsonTypeReflector.GetDataContractAttribute(type);

            // Exclude index properties and ByRef types
            var defaultMembers = ReflectionUtils.GetFieldsAndProperties(type,BindingFlags.Instance | BindingFlags.Public)
                .Where(FilterMembers).ToList();

            foreach (var member in allMembers)
            {
                // exclude members that are compiler generated if set
                if (SerializeCompilerGeneratedMembers || !member.IsDefined(typeof(CompilerGeneratedAttribute), true))
                {
                    if (defaultMembers.Contains(member))
                    {
                        // add all members that are found by default member search
                        serializableMembers.Add(member);
                    }
                    else
                    {
                        // add members that are explicitly marked with JsonProperty/DataMember attribute
                        // or are a field if serializing just fields
                        if (JsonTypeReflector.GetAttribute<JsonPropertyAttribute>(member) != null)
                        {
                            serializableMembers.Add(member);
                        }
                        else if (JsonTypeReflector.GetAttribute<JsonRequiredAttribute>(member) != null)
                        {
                            serializableMembers.Add(member);
                        }
                        else if (dataContractAttribute != null && JsonTypeReflector.GetAttribute<DataMemberAttribute>(member) != null)
                        {
                            serializableMembers.Add(member);
                        }
                        else if (memberSerialization == MemberSerialization.Fields && member.MemberType == MemberTypes.Field)
                        {
                            serializableMembers.Add(member);
                        }
                    }
                }
            }

            // don't include TargetSite on non-serializable exceptions
            // MemberBase is problematic to serialize. Large, self referencing instances, etc
            if (typeof(Exception).IsAssignableFrom(type))
            {
                serializableMembers = serializableMembers.Where(m => !string.Equals(m.Name, "TargetSite", StringComparison.Ordinal)).ToList();
            }
        }
        else
        {
            // serialize all fields
            foreach (var member in allMembers)
            {
                if (member is FieldInfo {IsStatic: false})
                {
                    serializableMembers.Add(member);
                }
            }
        }

        return serializableMembers;
    }

    /// <summary>
    /// Creates a <see cref="JsonObjectContract"/> for the given type.
    /// </summary>
    /// <param name="type">Type of the object.</param>
    /// <returns>A <see cref="JsonObjectContract"/> for the given type.</returns>
    protected virtual JsonObjectContract CreateObjectContract(Type type)
    {
        var contract = new JsonObjectContract(type);
        InitializeContract(contract);

        contract.MemberSerialization = JsonTypeReflector.GetObjectMemberSerialization(contract.NonNullableUnderlyingType);
        contract.Properties.AddRange(CreateProperties(contract.NonNullableUnderlyingType, contract.MemberSerialization));

        Func<string, string>? extensionDataNameResolver = null;

        var attribute = JsonTypeReflector.GetCachedAttribute<JsonObjectAttribute>(contract.NonNullableUnderlyingType);
        if (attribute != null)
        {
            contract.ItemRequired = attribute._itemRequired;
            contract.ItemNullValueHandling = attribute._itemNullValueHandling;
            contract.MissingMemberHandling = attribute._missingMemberHandling;

            if (attribute.NamingStrategyType != null)
            {
                var namingStrategy = JsonTypeReflector.GetContainerNamingStrategy(attribute)!;
                extensionDataNameResolver = s => namingStrategy.GetDictionaryKey(s);
            }
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

        // serializing DirectoryInfo without ISerializable will stackoverflow
        // https://github.com/JamesNK/Newtonsoft.Json/issues/1541
        if (Array.IndexOf(BlacklistedTypeNames, type.FullName) != -1)
        {
            contract.OnSerializingCallbacks.Add(ThrowUnableToSerializeError);
        }

        return contract;
    }

    static void ThrowUnableToSerializeError(object o, StreamingContext context)
    {
        throw new JsonSerializationException($"Unable to serialize instance of '{o.GetType()}'.");
    }

    static MemberInfo GetExtensionDataMemberForType(Type type)
    {
        var members = GetClassHierarchyForType(type).SelectMany(baseType =>
        {
            var m = new List<MemberInfo>();
            m.AddRange(baseType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            m.AddRange(baseType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            return m;
        });

        var extensionDataMember = members.LastOrDefault(m =>
        {
            var memberType = m.MemberType;
            if (memberType != MemberTypes.Property && memberType != MemberTypes.Field)
            {
                return false;
            }

            // last instance of attribute wins on type if there are multiple
            if (!m.IsDefined(typeof(JsonExtensionDataAttribute), false))
            {
                return false;
            }

            if (!ReflectionUtils.CanReadMemberValue(m, true))
            {
                throw new JsonException($"Invalid extension data attribute on '{GetClrTypeFullName(m.DeclaringType)}'. Member '{m.Name}' must have a getter.");
            }

            var t = ReflectionUtils.GetMemberUnderlyingType(m);

            if (ReflectionUtils.ImplementsGenericDefinition(t, typeof(IDictionary<,>), out var dictionaryType))
            {
                var keyType = dictionaryType.GetGenericArguments()[0];
                var valueType = dictionaryType.GetGenericArguments()[1];

                if (keyType.IsAssignableFrom(typeof(string)) && valueType.IsAssignableFrom(typeof(JToken)))
                {
                    return true;
                }
            }

            throw new JsonException($"Invalid extension data attribute on '{GetClrTypeFullName(m.DeclaringType)}'. Member '{m.Name}' type must implement IDictionary<string, JToken>.");
        });

        return extensionDataMember;
    }

    static void SetExtensionDataDelegates(JsonObjectContract contract, MemberInfo member)
    {
        var extensionDataAttribute = ReflectionUtils.GetAttribute<JsonExtensionDataAttribute>(member);
        if (extensionDataAttribute == null)
        {
            return;
        }

        var t = ReflectionUtils.GetMemberUnderlyingType(member);

        ReflectionUtils.ImplementsGenericDefinition(t, typeof(IDictionary<,>), out var dictionaryType);

        var keyType = dictionaryType!.GetGenericArguments()[0];
        var valueType = dictionaryType!.GetGenericArguments()[1];

        Type createdType;

        // change type to a class if it is the base interface so it can be instantiated if needed
        if (ReflectionUtils.IsGenericDefinition(t, typeof(IDictionary<,>)))
        {
            createdType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
        }
        else
        {
            createdType = t;
        }

        var getExtensionDataDictionary = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(member);

        if (extensionDataAttribute.ReadData)
        {
            var setExtensionDataDictionary = ReflectionUtils.CanSetMemberValue(member, true, false)
                ? JsonTypeReflector.ReflectionDelegateFactory.CreateSet<object>(member)
                : null;
            var createExtensionDataDictionary = JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(createdType);
            var setMethod = t.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance, null, valueType, new[] { keyType }, null)?.GetSetMethod();
            if (setMethod == null)
            {
                // Item is explicitly implemented and non-public
                // get from dictionary interface
                setMethod = dictionaryType!.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance, null, valueType, new[] { keyType }, null)?.GetSetMethod();
            }

            var setExtensionDataDictionaryValue = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(setMethod!);

            ExtensionDataSetter extensionDataSetter = (o, key, value) =>
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

            contract.ExtensionDataSetter = extensionDataSetter;
        }

        if (extensionDataAttribute.WriteData)
        {
            var enumerableWrapper = typeof(EnumerableDictionaryWrapper<,>).MakeGenericType(keyType, valueType);
            var constructors = enumerableWrapper.GetConstructors().First();
            var createEnumerableWrapper = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(constructors);

            ExtensionDataGetter extensionDataGetter = o =>
            {
                var dictionary = getExtensionDataDictionary(o);
                if (dictionary == null)
                {
                    return null;
                }

                return (IEnumerable<KeyValuePair<object, object>>)createEnumerableWrapper(dictionary);
            };

            contract.ExtensionDataGetter = extensionDataGetter;
        }

        contract.ExtensionDataValueType = valueType;
    }

    // leave as class instead of struct
    // will be always return as an interface and boxed
    internal class EnumerableDictionaryWrapper<TEnumeratorKey, TEnumeratorValue> : IEnumerable<KeyValuePair<object, object>>
    {
        readonly IEnumerable<KeyValuePair<TEnumeratorKey, TEnumeratorValue>> _e;

        public EnumerableDictionaryWrapper(IEnumerable<KeyValuePair<TEnumeratorKey, TEnumeratorValue>> e)
        {
            _e = e;
        }

        public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
        {
            foreach (var item in _e)
            {
                yield return new KeyValuePair<object, object>(item.Key!, item.Value!);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    static ConstructorInfo? GetAttributeConstructor(Type type)
    {
        var en = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(c => c.IsDefined(typeof(JsonConstructorAttribute), true)).GetEnumerator();

        if (en.MoveNext())
        {
            var conInfo = en.Current;
            if (en.MoveNext())
            {
                throw new JsonException("Multiple constructors with the JsonConstructorAttribute.");
            }

            return conInfo;
        }

        // little hack to get Version objects to deserialize correctly
        if (type == typeof(Version))
        {
            return type.GetConstructor(new[] { typeof(int), typeof(int), typeof(int), typeof(int) });
        }

        return null;
    }

    static ConstructorInfo? GetImmutableConstructor(Type type, JsonPropertyCollection memberProperties)
    {
        IEnumerable<ConstructorInfo> constructors = type.GetConstructors();
        var en = constructors.GetEnumerator();
        if (en.MoveNext())
        {
            var constructor = en.Current;
            if (en.MoveNext())
            {
                return null;
            }

            var parameters = constructor.GetParameters();
            if (parameters.Length > 0)
            {
                foreach (var parameterInfo in parameters)
                {
                    var memberProperty = MatchProperty(memberProperties, parameterInfo.Name, parameterInfo.ParameterType);
                    if (memberProperty == null || memberProperty.Writable)
                    {
                        return null;
                    }
                }

                return constructor;
            }
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
    /// <returns>Properties for the given <see cref="ConstructorInfo"/>.</returns>
    protected virtual IList<JsonProperty> CreateConstructorParameters(ConstructorInfo constructor, JsonPropertyCollection memberProperties)
    {
        var constructorParameters = constructor.GetParameters();

        var parameterCollection = new JsonPropertyCollection(constructor.DeclaringType);

        foreach (var parameterInfo in constructorParameters)
        {
            if (parameterInfo.Name == null)
            {
                continue;
            }

            var matchingMemberProperty = MatchProperty(memberProperties, parameterInfo.Name, parameterInfo.ParameterType);

            // ensure that property will have a name from matching property or from parameterinfo
            // parameterinfo could have no name if generated by a proxy (I'm looking at you Castle)
            if (matchingMemberProperty != null || parameterInfo.Name != null)
            {
                var property = CreatePropertyFromConstructorParameter(matchingMemberProperty, parameterInfo);

                if (property != null)
                {
                    parameterCollection.AddProperty(property);
                }
            }
        }

        return parameterCollection;
    }

    static JsonProperty? MatchProperty(JsonPropertyCollection properties, string name, Type type)
    {
        // it is possible to generate a member with a null name using Reflection.Emit
        // protect against an ArgumentNullException from GetClosestMatchProperty by testing for null here
        if (name == null)
        {
            return null;
        }

        var property = properties.GetClosestMatchProperty(name);
        // must match type as well as name
        if (property == null || property.PropertyType != type)
        {
            return null;
        }

        return property;
    }

    /// <summary>
    /// Creates a <see cref="JsonProperty"/> for the given <see cref="ParameterInfo"/>.
    /// </summary>
    /// <param name="matchingMemberProperty">The matching member property.</param>
    /// <param name="parameterInfo">The constructor parameter.</param>
    /// <returns>A created <see cref="JsonProperty"/> for the given <see cref="ParameterInfo"/>.</returns>
    protected virtual JsonProperty CreatePropertyFromConstructorParameter(JsonProperty? matchingMemberProperty, ParameterInfo parameterInfo)
    {
        var property = new JsonProperty
        {
            PropertyType = parameterInfo.ParameterType,
            AttributeProvider = new ReflectionAttributeProvider(parameterInfo)
        };

        SetPropertySettingsFromAttributes(property, parameterInfo, parameterInfo.Name, parameterInfo.Member.DeclaringType, MemberSerialization.OptOut, out _);

        property.Readable = false;
        property.Writable = true;

        // "inherit" values from matching member property if unset on parameter
        if (matchingMemberProperty != null)
        {
            property.PropertyName = property.PropertyName != parameterInfo.Name ? property.PropertyName : matchingMemberProperty.PropertyName;
            property.Converter ??= matchingMemberProperty.Converter;

            if (!property._hasExplicitDefaultValue && matchingMemberProperty._hasExplicitDefaultValue)
            {
                property.DefaultValue = matchingMemberProperty.DefaultValue;
            }

            property._required ??= matchingMemberProperty._required;
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
    /// <param name="type">Type of the object.</param>
    /// <returns>The contract's default <see cref="JsonConverter" />.</returns>
    protected virtual JsonConverter? ResolveContractConverter(Type type)
    {
        return JsonTypeReflector.GetJsonConverter(type);
    }

    static Func<object> GetDefaultCreator(Type createdType)
    {
        return JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(createdType);
    }

    void InitializeContract(JsonContract contract)
    {
        var containerAttribute = JsonTypeReflector.GetCachedAttribute<JsonContainerAttribute>(contract.NonNullableUnderlyingType);
        if (containerAttribute != null)
        {
            contract.IsReference = containerAttribute._isReference;
        }
        else
        {
            var dataContractAttribute = JsonTypeReflector.GetDataContractAttribute(contract.NonNullableUnderlyingType);
            // doesn't have a null value
            if (dataContractAttribute is {IsReference: true})
            {
                contract.IsReference = true;
            }
        }

        contract.Converter = ResolveContractConverter(contract.NonNullableUnderlyingType);

        // then see whether object is compatible with any of the built in converters
        contract.InternalConverter = JsonSerializer.GetMatchingConverter(BuiltInConverters, contract.NonNullableUnderlyingType);

        var createdType = contract.CreatedType;
        if (contract.IsInstantiable
            && (ReflectionUtils.HasDefaultConstructor(createdType, true) || createdType.IsValueType))
        {
            contract.DefaultCreator = GetDefaultCreator(createdType);

            contract.DefaultCreatorNonPublic = !createdType.IsValueType &&
                                               createdType.GetDefaultConstructor() == null;
        }

        ResolveCallbackMethods(contract, contract.NonNullableUnderlyingType);
    }

    static void ResolveCallbackMethods(JsonContract contract, Type type)
    {
        GetCallbackMethodsForType(
            type,
            out var onSerializing,
            out var onSerialized,
            out var onDeserializing,
            out var onDeserialized,
            out var onError);

        if (onSerializing != null)
        {
            contract.OnSerializingCallbacks.AddRange(onSerializing);
        }

        if (onSerialized != null)
        {
            contract.OnSerializedCallbacks.AddRange(onSerialized);
        }

        if (onDeserializing != null)
        {
            contract.OnDeserializingCallbacks.AddRange(onDeserializing);
        }

        if (onDeserialized != null)
        {
            contract.OnDeserializedCallbacks.AddRange(onDeserialized);
        }

        if (onError != null)
        {
            contract.OnErrorCallbacks.AddRange(onError);
        }
    }

    static void GetCallbackMethodsForType(Type type, out List<SerializationCallback>? onSerializing, out List<SerializationCallback>? onSerialized, out List<SerializationCallback>? onDeserializing, out List<SerializationCallback>? onDeserialized, out List<SerializationErrorCallback>? onError)
    {
        onSerializing = null;
        onSerialized = null;
        onDeserializing = null;
        onDeserialized = null;
        onError = null;

        foreach (var baseType in GetClassHierarchyForType(type))
        {
            // while we allow more than one OnSerialized total, only one can be defined per class
            MethodInfo? currentOnSerializing = null;
            MethodInfo? currentOnSerialized = null;
            MethodInfo? currentOnDeserializing = null;
            MethodInfo? currentOnDeserialized = null;
            MethodInfo? currentOnError = null;

            var skipSerializing = ShouldSkipSerializing(baseType);
            var skipDeserialized = ShouldSkipDeserialized(baseType);

            foreach (var method in baseType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                // compact framework errors when getting parameters for a generic method
                // lame, but generic methods should not be callbacks anyway
                if (method.ContainsGenericParameters)
                {
                    continue;
                }

                Type? prevAttributeType = null;
                var parameters = method.GetParameters();

                if (!skipSerializing && IsValidCallback(method, parameters, typeof(OnSerializingAttribute), currentOnSerializing, ref prevAttributeType))
                {
                    onSerializing ??= new List<SerializationCallback>();
                    onSerializing.Add(JsonContract.CreateSerializationCallback(method));
                    currentOnSerializing = method;
                }
                if (IsValidCallback(method, parameters, typeof(OnSerializedAttribute), currentOnSerialized, ref prevAttributeType))
                {
                    onSerialized ??= new List<SerializationCallback>();
                    onSerialized.Add(JsonContract.CreateSerializationCallback(method));
                    currentOnSerialized = method;
                }
                if (IsValidCallback(method, parameters, typeof(OnDeserializingAttribute), currentOnDeserializing, ref prevAttributeType))
                {
                    onDeserializing ??= new List<SerializationCallback>();
                    onDeserializing.Add(JsonContract.CreateSerializationCallback(method));
                    currentOnDeserializing = method;
                }
                if (!skipDeserialized && IsValidCallback(method, parameters, typeof(OnDeserializedAttribute), currentOnDeserialized, ref prevAttributeType))
                {
                    onDeserialized ??= new List<SerializationCallback>();
                    onDeserialized.Add(JsonContract.CreateSerializationCallback(method));
                    currentOnDeserialized = method;
                }
                if (IsValidCallback(method, parameters, typeof(OnErrorAttribute), currentOnError, ref prevAttributeType))
                {
                    onError ??= new List<SerializationErrorCallback>();
                    onError.Add(JsonContract.CreateSerializationErrorCallback(method));
                    currentOnError = method;
                }
            }
        }
    }

    static bool IsConcurrentOrObservableCollection(Type type)
    {
        if (type.IsGenericType)
        {
            var definition = type.GetGenericTypeDefinition();

            switch (definition.FullName)
            {
                case "System.Collections.Concurrent.ConcurrentQueue`1":
                case "System.Collections.Concurrent.ConcurrentStack`1":
                case "System.Collections.Concurrent.ConcurrentBag`1":
                case JsonTypeReflector.ConcurrentDictionaryTypeName:
                case "System.Collections.ObjectModel.ObservableCollection`1":
                    return true;
            }
        }

        return false;
    }

    static bool ShouldSkipDeserialized(Type type)
    {
        // ConcurrentDictionary throws an error in its OnDeserialized so ignore - http://json.codeplex.com/discussions/257093
        if (IsConcurrentOrObservableCollection(type))
        {
            return true;
        }

        return type.Name is
            FSharpUtils.FSharpSetTypeName or
            FSharpUtils.FSharpMapTypeName;
    }

    static bool ShouldSkipSerializing(Type type)
    {
        if (IsConcurrentOrObservableCollection(type))
        {
            return true;
        }

        return type.Name is
            FSharpUtils.FSharpSetTypeName or
            FSharpUtils.FSharpMapTypeName;
    }

    static List<Type> GetClassHierarchyForType(Type type)
    {
        var ret = new List<Type>();

        var current = type;
        while (current != null && current != typeof(object))
        {
            ret.Add(current);
            current = current.BaseType;
        }

        // Return the class list in order of simple => complex
        ret.Reverse();
        return ret;
    }

    /// <summary>
    /// Creates a <see cref="JsonDictionaryContract"/> for the given type.
    /// </summary>
    /// <param name="type">Type of the object.</param>
    /// <returns>A <see cref="JsonDictionaryContract"/> for the given type.</returns>
    protected virtual JsonDictionaryContract CreateDictionaryContract(Type type)
    {
        var contract = new JsonDictionaryContract(type);
        InitializeContract(contract);

        var containerAttribute = JsonTypeReflector.GetAttribute<JsonContainerAttribute>(type);
        if (containerAttribute?.NamingStrategyType != null)
        {
            var namingStrategy = JsonTypeReflector.GetContainerNamingStrategy(containerAttribute)!;
            contract.DictionaryKeyResolver = s => namingStrategy.GetDictionaryKey(s);
        }
        else
        {
            contract.DictionaryKeyResolver = ResolveDictionaryKey;
        }

        var overrideConstructor = GetAttributeConstructor(contract.NonNullableUnderlyingType);

        if (overrideConstructor != null)
        {
            var parameters = overrideConstructor.GetParameters();
            var expectedParameterType = contract.DictionaryKeyType != null && contract.DictionaryValueType != null
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
    /// Creates a <see cref="JsonArrayContract"/> for the given type.
    /// </summary>
    /// <param name="type">Type of the object.</param>
    /// <returns>A <see cref="JsonArrayContract"/> for the given type.</returns>
    protected virtual JsonArrayContract CreateArrayContract(Type type)
    {
        var contract = new JsonArrayContract(type);
        InitializeContract(contract);

        var overrideConstructor = GetAttributeConstructor(contract.NonNullableUnderlyingType);

        if (overrideConstructor != null)
        {
            var parameters = overrideConstructor.GetParameters();
            var expectedParameterType = contract.CollectionItemType != null
                ? typeof(IEnumerable<>).MakeGenericType(contract.CollectionItemType)
                : typeof(IEnumerable);

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
    /// Creates a <see cref="JsonPrimitiveContract"/> for the given type.
    /// </summary>
    /// <param name="type">Type of the object.</param>
    /// <returns>A <see cref="JsonPrimitiveContract"/> for the given type.</returns>
    protected virtual JsonPrimitiveContract CreatePrimitiveContract(Type type)
    {
        var contract = new JsonPrimitiveContract(type);
        InitializeContract(contract);

        return contract;
    }

    /// <summary>
    /// Creates a <see cref="JsonLinqContract"/> for the given type.
    /// </summary>
    /// <param name="type">Type of the object.</param>
    /// <returns>A <see cref="JsonLinqContract"/> for the given type.</returns>
    protected virtual JsonLinqContract CreateLinqContract(Type type)
    {
        var contract = new JsonLinqContract(type);
        InitializeContract(contract);

        return contract;
    }

    /// <summary>
    /// Creates a <see cref="JsonDynamicContract"/> for the given type.
    /// </summary>
    /// <param name="type">Type of the object.</param>
    /// <returns>A <see cref="JsonDynamicContract"/> for the given type.</returns>
    protected virtual JsonDynamicContract CreateDynamicContract(Type type)
    {
        var contract = new JsonDynamicContract(type);
        InitializeContract(contract);

        var containerAttribute = JsonTypeReflector.GetAttribute<JsonContainerAttribute>(type);
        if (containerAttribute?.NamingStrategyType != null)
        {
            var namingStrategy = JsonTypeReflector.GetContainerNamingStrategy(containerAttribute)!;
            contract.PropertyNameResolver = s => namingStrategy.GetDictionaryKey(s);
        }
        else
        {
            contract.PropertyNameResolver = ResolveDictionaryKey;
        }

        contract.Properties.AddRange(CreateProperties(type, MemberSerialization.OptOut));

        return contract;
    }

    /// <summary>
    /// Creates a <see cref="JsonStringContract"/> for the given type.
    /// </summary>
    /// <param name="type">Type of the object.</param>
    /// <returns>A <see cref="JsonStringContract"/> for the given type.</returns>
    protected virtual JsonStringContract CreateStringContract(Type type)
    {
        var contract = new JsonStringContract(type);
        InitializeContract(contract);

        return contract;
    }

    /// <summary>
    /// Determines which contract type is created for the given type.
    /// </summary>
    /// <param name="type">Type of the object.</param>
    /// <returns>A <see cref="JsonContract"/> for the given type.</returns>
    protected virtual JsonContract CreateContract(Type type)
    {
        var t = ReflectionUtils.EnsureNotByRefType(type);

        if (IsJsonPrimitiveType(t))
        {
            return CreatePrimitiveContract(type);
        }

        t = ReflectionUtils.EnsureNotNullableType(t);
        var containerAttribute = JsonTypeReflector.GetCachedAttribute<JsonContainerAttribute>(t);

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

        if (CollectionUtils.IsDictionaryType(t))
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

    internal static bool IsJsonPrimitiveType(Type type)
    {
        var typeCode = ConvertUtils.GetTypeCode(type);

        return typeCode != PrimitiveTypeCode.Empty && typeCode != PrimitiveTypeCode.Object;
    }

    internal static bool IsIConvertible(Type t)
    {
        if (typeof(IConvertible).IsAssignableFrom(t)
            || (ReflectionUtils.IsNullableType(t) && typeof(IConvertible).IsAssignableFrom(Nullable.GetUnderlyingType(t))))
        {
            return !typeof(JToken).IsAssignableFrom(t);
        }

        return false;
    }

    internal static bool CanConvertToString(Type type)
    {
        if (JsonTypeReflector.CanTypeDescriptorConvertString(type, out _))
        {
            return true;
        }

        return type == typeof(Type) ||
               type.IsSubclassOf(typeof(Type));
    }

    static bool IsValidCallback(MethodInfo method, ParameterInfo[] parameters, Type attributeType, MethodInfo? currentCallback, ref Type? prevAttributeType)
    {
        if (!method.IsDefined(attributeType, false))
        {
            return false;
        }

        if (currentCallback != null)
        {
            throw new JsonException($"Invalid attribute. Both '{method}' and '{currentCallback}' in type '{GetClrTypeFullName(method.DeclaringType)}' have '{attributeType}'.");
        }

        if (prevAttributeType != null)
        {
            throw new JsonException($"Invalid Callback. Method '{method}' in type '{GetClrTypeFullName(method.DeclaringType)}' has both '{prevAttributeType}' and '{attributeType}'.");
        }

        if (method.IsVirtual)
        {
            throw new JsonException($"Virtual Method '{method}' of type '{GetClrTypeFullName(method.DeclaringType)}' cannot be marked with '{attributeType}' attribute.");
        }

        if (method.ReturnType != typeof(void))
        {
            throw new JsonException($"Serialization Callback '{method}' in type '{GetClrTypeFullName(method.DeclaringType)}' must return void.");
        }

        if (attributeType == typeof(OnErrorAttribute))
        {
            if (parameters is not {Length: 2} || parameters[0].ParameterType != typeof(StreamingContext) || parameters[1].ParameterType != typeof(ErrorContext))
            {
                throw new JsonException($"Serialization Error Callback '{method}' in type '{GetClrTypeFullName(method.DeclaringType)}' must have two parameters of type '{typeof(StreamingContext)}' and '{typeof(ErrorContext)}'.");
            }
        }
        else
        {
            if (parameters is not {Length: 1} || parameters[0].ParameterType != typeof(StreamingContext))
            {
                throw new JsonException($"Serialization Callback '{method}' in type '{GetClrTypeFullName(method.DeclaringType)}' must have a single parameter of type '{typeof(StreamingContext)}'.");
            }
        }

        prevAttributeType = attributeType;

        return true;
    }

    internal static string GetClrTypeFullName(Type type)
    {
        if (type.IsGenericTypeDefinition || !type.ContainsGenericParameters)
        {
            return type.FullName;
        }

        return $"{type.Namespace}.{type.Name}";
    }

    /// <summary>
    /// Creates properties for the given <see cref="JsonContract"/>.
    /// </summary>
    /// <param name="type">The type to create properties for.</param>
    /// /// <param name="memberSerialization">The member serialization mode for the type.</param>
    /// <returns>Properties for the given <see cref="JsonContract"/>.</returns>
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

            if (property != null)
            {
                // nametable is not thread-safe for multiple writers
                lock (nameTable)
                {
                    property.PropertyName = nameTable.Add(property.PropertyName!);
                }

                properties.AddProperty(property);
            }
        }

        return properties.OrderBy(p => p.Order ?? -1).ToList();
    }

    internal virtual DefaultJsonNameTable GetNameTable()
    {
        return _nameTable;
    }

    /// <summary>
    /// Creates the <see cref="IValueProvider"/> used by the serializer to get and set values from a member.
    /// </summary>
    /// <param name="member">The member.</param>
    /// <returns>The <see cref="IValueProvider"/> used by the serializer to get and set values from a member.</returns>
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
    /// Creates a <see cref="JsonProperty"/> for the given <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="memberSerialization">The member's parent <see cref="MemberSerialization"/>.</param>
    /// <param name="member">The member to create a <see cref="JsonProperty"/> for.</param>
    /// <returns>A created <see cref="JsonProperty"/> for the given <see cref="MemberInfo"/>.</returns>
    protected virtual JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = new JsonProperty
        {
            PropertyType = ReflectionUtils.GetMemberUnderlyingType(member),
            DeclaringType = member.DeclaringType,
            ValueProvider = CreateMemberValueProvider(member),
            AttributeProvider = new ReflectionAttributeProvider(member)
        };

        SetPropertySettingsFromAttributes(property, member, member.Name, member.DeclaringType, memberSerialization, out var allowNonPublicAccess);

        if (memberSerialization != MemberSerialization.Fields)
        {
            property.Readable = ReflectionUtils.CanReadMemberValue(member, allowNonPublicAccess);
            property.Writable = ReflectionUtils.CanSetMemberValue(member, allowNonPublicAccess, property.HasMemberAttribute);
        }
        else
        {
            // write to readonly fields
            property.Readable = true;
            property.Writable = true;
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

    void SetPropertySettingsFromAttributes(JsonProperty property, object attributeProvider, string name, Type declaringType, MemberSerialization memberSerialization, out bool allowNonPublicAccess)
    {
        var dataContractAttribute = JsonTypeReflector.GetDataContractAttribute(declaringType);

        var member = attributeProvider as MemberInfo;

        DataMemberAttribute? dataMemberAttribute;
        if (dataContractAttribute != null && member != null)
        {
            dataMemberAttribute = JsonTypeReflector.GetDataMemberAttribute(member);
        }
        else
        {
            dataMemberAttribute = null;
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

        var containerAttribute = JsonTypeReflector.GetAttribute<JsonContainerAttribute>(declaringType);

        NamingStrategy? namingStrategy;
        if (propertyAttribute?.NamingStrategyType != null)
        {
            namingStrategy = JsonTypeReflector.CreateNamingStrategyInstance(propertyAttribute.NamingStrategyType, propertyAttribute.NamingStrategyParameters);
        }
        else if (containerAttribute?.NamingStrategyType != null)
        {
            namingStrategy = JsonTypeReflector.GetContainerNamingStrategy(containerAttribute);
        }
        else
        {
            namingStrategy = NamingStrategy;
        }

        if (namingStrategy != null)
        {
            property.PropertyName = namingStrategy.GetPropertyName(mappedName, hasSpecifiedName);
        }
        else
        {
            property.PropertyName = ResolvePropertyName(mappedName);
        }

        property.UnderlyingName = name;

        var hasMemberAttribute = false;
        if (propertyAttribute != null)
        {
            property._required = propertyAttribute._required;
            property.Order = propertyAttribute._order;
            property.DefaultValueHandling = propertyAttribute._defaultValueHandling;
            hasMemberAttribute = true;
            property.NullValueHandling = propertyAttribute._nullValueHandling;
            property.ReferenceLoopHandling = propertyAttribute._referenceLoopHandling;
            property.ObjectCreationHandling = propertyAttribute._objectCreationHandling;
            property.TypeNameHandling = propertyAttribute._typeNameHandling;
            property.IsReference = propertyAttribute._isReference;

            property.ItemIsReference = propertyAttribute._itemIsReference;
            property.ItemConverter = propertyAttribute.ItemConverterType != null ? JsonTypeReflector.CreateJsonConverterInstance(propertyAttribute.ItemConverterType, propertyAttribute.ItemConverterParameters) : null;
            property.ItemReferenceLoopHandling = propertyAttribute._itemReferenceLoopHandling;
            property.ItemTypeNameHandling = propertyAttribute._itemTypeNameHandling;
        }
        else
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
                property._required = dataMemberAttribute.IsRequired ? Required.AllowNull : Required.Default;
                property.Order = dataMemberAttribute.Order != -1 ? dataMemberAttribute.Order : null;
                property.DefaultValueHandling = !dataMemberAttribute.EmitDefaultValue ? DefaultValueHandling.Ignore : null;
                hasMemberAttribute = true;
            }
        }

        if (requiredAttribute != null)
        {
            property._required = Required.Always;
            hasMemberAttribute = true;
        }

        property.HasMemberAttribute = hasMemberAttribute;

        var hasJsonIgnoreAttribute =
                JsonTypeReflector.GetAttribute<JsonIgnoreAttribute>(attributeProvider) != null
                // automatically ignore extension data dictionary property if it is public
                || JsonTypeReflector.GetAttribute<JsonExtensionDataAttribute>(attributeProvider) != null;

        if (memberSerialization != MemberSerialization.OptIn)
        {
            var hasIgnoreDataMemberAttribute = JsonTypeReflector.GetAttribute<IgnoreDataMemberAttribute>(attributeProvider) != null;

            // ignored if it has JsonIgnore or NonSerialized or IgnoreDataMember attributes
            property.Ignored = hasJsonIgnoreAttribute || hasIgnoreDataMemberAttribute;
        }
        else
        {
            // ignored if it has JsonIgnore/NonSerialized or does not have DataMember or JsonProperty attributes
            property.Ignored = hasJsonIgnoreAttribute || !hasMemberAttribute;
        }

        // resolve converter for property
        // the class type might have a converter but the property converter takes precedence
        property.Converter = JsonTypeReflector.GetJsonConverter(attributeProvider);

        var defaultValueAttribute = JsonTypeReflector.GetAttribute<DefaultValueAttribute>(attributeProvider);
        if (defaultValueAttribute != null)
        {
            property.DefaultValue = defaultValueAttribute.Value;
        }

        allowNonPublicAccess = false;
        if (hasMemberAttribute)
        {
            allowNonPublicAccess = true;
        }
        if (memberSerialization == MemberSerialization.Fields)
        {
            allowNonPublicAccess = true;
        }
    }

    static Predicate<object>? CreateShouldSerializeTest(MemberInfo member)
    {
        var shouldSerializeMethod = member.DeclaringType.GetMethod(JsonTypeReflector.ShouldSerializePrefix + member.Name, Type.EmptyTypes);

        if (shouldSerializeMethod == null || shouldSerializeMethod.ReturnType != typeof(bool))
        {
            return null;
        }

        var shouldSerializeCall =
            JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(shouldSerializeMethod);

        return o => (bool)shouldSerializeCall(o)!;
    }

    static void SetIsSpecifiedActions(JsonProperty property, MemberInfo member, bool allowNonPublicAccess)
    {
        MemberInfo? specifiedMember = member.DeclaringType.GetProperty(member.Name + JsonTypeReflector.SpecifiedPostfix, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (specifiedMember == null)
        {
            specifiedMember = member.DeclaringType.GetField(member.Name + JsonTypeReflector.SpecifiedPostfix, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        if (specifiedMember == null || ReflectionUtils.GetMemberUnderlyingType(specifiedMember) != typeof(bool))
        {
            return;
        }

        Func<object, object> specifiedPropertyGet = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(specifiedMember)!;

        property.GetIsSpecified = o => (bool)specifiedPropertyGet(o);

        if (ReflectionUtils.CanSetMemberValue(specifiedMember, allowNonPublicAccess, false))
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
        if (NamingStrategy != null)
        {
            return NamingStrategy.GetPropertyName(propertyName, false);
        }

        return propertyName;
    }

    /// <summary>
    /// Resolves the name of the extension data. By default no changes are made to extension data names.
    /// </summary>
    /// <param name="extensionDataName">Name of the extension data.</param>
    /// <returns>Resolved name of the extension data.</returns>
    protected virtual string ResolveExtensionDataName(string extensionDataName)
    {
        if (NamingStrategy != null)
        {
            return NamingStrategy.GetExtensionDataName(extensionDataName);
        }

        return extensionDataName;
    }

    /// <summary>
    /// Resolves the key of the dictionary. By default <see cref="ResolvePropertyName"/> is used to resolve dictionary keys.
    /// </summary>
    /// <param name="dictionaryKey">Key of the dictionary.</param>
    /// <returns>Resolved key of the dictionary.</returns>
    protected virtual string ResolveDictionaryKey(string dictionaryKey)
    {
        if (NamingStrategy != null)
        {
            return NamingStrategy.GetDictionaryKey(dictionaryKey);
        }

        return ResolvePropertyName(dictionaryKey);
    }

    /// <summary>
    /// Gets the resolved name of the property.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Name of the property.</returns>
    public string GetResolvedPropertyName(string propertyName)
    {
        // this is a new method rather than changing the visibility of ResolvePropertyName to avoid
        // a breaking change for anyone who has overidden the method
        return ResolvePropertyName(propertyName);
    }
}