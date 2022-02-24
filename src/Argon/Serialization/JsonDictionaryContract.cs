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

using System.Collections.ObjectModel;

namespace Argon;

/// <summary>
/// Contract details for a <see cref="System.Type"/> used by the <see cref="JsonSerializer"/>.
/// </summary>
public class JsonDictionaryContract : JsonContainerContract
{
    /// <summary>
    /// Gets or sets the dictionary key resolver.
    /// </summary>
    public Func<string, string>? DictionaryKeyResolver { get; set; }

    /// <summary>
    /// Gets the <see cref="System.Type"/> of the dictionary keys.
    /// </summary>
    public Type? DictionaryKeyType { get; }

    /// <summary>
    /// Gets the <see cref="System.Type"/> of the dictionary values.
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
    /// Gets or sets the function used to create the object. When set this function will override <see cref="JsonContract.DefaultCreator"/>.
    /// </summary>
    public ObjectConstructor<object>? OverrideCreator { get; set; }

    /// <summary>
    /// Gets a value indicating whether the creator has a parameter with the dictionary values.
    /// </summary>
    public bool HasParameterizedCreator { get; set; }

    internal bool HasParameterizedCreatorInternal => HasParameterizedCreator || parameterizedCreator != null || parameterizedConstructor != null;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonDictionaryContract"/> class.
    /// </summary>
    public JsonDictionaryContract(Type underlyingType)
        : base(underlyingType)
    {
        ContractType = JsonContractType.Dictionary;

        Type? keyType;
        Type? valueType;

        if (ReflectionUtils.ImplementsGenericDefinition(NonNullableUnderlyingType, typeof(IDictionary<,>), out genericCollectionDefinitionType))
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

            IsReadOnlyOrFixedSize = ReflectionUtils.InheritsGenericDefinition(NonNullableUnderlyingType, typeof(ReadOnlyDictionary<,>));

        }
        else if (ReflectionUtils.ImplementsGenericDefinition(NonNullableUnderlyingType, typeof(IReadOnlyDictionary<,>), out genericCollectionDefinitionType))
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
            ReflectionUtils.GetDictionaryKeyValueTypes(NonNullableUnderlyingType, out keyType, out valueType);

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
            genericWrapperType = typeof(DictionaryWrapper<,>).MakeGenericType(DictionaryKeyType, DictionaryValueType);

            var genericWrapperConstructor = genericWrapperType.GetConstructor(new[] { genericCollectionDefinitionType! });
            genericWrapperCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(genericWrapperConstructor);
        }

        return (IWrappedDictionary)genericWrapperCreator(dictionary);
    }

    internal IDictionary CreateTemporaryDictionary()
    {
        if (genericTemporaryDictionaryCreator == null)
        {
            var temporaryDictionaryType = typeof(Dictionary<,>).MakeGenericType(DictionaryKeyType ?? typeof(object), DictionaryValueType ?? typeof(object));

            genericTemporaryDictionaryCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(temporaryDictionaryType);
        }

        return (IDictionary)genericTemporaryDictionaryCreator();
    }
}