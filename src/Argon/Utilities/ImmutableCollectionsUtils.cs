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
    class ImmutableCollectionTypeInfo(Type createdType, Type builderType)
    {
        public Type CreatedType { get; } = createdType;
        public Type BuilderType { get; } = builderType;
    }

    static IReadOnlyDictionary<Type, ImmutableCollectionTypeInfo> arrayDefinitions = new Dictionary<Type, ImmutableCollectionTypeInfo>
        {
            {
                typeof(IImmutableList<>), new(typeof(ImmutableList<>), typeof(ImmutableList))
            },
            {
                typeof(ImmutableList<>), new(typeof(ImmutableList<>), typeof(ImmutableList))
            },
            {
                typeof(IImmutableQueue<>), new(typeof(IImmutableQueue<>), typeof(ImmutableQueue))
            },
            {
                typeof(ImmutableQueue<>), new(typeof(ImmutableQueue<>), typeof(ImmutableQueue))
            },
            {
                typeof(IImmutableStack<>), new(typeof(ImmutableStack<>), typeof(ImmutableStack))
            },
            {
                typeof(ImmutableStack<>), new(typeof(ImmutableStack<>), typeof(ImmutableStack))
            },
            {
                typeof(IImmutableSet<>), new(typeof(ImmutableHashSet<>), typeof(ImmutableHashSet))
            },
            {
                typeof(ImmutableSortedSet<>), new(typeof(ImmutableSortedSet<>), typeof(ImmutableSortedSet))
            },
            {
                typeof(ImmutableHashSet<>), new(typeof(ImmutableHashSet<>), typeof(ImmutableHashSet))
            },
            {
                typeof(ImmutableArray<>), new(typeof(ImmutableArray<>), typeof(ImmutableArray))
            }
        }
        .ToFrozenDictionary();

    static IReadOnlyDictionary<Type, ImmutableCollectionTypeInfo> dictionaryDefinitions = new Dictionary<Type, ImmutableCollectionTypeInfo>
        {
            {
                typeof(IImmutableDictionary<,>), new(typeof(ImmutableDictionary<,>), typeof(ImmutableDictionary))
            },
            {
                typeof(ImmutableSortedDictionary<,>), new(typeof(ImmutableSortedDictionary<,>), typeof(ImmutableSortedDictionary))
            },
            {
                typeof(ImmutableDictionary<,>), new(typeof(ImmutableDictionary<,>), typeof(ImmutableDictionary))
            }
        }
        .ToFrozenDictionary();

    internal static bool TryBuildImmutableForArrayContract(Type targetType, Type collectionItemType, [NotNullWhen(true)] out Type? createdType, [NotNullWhen(true)] out ObjectConstructor? parameterizedCreator)
    {
        if (targetType.IsGenericType &&
            arrayDefinitions.TryGetValue(targetType.GetGenericTypeDefinition(), out var definition))
        {
            var create = definition
                .BuilderType
                .GetMethods()
                .FirstOrDefault(_ => _.Name == "CreateRange" &&
                                     _.GetParameters()
                                         .Length == 1);
            if (create != null)
            {
                createdType = definition.CreatedType.MakeGenericType(collectionItemType);
                var method = create.MakeGenericMethod(collectionItemType);
                parameterizedCreator = DelegateFactory.CreateParameterizedConstructor(method);
                return true;
            }
        }

        createdType = null;
        parameterizedCreator = null;

        return false;
    }

    internal static bool TryBuildImmutableForDictionaryContract(Type targetType, Type keyItemType, Type valueItemType, [NotNullWhen(true)] out Type? createdType, [NotNullWhen(true)] out ObjectConstructor? parameterizedCreator)
    {
        if (targetType.IsGenericType &&
            dictionaryDefinitions.TryGetValue(targetType.GetGenericTypeDefinition(), out var definition))
        {
            var create = definition
                .BuilderType
                .GetMethods()
                .FirstOrDefault(_ =>
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
            if (create != null)
            {
                createdType = definition.CreatedType.MakeGenericType(keyItemType, valueItemType);
                create = create.MakeGenericMethod(keyItemType, valueItemType);
                parameterizedCreator = DelegateFactory.CreateParameterizedConstructor(create);
                return true;
            }
        }

        createdType = null;
        parameterizedCreator = null;
        return false;
    }
}