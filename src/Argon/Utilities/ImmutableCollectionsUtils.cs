// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

/// <summary>
/// Helper class for serializing immutable collections.
/// Note that this is used by all builds, even those that don't support immutable collections, in case the DLL is GACed
/// https://github.com/JamesNK/Newtonsoft.Json/issues/652
/// </summary>
static class ImmutableCollectionsUtils
{
    internal class ImmutableCollectionTypeInfo(string contractTypeName, string createdTypeName, string builderTypeName)
    {
        public string ContractTypeName { get; set; } = contractTypeName;
        public string CreatedTypeName { get; set; } = createdTypeName;
        public string BuilderTypeName { get; set; } = builderTypeName;
    }

    const string ImmutableListGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableList`1";
    const string ImmutableQueueGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableQueue`1";
    const string ImmutableStackGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableStack`1";
    const string ImmutableSetGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableSet`1";

    const string ImmutableArrayTypeName = "System.Collections.Immutable.ImmutableArray";
    const string ImmutableArrayGenericTypeName = "System.Collections.Immutable.ImmutableArray`1";

    const string ImmutableListTypeName = "System.Collections.Immutable.ImmutableList";
    const string ImmutableListGenericTypeName = "System.Collections.Immutable.ImmutableList`1";

    const string ImmutableQueueTypeName = "System.Collections.Immutable.ImmutableQueue";
    const string ImmutableQueueGenericTypeName = "System.Collections.Immutable.ImmutableQueue`1";

    const string ImmutableStackTypeName = "System.Collections.Immutable.ImmutableStack";
    const string ImmutableStackGenericTypeName = "System.Collections.Immutable.ImmutableStack`1";

    const string ImmutableSortedSetTypeName = "System.Collections.Immutable.ImmutableSortedSet";
    const string ImmutableSortedSetGenericTypeName = "System.Collections.Immutable.ImmutableSortedSet`1";

    const string ImmutableHashSetTypeName = "System.Collections.Immutable.ImmutableHashSet";
    const string ImmutableHashSetGenericTypeName = "System.Collections.Immutable.ImmutableHashSet`1";

    static List<ImmutableCollectionTypeInfo> ArrayContractImmutableCollectionDefinitions =
    [
        new(ImmutableListGenericInterfaceTypeName, ImmutableListGenericTypeName, ImmutableListTypeName),
        new(ImmutableListGenericTypeName, ImmutableListGenericTypeName, ImmutableListTypeName),
        new(ImmutableQueueGenericInterfaceTypeName, ImmutableQueueGenericTypeName, ImmutableQueueTypeName),
        new(ImmutableQueueGenericTypeName, ImmutableQueueGenericTypeName, ImmutableQueueTypeName),
        new(ImmutableStackGenericInterfaceTypeName, ImmutableStackGenericTypeName, ImmutableStackTypeName),
        new(ImmutableStackGenericTypeName, ImmutableStackGenericTypeName, ImmutableStackTypeName),
        new(ImmutableSetGenericInterfaceTypeName, ImmutableHashSetGenericTypeName, ImmutableHashSetTypeName),
        new(ImmutableSortedSetGenericTypeName, ImmutableSortedSetGenericTypeName, ImmutableSortedSetTypeName),
        new(ImmutableHashSetGenericTypeName, ImmutableHashSetGenericTypeName, ImmutableHashSetTypeName),
        new(ImmutableArrayGenericTypeName, ImmutableArrayGenericTypeName, ImmutableArrayTypeName)
    ];

    const string ImmutableDictionaryGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableDictionary`2";

    const string ImmutableDictionaryTypeName = "System.Collections.Immutable.ImmutableDictionary";
    const string ImmutableDictionaryGenericTypeName = "System.Collections.Immutable.ImmutableDictionary`2";

    const string ImmutableSortedDictionaryTypeName = "System.Collections.Immutable.ImmutableSortedDictionary";
    const string ImmutableSortedDictionaryGenericTypeName = "System.Collections.Immutable.ImmutableSortedDictionary`2";

    static List<ImmutableCollectionTypeInfo> dictionaryContractImmutableCollectionDefinitions =
    [
        new(ImmutableDictionaryGenericInterfaceTypeName, ImmutableDictionaryGenericTypeName, ImmutableDictionaryTypeName),
        new(ImmutableSortedDictionaryGenericTypeName, ImmutableSortedDictionaryGenericTypeName, ImmutableSortedDictionaryTypeName),
        new(ImmutableDictionaryGenericTypeName, ImmutableDictionaryGenericTypeName, ImmutableDictionaryTypeName)
    ];

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

        var definition = ArrayContractImmutableCollectionDefinitions.FirstOrDefault(d => d.ContractTypeName == name);
        if (definition == null)
        {
            return false;
        }

        var createdTypeDefinition = underlyingTypeDefinition.Assembly.GetType(definition.CreatedTypeName);
        var builderTypeDefinition = underlyingTypeDefinition.Assembly.GetType(definition.BuilderTypeName);

        if (createdTypeDefinition == null || builderTypeDefinition == null)
        {
            return false;
        }

        var mb = builderTypeDefinition.GetMethods()
            .FirstOrDefault(_ => _.Name == "CreateRange" &&
                                 _.GetParameters().Length == 1);
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

            var definition = dictionaryContractImmutableCollectionDefinitions.FirstOrDefault(d => d.ContractTypeName == name);
            if (definition != null)
            {
                var createdTypeDefinition = underlyingTypeDefinition.Assembly.GetType(definition.CreatedTypeName);
                var builderTypeDefinition = underlyingTypeDefinition.Assembly.GetType(definition.BuilderTypeName);

                if (createdTypeDefinition != null && builderTypeDefinition != null)
                {
                    var method = builderTypeDefinition
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