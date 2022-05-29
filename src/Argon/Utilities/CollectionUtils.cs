// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

static class CollectionUtils
{
    /// <summary>
    /// Adds the elements of the specified collection to the specified generic <see cref="IList{T}" />.
    /// </summary>
    /// <param name="initial">The list to add to.</param>
    /// <param name="collection">The collection of elements to add.</param>
    public static void AddRange<T>(this IList<T> initial, IEnumerable<T> collection)
    {
        foreach (var value in collection)
        {
            initial.Add(value);
        }
    }

    public static bool IsDictionaryType(Type type) =>
        typeof(IDictionary).IsAssignableFrom(type) ||
        type.ImplementsGenericDefinition(typeof(IDictionary<,>)) ||
        type.ImplementsGenericDefinition(typeof(IReadOnlyDictionary<,>));

    public static ConstructorInfo? ResolveEnumerableCollectionConstructor(Type collectionType, Type collectionItemType)
    {
        var genericConstructorArgument = typeof(IList<>).MakeGenericType(collectionItemType);

        return ResolveEnumerableCollectionConstructor(collectionType, collectionItemType, genericConstructorArgument);
    }

    public static ConstructorInfo? ResolveEnumerableCollectionConstructor(Type collectionType, Type collectionItemType, Type constructorArgumentType)
    {
        var genericEnumerable = typeof(IEnumerable<>).MakeGenericType(collectionItemType);
        ConstructorInfo? match = null;

        foreach (var constructor in collectionType.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
        {
            var parameters = constructor.GetParameters();

            if (parameters.Length == 1)
            {
                var parameterType = parameters[0].ParameterType;

                if (genericEnumerable == parameterType)
                {
                    // exact match
                    match = constructor;
                    break;
                }

                // in case we can't find an exact match, use first inexact
                if (match == null)
                {
                    if (parameterType.IsAssignableFrom(constructorArgumentType))
                    {
                        match = constructor;
                    }
                }
            }
        }

        return match;
    }

    public static bool Contains<T>(this List<T> list, T value, IEqualityComparer comparer)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (comparer.Equals(value, list[i]))
            {
                return true;
            }
        }

        return false;
    }

    public static int IndexOfReference<T>(this List<T> list, T item)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (ReferenceEquals(item, list[i]))
            {
                return i;
            }
        }

        return -1;
    }

    // faster reverse in .NET Framework with value types - https://github.com/JamesNK/Newtonsoft.Json/issues/1430
    public static void FastReverse<T>(this List<T> list)
    {
        var i = 0;
        var j = list.Count - 1;
        while (i < j)
        {
            (list[i], list[j]) = (list[j], list[i]);
            i++;
            j--;
        }
    }

    static IList<int> GetDimensions(IList values, int dimensionsCount)
    {
        var dimensions = new List<int>();

        var currentArray = values;
        while (true)
        {
            dimensions.Add(currentArray.Count);

            // don't keep calculating dimensions for arrays inside the value array
            if (dimensions.Count == dimensionsCount)
            {
                break;
            }

            if (currentArray.Count == 0)
            {
                break;
            }

            var v = currentArray[0];
            if (v is IList list)
            {
                currentArray = list;
            }
            else
            {
                break;
            }
        }

        return dimensions;
    }

    static void CopyFromJaggedToMultidimensionalArray(IList values, Array multidimensionalArray, int[] indices)
    {
        var dimension = indices.Length;
        if (dimension == multidimensionalArray.Rank)
        {
            multidimensionalArray.SetValue(JaggedArrayGetValue(values, indices), indices);
            return;
        }

        var dimensionLength = multidimensionalArray.GetLength(dimension);
        var list = (IList) JaggedArrayGetValue(values, indices);
        var currentValuesLength = list.Count;
        if (currentValuesLength != dimensionLength)
        {
            throw new("Cannot deserialize non-cubical array as multidimensional array.");
        }

        var newIndices = new int[dimension + 1];
        for (var i = 0; i < dimension; i++)
        {
            newIndices[i] = indices[i];
        }

        for (var i = 0; i < multidimensionalArray.GetLength(dimension); i++)
        {
            newIndices[dimension] = i;
            CopyFromJaggedToMultidimensionalArray(values, multidimensionalArray, newIndices);
        }
    }

    static object JaggedArrayGetValue(IList values, int[] indices)
    {
        var currentList = values;
        for (var i = 0; i < indices.Length; i++)
        {
            var index = indices[i];
            if (i == indices.Length - 1)
            {
                return currentList[index]!;
            }

            currentList = (IList) currentList[index]!;
        }

        return currentList;
    }

    public static Array ToMultidimensionalArray(IList values, Type type, int rank)
    {
        var dimensions = GetDimensions(values, rank);

        while (dimensions.Count < rank)
        {
            dimensions.Add(0);
        }

        var multidimensionalArray = Array.CreateInstance(type, dimensions.ToArray());
        CopyFromJaggedToMultidimensionalArray(values, multidimensionalArray, Array.Empty<int>());

        return multidimensionalArray;
    }
}