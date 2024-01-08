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
    internal class ImmutableCollectionTypeInfo(Type createdType, Type builderType)
    {
        public Type CreatedType { get; set; } = createdType;
        public Type BuilderType { get; set; } = builderType;
    }

    const string ImmutableListGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableList`1";
    const string ImmutableQueueGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableQueue`1";
    const string ImmutableStackGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableStack`1";
    const string ImmutableSetGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableSet`1";
    const string ImmutableArrayGenericTypeName = "System.Collections.Immutable.ImmutableArray`1";
    const string ImmutableListGenericTypeName = "System.Collections.Immutable.ImmutableList`1";
    const string ImmutableQueueGenericTypeName = "System.Collections.Immutable.ImmutableQueue`1";
    const string ImmutableStackGenericTypeName = "System.Collections.Immutable.ImmutableStack`1";
    const string ImmutableSortedSetGenericTypeName = "System.Collections.Immutable.ImmutableSortedSet`1";
    const string ImmutableHashSetGenericTypeName = "System.Collections.Immutable.ImmutableHashSet`1";

    static IReadOnlyDictionary<string, ImmutableCollectionTypeInfo> arrayDefinitions = new Dictionary<string, ImmutableCollectionTypeInfo>
        {
            {
                ImmutableListGenericInterfaceTypeName, new(typeof(ImmutableList<>), typeof(ImmutableList))
            },
            {
                ImmutableListGenericTypeName, new(typeof(ImmutableList<>), typeof(ImmutableList))
            },
            {
                ImmutableQueueGenericInterfaceTypeName, new(typeof(IImmutableQueue<>), typeof(ImmutableQueue))
            },
            {
                ImmutableQueueGenericTypeName, new(typeof(ImmutableQueue<>), typeof(ImmutableQueue))
            },
            {
                ImmutableStackGenericInterfaceTypeName, new(typeof(ImmutableStack<>), typeof(ImmutableStack))
            },
            {
                ImmutableStackGenericTypeName, new(typeof(ImmutableStack<>), typeof(ImmutableStack))
            },
            {
                ImmutableSetGenericInterfaceTypeName, new(typeof(ImmutableHashSet<>), typeof(ImmutableHashSet))
            },
            {
                ImmutableSortedSetGenericTypeName, new(typeof(ImmutableSortedSet<>), typeof(ImmutableSortedSet))
            },
            {
                ImmutableHashSetGenericTypeName, new(typeof(ImmutableHashSet<>), typeof(ImmutableHashSet))
            },
            {
                ImmutableArrayGenericTypeName, new(typeof(ImmutableArray<>), typeof(ImmutableArray))
            }
        }
        .ToFrozenDictionary();

    const string ImmutableDictionaryGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableDictionary`2";
    const string ImmutableDictionaryGenericTypeName = "System.Collections.Immutable.ImmutableDictionary`2";
    const string ImmutableSortedDictionaryGenericTypeName = "System.Collections.Immutable.ImmutableSortedDictionary`2";

    static IReadOnlyDictionary<string, ImmutableCollectionTypeInfo> dictionaryDefinitions = new Dictionary<string, ImmutableCollectionTypeInfo>
        {
            {
                ImmutableDictionaryGenericInterfaceTypeName, new(typeof(ImmutableDictionary<,>), typeof(ImmutableDictionary))
            },
            {
                ImmutableSortedDictionaryGenericTypeName, new(typeof(ImmutableSortedDictionary<,>), typeof(ImmutableSortedDictionary))
            },
            {
                ImmutableDictionaryGenericTypeName, new(typeof(ImmutableDictionary<,>), typeof(ImmutableDictionary))
            }
        }
        .ToFrozenDictionary();

    internal static bool TryBuildImmutableForArrayContract(Type underlyingType, Type collectionItemType, [NotNullWhen(true)] out Type? createdType, [NotNullWhen(true)] out ObjectConstructor? parameterizedCreator)
    {
        if (underlyingType.IsGenericType)
        {
            var underlyingTypeDefinition = underlyingType.GetGenericTypeDefinition();
            var name = underlyingTypeDefinition.FullName;

            if (name != null &&
                arrayDefinitions.TryGetValue(name, out var definition))
            {
                var mb = definition
                    .BuilderType
                    .GetMethods()
                    .FirstOrDefault(_ => _.Name == "CreateRange" &&
                                         _.GetParameters()
                                             .Length == 1);
                if (mb != null)
                {
                    createdType = definition.CreatedType.MakeGenericType(collectionItemType);
                    var method = mb.MakeGenericMethod(collectionItemType);
                    parameterizedCreator = DelegateFactory.CreateParameterizedConstructor(method);
                    return true;
                }
            }
        }

        createdType = null;
        parameterizedCreator = null;

        return false;
    }

    internal static bool TryBuildImmutableForDictionaryContract(Type underlyingType, Type keyItemType, Type valueItemType, [NotNullWhen(true)] out Type? createdType, [NotNullWhen(true)] out ObjectConstructor? parameterizedCreator)
    {
        if (underlyingType.IsGenericType)
        {
            var underlyingTypeDefinition = underlyingType.GetGenericTypeDefinition();
            var name = underlyingTypeDefinition.FullName;

            if (name != null &&
                dictionaryDefinitions.TryGetValue(name, out var definition))
            {
                var method = definition
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
                if (method != null)
                {
                    createdType = definition.CreatedType.MakeGenericType(keyItemType, valueItemType);
                    method = method.MakeGenericMethod(keyItemType, valueItemType);
                    parameterizedCreator = DelegateFactory.CreateParameterizedConstructor(method);
                    return true;
                }
            }
        }

        createdType = null;
        parameterizedCreator = null;
        return false;
    }
}