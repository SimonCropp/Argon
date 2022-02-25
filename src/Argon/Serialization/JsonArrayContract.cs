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
public class JsonArrayContract : JsonContainerContract
{
    /// <summary>
    /// Gets the <see cref="System.Type"/> of the collection items.
    /// </summary>
    public Type? CollectionItemType { get; }

    /// <summary>
    /// Gets a value indicating whether the collection type is a multidimensional array.
    /// </summary>
    public bool IsMultidimensionalArray { get; }

    readonly Type? genericCollectionDefinitionType;

    Type? genericWrapperType;
    ObjectConstructor<object>? genericWrapperCreator;
    Func<object>? genericTemporaryCollectionCreator;

    internal bool IsArray { get; }
    internal bool ShouldCreateWrapper { get; }
    internal bool CanDeserialize { get; private set; }

    readonly ConstructorInfo? parameterizedConstructor;

    ObjectConstructor<object>? parameterizedCreator;
    ObjectConstructor<object>? overrideCreator;

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
    public ObjectConstructor<object>? OverrideCreator
    {
        get => overrideCreator;
        set
        {
            overrideCreator = value;
            // hacky
            CanDeserialize = true;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the creator has a parameter with the collection values.
    /// </summary>
    public bool HasParameterizedCreator { get; set; }

    internal bool HasParameterizedCreatorInternal => HasParameterizedCreator || parameterizedCreator != null || parameterizedConstructor != null;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonArrayContract"/> class.
    /// </summary>
    public JsonArrayContract(Type underlyingType)
        : base(underlyingType)
    {
        ContractType = JsonContractType.Array;

        // netcoreapp3.0 uses EmptyPartition for empty enumerable. Treat as an empty array.
        IsArray = CreatedType.IsArray ||
                  (NonNullableUnderlyingType.IsGenericType && NonNullableUnderlyingType.GetGenericTypeDefinition().FullName == "System.Linq.EmptyPartition`1");

        bool canDeserialize;

        if (IsArray)
        {
            CollectionItemType = UnderlyingType.GetCollectionItemType()!;
            IsReadOnlyOrFixedSize = true;
            genericCollectionDefinitionType = typeof(List<>).MakeGenericType(CollectionItemType);

            canDeserialize = true;
            IsMultidimensionalArray = CreatedType.IsArray && UnderlyingType.GetArrayRank() > 1;
        }
        else if (typeof(IList).IsAssignableFrom(NonNullableUnderlyingType))
        {
            if (NonNullableUnderlyingType.ImplementsGenericDefinition(typeof(ICollection<>), out genericCollectionDefinitionType))
            {
                CollectionItemType = genericCollectionDefinitionType.GetGenericArguments()[0];
            }
            else
            {
                CollectionItemType = NonNullableUnderlyingType.GetCollectionItemType();
            }

            if (NonNullableUnderlyingType == typeof(IList))
            {
                CreatedType = typeof(List<object>);
            }

            if (CollectionItemType != null)
            {
                parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(NonNullableUnderlyingType, CollectionItemType);
            }

            IsReadOnlyOrFixedSize = NonNullableUnderlyingType.InheritsGenericDefinition(typeof(ReadOnlyCollection<>));
            canDeserialize = true;
        }
        else if (NonNullableUnderlyingType.ImplementsGenericDefinition(typeof(ICollection<>), out genericCollectionDefinitionType))
        {
            CollectionItemType = genericCollectionDefinitionType.GetGenericArguments()[0];

            if (ReflectionUtils.IsGenericDefinition(NonNullableUnderlyingType, typeof(ICollection<>))
                || ReflectionUtils.IsGenericDefinition(NonNullableUnderlyingType, typeof(IList<>)))
            {
                CreatedType = typeof(List<>).MakeGenericType(CollectionItemType);
            }

            if (ReflectionUtils.IsGenericDefinition(NonNullableUnderlyingType, typeof(ISet<>)))
            {
                CreatedType = typeof(HashSet<>).MakeGenericType(CollectionItemType);
            }

            parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(NonNullableUnderlyingType, CollectionItemType);
            canDeserialize = true;
            ShouldCreateWrapper = true;
        }
        else if (NonNullableUnderlyingType.ImplementsGenericDefinition(typeof(IReadOnlyCollection<>), out var tempCollectionType))
        {
            CollectionItemType = tempCollectionType.GetGenericArguments()[0];

            if (ReflectionUtils.IsGenericDefinition(NonNullableUnderlyingType, typeof(IReadOnlyCollection<>))
                || ReflectionUtils.IsGenericDefinition(NonNullableUnderlyingType, typeof(IReadOnlyList<>)))
            {
                CreatedType = typeof(ReadOnlyCollection<>).MakeGenericType(CollectionItemType);
            }

            genericCollectionDefinitionType = typeof(List<>).MakeGenericType(CollectionItemType);
            parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(CreatedType, CollectionItemType);

            StoreFSharpListCreatorIfNecessary(NonNullableUnderlyingType);

            IsReadOnlyOrFixedSize = true;
            canDeserialize = HasParameterizedCreatorInternal;
        }
        else if (NonNullableUnderlyingType.ImplementsGenericDefinition(typeof(IEnumerable<>), out tempCollectionType))
        {
            CollectionItemType = tempCollectionType.GetGenericArguments()[0];

            if (ReflectionUtils.IsGenericDefinition(UnderlyingType, typeof(IEnumerable<>)))
            {
                CreatedType = typeof(List<>).MakeGenericType(CollectionItemType);
            }

            parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(NonNullableUnderlyingType, CollectionItemType);

            StoreFSharpListCreatorIfNecessary(NonNullableUnderlyingType);

            if (NonNullableUnderlyingType.IsGenericType && NonNullableUnderlyingType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                genericCollectionDefinitionType = tempCollectionType;

                IsReadOnlyOrFixedSize = false;
                ShouldCreateWrapper = false;
                canDeserialize = true;
            }
            else
            {
                genericCollectionDefinitionType = typeof(List<>).MakeGenericType(CollectionItemType);

                IsReadOnlyOrFixedSize = true;
                ShouldCreateWrapper = true;
                canDeserialize = HasParameterizedCreatorInternal;
            }
        }
        else
        {
            // types that implement IEnumerable and nothing else
            canDeserialize = false;
            ShouldCreateWrapper = true;
        }

        CanDeserialize = canDeserialize;

        if (CollectionItemType != null &&
            ImmutableCollectionsUtils.TryBuildImmutableForArrayContract(
                NonNullableUnderlyingType,
                CollectionItemType,
                out var immutableCreatedType,
                out var immutableParameterizedCreator))
        {
            CreatedType = immutableCreatedType;
            parameterizedCreator = immutableParameterizedCreator;
            IsReadOnlyOrFixedSize = true;
            CanDeserialize = true;
        }
    }

    internal IWrappedCollection CreateWrapper(object list)
    {
        if (genericWrapperCreator == null)
        {
            MiscellaneousUtils.Assert(genericCollectionDefinitionType != null);

            genericWrapperType = typeof(CollectionWrapper<>).MakeGenericType(CollectionItemType!);

            Type constructorArgument;

            if (genericCollectionDefinitionType.InheritsGenericDefinition(typeof(List<>))
                || genericCollectionDefinitionType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                constructorArgument = typeof(ICollection<>).MakeGenericType(CollectionItemType!);
            }
            else
            {
                constructorArgument = genericCollectionDefinitionType;
            }

            var genericWrapperConstructor = genericWrapperType.GetConstructor(new[] { constructorArgument })!;
            genericWrapperCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(genericWrapperConstructor);
        }

        return (IWrappedCollection)genericWrapperCreator(list);
    }

    internal IList CreateTemporaryCollection()
    {
        if (genericTemporaryCollectionCreator == null)
        {
            // multidimensional array will also have array instances in it
            var collectionItemType = IsMultidimensionalArray || CollectionItemType == null
                ? typeof(object)
                : CollectionItemType;

            var temporaryListType = typeof(List<>).MakeGenericType(collectionItemType);
            genericTemporaryCollectionCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(temporaryListType);
        }

        return (IList)genericTemporaryCollectionCreator();
    }

    void StoreFSharpListCreatorIfNecessary(Type underlyingType)
    {
        if (!HasParameterizedCreatorInternal && underlyingType.Name == FSharpUtils.FSharpListTypeName)
        {
            FSharpUtils.EnsureInitialized(underlyingType.Assembly);
            parameterizedCreator = FSharpUtils.Instance.CreateSeq(CollectionItemType!);
        }
    }
}