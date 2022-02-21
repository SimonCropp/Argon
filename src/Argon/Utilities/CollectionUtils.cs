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

static class CollectionUtils
{
    /// <summary>
    /// Determines whether the collection is <c>null</c> or empty.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <returns>
    /// 	<c>true</c> if the collection is <c>null</c> or empty; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullOrEmpty<T>(ICollection<T> collection)
    {
        if (collection != null)
        {
            return collection.Count == 0;
        }
        return true;
    }

    /// <summary>
    /// Adds the elements of the specified collection to the specified generic <see cref="IList{T}"/>.
    /// </summary>
    /// <param name="initial">The list to add to.</param>
    /// <param name="collection">The collection of elements to add.</param>
    public static void AddRange<T>(this IList<T> initial, IEnumerable<T> collection)
    {
        if (initial == null)
        {
            throw new ArgumentNullException(nameof(initial));
        }

        if (collection == null)
        {
            return;
        }

        foreach (var value in collection)
        {
            initial.Add(value);
        }
    }

    public static bool IsDictionaryType(Type type)
    {
        return typeof(IDictionary).IsAssignableFrom(type) ||
               ReflectionUtils.ImplementsGenericDefinition(type, typeof(IDictionary<,>)) ||
               ReflectionUtils.ImplementsGenericDefinition(type, typeof(IReadOnlyDictionary<,>));
    }

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

    public static bool AddDistinct<T>(this IList<T> list, T value)
    {
        return list.AddDistinct(value, EqualityComparer<T>.Default);
    }

    public static bool AddDistinct<T>(this IList<T> list, T value, IEqualityComparer<T> comparer)
    {
        if (list.ContainsValue(value, comparer))
        {
            return false;
        }

        list.Add(value);
        return true;
    }

    // this is here because LINQ Bridge doesn't support Contains with IEqualityComparer<T>
    public static bool ContainsValue<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
    {
        comparer ??= EqualityComparer<TSource>.Default;

        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        foreach (var local in source)
        {
            if (comparer.Equals(local, value))
            {
                return true;
            }
        }

        return false;
    }

    public static bool AddRangeDistinct<T>(this IList<T> list, IEnumerable<T> values, IEqualityComparer<T> comparer)
    {
        var allAdded = true;
        foreach (var value in values)
        {
            if (!list.AddDistinct(value, comparer))
            {
                allAdded = false;
            }
        }

        return allAdded;
    }

    public static int IndexOf<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
    {
        var index = 0;
        foreach (var value in collection)
        {
            if (predicate(value))
            {
                return index;
            }

            index++;
        }

        return -1;
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
        var list = (IList)JaggedArrayGetValue(values, indices);
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
                return currentList[index];
            }

            currentList = (IList)currentList[index];
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
        CopyFromJaggedToMultidimensionalArray(values, multidimensionalArray, ArrayEmpty<int>());

        return multidimensionalArray;
    }

    public static T[] ArrayEmpty<T>()
    {
        // Enumerable.Empty<T> no longer returns an empty array in .NET Core 3.0
        return EmptyArrayContainer<T>.Empty;
    }

    static class EmptyArrayContainer<T>
    {
#pragma warning disable CA1825 // Avoid zero-length array allocations.
        public static readonly T[] Empty = new T[0];
#pragma warning restore CA1825 // Avoid zero-length array allocations.
    }
}