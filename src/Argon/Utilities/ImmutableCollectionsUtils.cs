// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.Collections.Immutable;

/// <summary>
/// Helper class for serializing immutable collections.
/// Note that this is used by all builds, even those that don't support immutable collections, in case the DLL is GACed
/// https://github.com/JamesNK/Newtonsoft.Json/issues/652
/// </summary>
static class ImmutableCollectionsUtils
{
    class TypeInfo(Type createdType, MethodInfo createRange)
    {
        public Type CreatedType { get; } = createdType;
        public MethodInfo CreateRange { get; } = createRange;
    }

    static FrozenDictionary<Type, TypeInfo> arrayDefinitions = new Dictionary<Type, TypeInfo>
        {
            {
                typeof(IImmutableList<>), new(typeof(ImmutableList<>), GetArrayCreateRange(typeof(ImmutableList)))
            },
            {
                typeof(ImmutableList<>), new(typeof(ImmutableList<>), GetArrayCreateRange(typeof(ImmutableList)))
            },
            {
                typeof(IImmutableQueue<>), new(typeof(IImmutableQueue<>), GetArrayCreateRange(typeof(ImmutableQueue)))
            },
            {
                typeof(ImmutableQueue<>), new(typeof(ImmutableQueue<>), GetArrayCreateRange(typeof(ImmutableQueue)))
            },
            {
                typeof(IImmutableStack<>), new(typeof(ImmutableStack<>), GetArrayCreateRange(typeof(ImmutableStack)))
            },
            {
                typeof(ImmutableStack<>), new(typeof(ImmutableStack<>), GetArrayCreateRange(typeof(ImmutableStack)))
            },
            {
                typeof(IImmutableSet<>), new(typeof(ImmutableHashSet<>), GetArrayCreateRange(typeof(ImmutableHashSet)))
            },
            {
                typeof(ImmutableSortedSet<>), new(typeof(ImmutableSortedSet<>), GetArrayCreateRange(typeof(ImmutableSortedSet)))
            },
            {
                typeof(ImmutableHashSet<>), new(typeof(ImmutableHashSet<>), GetArrayCreateRange(typeof(ImmutableHashSet)))
            },
            {
                typeof(ImmutableArray<>), new(typeof(ImmutableArray<>), GetArrayCreateRange(typeof(ImmutableArray)))
            }
        }
        .ToFrozenDictionary();

    static FrozenDictionary<Type, TypeInfo> dictionaryDefinitions = new Dictionary<Type, TypeInfo>
        {
            {
                typeof(IImmutableDictionary<,>), new(typeof(ImmutableDictionary<,>), GetDictionaryCreateRange(typeof(ImmutableDictionary)))
            },
            {
                typeof(ImmutableSortedDictionary<,>), new(typeof(ImmutableSortedDictionary<,>), GetDictionaryCreateRange(typeof(ImmutableSortedDictionary)))
            },
            {
                typeof(ImmutableDictionary<,>), new(typeof(ImmutableDictionary<,>), GetDictionaryCreateRange(typeof(ImmutableDictionary)))
            }
        }
        .ToFrozenDictionary();

    internal static bool TryBuildImmutableForArrayContract(Type targetType, Type collectionItemType, [NotNullWhen(true)] out Type? createdType, [NotNullWhen(true)] out ObjectConstructor? parameterizedCreator)
    {
        if (targetType.IsGenericType &&
            arrayDefinitions.TryGetValue(targetType.GetGenericTypeDefinition(), out var definition))
        {
            createdType = definition.CreatedType.MakeGenericType(collectionItemType);
            var method = definition.CreateRange.MakeGenericMethod(collectionItemType);
            parameterizedCreator = DelegateFactory.CreateParameterizedConstructor(method);
            return true;
        }

        createdType = null;
        parameterizedCreator = null;

        return false;
    }

    static MethodInfo GetArrayCreateRange(Type type) =>
        type
            .GetMethods()
            .Single(_ => _.Name == "CreateRange" &&
                         _.GetParameters()
                             .Length == 1);

    internal static bool TryBuildImmutableForDictionaryContract(Type targetType, Type keyItemType, Type valueItemType, [NotNullWhen(true)] out Type? createdType, [NotNullWhen(true)] out ObjectConstructor? parameterizedCreator)
    {
        if (targetType.IsGenericType &&
            dictionaryDefinitions.TryGetValue(targetType.GetGenericTypeDefinition(), out var definition))
        {
            createdType = definition.CreatedType.MakeGenericType(keyItemType, valueItemType);
            var create = definition.CreateRange.MakeGenericMethod(keyItemType, valueItemType);
            parameterizedCreator = DelegateFactory.CreateParameterizedConstructor(create);
            return true;
        }

        createdType = null;
        parameterizedCreator = null;
        return false;
    }

    static MethodInfo GetDictionaryCreateRange(Type type) =>
        type
            .GetMethods()
            .Single(_ =>
            {
                var parameters = _.GetParameters();

                if (_.Name != "CreateRange" ||
                    parameters.Length != 1)
                {
                    return false;
                }

                var parameterType = parameters[0].ParameterType;
                return parameterType.IsGenericType &&
                       parameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
            });
}