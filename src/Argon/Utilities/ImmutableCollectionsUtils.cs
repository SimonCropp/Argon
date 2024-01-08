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
    internal class ImmutableCollectionTypeInfo(string contractTypeName, string createdTypeName, Type builderType)
    {
        public string ContractTypeName { get; set; } = contractTypeName;
        public string CreatedTypeName { get; set; } = createdTypeName;
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

    static IReadOnlyDictionary<string, ImmutableCollectionTypeInfo> ArrayContractImmutableCollectionDefinitions = new Dictionary<string, ImmutableCollectionTypeInfo>
        {
            {
                ImmutableListGenericInterfaceTypeName, new(ImmutableListGenericInterfaceTypeName, ImmutableListGenericTypeName, typeof(ImmutableList))
            },
            {
                ImmutableListGenericTypeName, new(ImmutableListGenericTypeName, ImmutableListGenericTypeName, typeof(ImmutableList))
            },
            {
                ImmutableQueueGenericInterfaceTypeName, new(ImmutableQueueGenericInterfaceTypeName, ImmutableQueueGenericTypeName, typeof(ImmutableQueue))
            },
            {
                ImmutableQueueGenericTypeName, new(ImmutableQueueGenericTypeName, ImmutableQueueGenericTypeName, typeof(ImmutableQueue))
            },
            {
                ImmutableStackGenericInterfaceTypeName, new(ImmutableStackGenericInterfaceTypeName, ImmutableStackGenericTypeName, typeof(ImmutableStack))
            },
            {
                ImmutableStackGenericTypeName, new(ImmutableStackGenericTypeName, ImmutableStackGenericTypeName, typeof(ImmutableStack))
            },
            {
                ImmutableSetGenericInterfaceTypeName, new(ImmutableSetGenericInterfaceTypeName, ImmutableHashSetGenericTypeName, typeof(ImmutableHashSet))
            },
            {
                ImmutableSortedSetGenericTypeName, new(ImmutableSortedSetGenericTypeName, ImmutableSortedSetGenericTypeName, typeof(ImmutableSortedSet))
            },
            {
                ImmutableHashSetGenericTypeName, new(ImmutableHashSetGenericTypeName, ImmutableHashSetGenericTypeName, typeof(ImmutableHashSet))
            },
            {
                ImmutableArrayGenericTypeName, new(ImmutableArrayGenericTypeName, ImmutableArrayGenericTypeName, typeof(ImmutableArray))
            }
        }
        .ToFrozenDictionary();

    const string ImmutableDictionaryGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableDictionary`2";
    const string ImmutableDictionaryGenericTypeName = "System.Collections.Immutable.ImmutableDictionary`2";
    const string ImmutableSortedDictionaryGenericTypeName = "System.Collections.Immutable.ImmutableSortedDictionary`2";

    static IReadOnlyDictionary<string, ImmutableCollectionTypeInfo> dictionaryContractImmutableCollectionDefinitions = new Dictionary<string, ImmutableCollectionTypeInfo>
        {
            {
                ImmutableDictionaryGenericInterfaceTypeName, new(ImmutableDictionaryGenericInterfaceTypeName, ImmutableDictionaryGenericTypeName, typeof(ImmutableDictionary))
            },
            {
                ImmutableSortedDictionaryGenericTypeName, new(ImmutableSortedDictionaryGenericTypeName, ImmutableSortedDictionaryGenericTypeName, typeof(ImmutableSortedDictionary))
            },
            {
                ImmutableDictionaryGenericTypeName, new(ImmutableDictionaryGenericTypeName, ImmutableDictionaryGenericTypeName, typeof(ImmutableDictionary))
            }
        }
        .ToFrozenDictionary();

    internal static bool TryBuildImmutableForArrayContract(Type underlyingType, Type collectionItemType, [NotNullWhen(true)] out Type? createdType, [NotNullWhen(true)] out ObjectConstructor? parameterizedCreator)
    {
        createdType = null;
        parameterizedCreator = null;

        if (!underlyingType.IsGenericType)
        {
            return false;
        }

        var underlyingTypeDefinition = underlyingType.GetGenericTypeDefinition();
        var name = underlyingTypeDefinition.FullName;

        if (name == null ||
            !ArrayContractImmutableCollectionDefinitions.TryGetValue(name, out var definition))
        {
            return false;
        }

        var createdTypeDefinition = underlyingTypeDefinition.Assembly.GetType(definition.CreatedTypeName);

        if (createdTypeDefinition == null)
        {
            return false;
        }

        var mb = definition.BuilderType
            .GetMethods()
            .FirstOrDefault(_ => _.Name == "CreateRange" &&
                                 _.GetParameters()
                                     .Length == 1);
        if (mb == null)
        {
            return false;
        }

        createdType = createdTypeDefinition.MakeGenericType(collectionItemType);
        var method = mb.MakeGenericMethod(collectionItemType);
        parameterizedCreator = DelegateFactory.CreateParameterizedConstructor(method);
        return true;
    }

    internal static bool TryBuildImmutableForDictionaryContract(Type underlyingType, Type keyItemType, Type valueItemType, [NotNullWhen(true)] out Type? createdType, [NotNullWhen(true)] out ObjectConstructor? parameterizedCreator)
    {
        if (underlyingType.IsGenericType)
        {
            var underlyingTypeDefinition = underlyingType.GetGenericTypeDefinition();
            var name = underlyingTypeDefinition.FullName;

            if (name != null &&
                dictionaryContractImmutableCollectionDefinitions.TryGetValue(name, out var definition))
            {
                var createdTypeDefinition = underlyingTypeDefinition.Assembly.GetType(definition.CreatedTypeName);

                if (createdTypeDefinition != null)
                {
                    var method = definition.BuilderType
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
                        createdType = createdTypeDefinition.MakeGenericType(keyItemType, valueItemType);
                        method = method.MakeGenericMethod(keyItemType, valueItemType);
                        parameterizedCreator = DelegateFactory.CreateParameterizedConstructor(method);
                        return true;
                    }
                }
            }
        }

        createdType = null;
        parameterizedCreator = null;
        return false;
    }
}