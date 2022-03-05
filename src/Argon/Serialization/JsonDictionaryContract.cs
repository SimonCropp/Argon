// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.Collections.ObjectModel;

namespace Argon;

/// <summary>
/// Contract details for a <see cref="System.Type" /> used by the <see cref="JsonSerializer" />.
/// </summary>
public class JsonDictionaryContract : JsonContainerContract
{
    /// <summary>
    /// Gets or sets the dictionary key resolver.
    /// </summary>
    public Func<string, string>? DictionaryKeyResolver { get; set; }

    /// <summary>
    /// Gets the <see cref="System.Type" /> of the dictionary keys.
    /// </summary>
    public Type? DictionaryKeyType { get; }

    /// <summary>
    /// Gets the <see cref="System.Type" /> of the dictionary values.
    /// </summary>
    public Type? DictionaryValueType { get; }

    internal JsonContract? KeyContract { get; set; }

    readonly Type? genericCollectionDefinitionType;

    Type? genericWrapperType;
    ObjectConstructor<object>? genericWrapperCreator;

    Func<object>? genericTemporaryDictionaryCreator;

    internal bool ShouldCreateWrapper { get; }

    readonly ConstructorInfo? parameterizedConstructor;

    ObjectConstructor<object>? parameterizedCreator;

    internal ObjectConstructor<object>? ParameterizedCreator
    {
        get
        {
            if (parameterizedCreator == null && parameterizedConstructor != null)
            {
                parameterizedCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(parameterizedConstructor);
            }

            return parameterizedCreator;
        }
    }

    /// <summary>
    /// Gets or sets the function used to create the object. When set this function will override <see cref="JsonContract.DefaultCreator" />.
    /// </summary>
    public ObjectConstructor<object>? OverrideCreator { get; set; }

    /// <summary>
    /// Gets a value indicating whether the creator has a parameter with the dictionary values.
    /// </summary>
    public bool HasParameterizedCreator { get; set; }

    internal bool HasParameterizedCreatorInternal => HasParameterizedCreator || parameterizedCreator != null || parameterizedConstructor != null;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonDictionaryContract" /> class.
    /// </summary>
    public JsonDictionaryContract(Type underlyingType)
        : base(underlyingType)
    {
        ContractType = JsonContractType.Dictionary;

        Type? keyType;
        Type? valueType;

        if (NonNullableUnderlyingType.ImplementsGenericDefinition(typeof(IDictionary<,>), out genericCollectionDefinitionType))
        {
            keyType = genericCollectionDefinitionType.GetGenericArguments()[0];
            valueType = genericCollectionDefinitionType.GetGenericArguments()[1];

            if (ReflectionUtils.IsGenericDefinition(NonNullableUnderlyingType, typeof(IDictionary<,>)))
            {
                CreatedType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            }
            else if (NonNullableUnderlyingType.IsGenericType)
            {
                // ConcurrentDictionary<,> + IDictionary setter + null value = error
                // wrap to use generic setter
                // https://github.com/JamesNK/Newtonsoft.Json/issues/1582
                var typeDefinition = NonNullableUnderlyingType.GetGenericTypeDefinition();
                if (typeDefinition.FullName == JsonTypeReflector.ConcurrentDictionaryTypeName)
                {
                    ShouldCreateWrapper = true;
                }
            }

            IsReadOnlyOrFixedSize = NonNullableUnderlyingType.InheritsGenericDefinition(typeof(ReadOnlyDictionary<,>));
        }
        else if (NonNullableUnderlyingType.ImplementsGenericDefinition(typeof(IReadOnlyDictionary<,>), out genericCollectionDefinitionType))
        {
            keyType = genericCollectionDefinitionType.GetGenericArguments()[0];
            valueType = genericCollectionDefinitionType.GetGenericArguments()[1];

            if (ReflectionUtils.IsGenericDefinition(NonNullableUnderlyingType, typeof(IReadOnlyDictionary<,>)))
            {
                CreatedType = typeof(ReadOnlyDictionary<,>).MakeGenericType(keyType, valueType);
            }

            IsReadOnlyOrFixedSize = true;
        }
        else
        {
            NonNullableUnderlyingType.GetDictionaryKeyValueTypes(out keyType, out valueType);

            if (NonNullableUnderlyingType == typeof(IDictionary))
            {
                CreatedType = typeof(Dictionary<object, object>);
            }
        }

        if (keyType != null && valueType != null)
        {
            parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(
                CreatedType,
                typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType),
                typeof(IDictionary<,>).MakeGenericType(keyType, valueType));

            if (!HasParameterizedCreatorInternal && NonNullableUnderlyingType.Name == FSharpUtils.FSharpMapTypeName)
            {
                FSharpUtils.EnsureInitialized(NonNullableUnderlyingType.Assembly);
                parameterizedCreator = FSharpUtils.Instance.CreateMap(keyType, valueType);
            }
        }

        if (!typeof(IDictionary).IsAssignableFrom(CreatedType))
        {
            ShouldCreateWrapper = true;
        }

        DictionaryKeyType = keyType;
        DictionaryValueType = valueType;

        if (DictionaryKeyType != null &&
            DictionaryValueType != null &&
            ImmutableCollectionsUtils.TryBuildImmutableForDictionaryContract(
                NonNullableUnderlyingType,
                DictionaryKeyType,
                DictionaryValueType,
                out var immutableCreatedType,
                out var immutableParameterizedCreator))
        {
            CreatedType = immutableCreatedType;
            parameterizedCreator = immutableParameterizedCreator;
            IsReadOnlyOrFixedSize = true;
        }
    }

    internal IWrappedDictionary CreateWrapper(object dictionary)
    {
        if (genericWrapperCreator == null)
        {
            genericWrapperType = typeof(DictionaryWrapper<,>).MakeGenericType(DictionaryKeyType!, DictionaryValueType!);

            var genericWrapperConstructor = genericWrapperType.GetConstructor(new[] {genericCollectionDefinitionType!})!;
            genericWrapperCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(genericWrapperConstructor);
        }

        return (IWrappedDictionary) genericWrapperCreator(dictionary);
    }

    internal IDictionary CreateTemporaryDictionary()
    {
        if (genericTemporaryDictionaryCreator == null)
        {
            var temporaryDictionaryType = typeof(Dictionary<,>).MakeGenericType(DictionaryKeyType ?? typeof(object), DictionaryValueType ?? typeof(object));

            genericTemporaryDictionaryCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(temporaryDictionaryType);
        }

        return (IDictionary) genericTemporaryDictionaryCreator();
    }
}