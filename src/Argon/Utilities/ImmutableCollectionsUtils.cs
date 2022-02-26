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
    internal class ImmutableCollectionTypeInfo
    {
        public ImmutableCollectionTypeInfo(string contractTypeName, string createdTypeName, string builderTypeName)
        {
            ContractTypeName = contractTypeName;
            CreatedTypeName = createdTypeName;
            BuilderTypeName = builderTypeName;
        }

        public string ContractTypeName { get; set; }
        public string CreatedTypeName { get; set; }
        public string BuilderTypeName { get; set; }
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

    static readonly IList<ImmutableCollectionTypeInfo> ArrayContractImmutableCollectionDefinitions = new List<ImmutableCollectionTypeInfo>
    {
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
    };

    const string ImmutableDictionaryGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableDictionary`2";

    const string ImmutableDictionaryTypeName = "System.Collections.Immutable.ImmutableDictionary";
    const string ImmutableDictionaryGenericTypeName = "System.Collections.Immutable.ImmutableDictionary`2";

    const string ImmutableSortedDictionaryTypeName = "System.Collections.Immutable.ImmutableSortedDictionary";
    const string ImmutableSortedDictionaryGenericTypeName = "System.Collections.Immutable.ImmutableSortedDictionary`2";

    static readonly IList<ImmutableCollectionTypeInfo> DictionaryContractImmutableCollectionDefinitions = new List<ImmutableCollectionTypeInfo>
    {
        new(ImmutableDictionaryGenericInterfaceTypeName, ImmutableDictionaryGenericTypeName, ImmutableDictionaryTypeName),
        new(ImmutableSortedDictionaryGenericTypeName, ImmutableSortedDictionaryGenericTypeName, ImmutableSortedDictionaryTypeName),
        new(ImmutableDictionaryGenericTypeName, ImmutableDictionaryGenericTypeName, ImmutableDictionaryTypeName)
    };

    internal static bool TryBuildImmutableForArrayContract(Type underlyingType, Type collectionItemType, [NotNullWhen(true)]out Type? createdType, [NotNullWhen(true)]out ObjectConstructor<object>? parameterizedCreator)
    {
        if (underlyingType.IsGenericType)
        {
            var underlyingTypeDefinition = underlyingType.GetGenericTypeDefinition();
            var name = underlyingTypeDefinition.FullName;

            var definition = ArrayContractImmutableCollectionDefinitions.FirstOrDefault(d => d.ContractTypeName == name);
            if (definition != null)
            {
                var createdTypeDefinition = underlyingTypeDefinition.Assembly.GetType(definition.CreatedTypeName);
                var builderTypeDefinition = underlyingTypeDefinition.Assembly.GetType(definition.BuilderTypeName);

                if (createdTypeDefinition != null && builderTypeDefinition != null)
                {
                    var mb = builderTypeDefinition.GetMethods().FirstOrDefault(m => m.Name == "CreateRange" && m.GetParameters().Length == 1);
                    if (mb != null)
                    {
                        createdType = createdTypeDefinition.MakeGenericType(collectionItemType);
                        var method = mb.MakeGenericMethod(collectionItemType);
                        parameterizedCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(method);
                        return true;
                    }
                }
            }
        }

        createdType = null;
        parameterizedCreator = null;
        return false;
    }

    internal static bool TryBuildImmutableForDictionaryContract(Type underlyingType, Type keyItemType, Type valueItemType, [NotNullWhen(true)]out Type? createdType, [NotNullWhen(true)]out ObjectConstructor<object>? parameterizedCreator)
    {
        if (underlyingType.IsGenericType)
        {
            var underlyingTypeDefinition = underlyingType.GetGenericTypeDefinition();
            var name = underlyingTypeDefinition.FullName;

            var definition = DictionaryContractImmutableCollectionDefinitions.FirstOrDefault(d => d.ContractTypeName == name);
            if (definition != null)
            {
                var createdTypeDefinition = underlyingTypeDefinition.Assembly.GetType(definition.CreatedTypeName);
                var builderTypeDefinition = underlyingTypeDefinition.Assembly.GetType(definition.BuilderTypeName);

                if (createdTypeDefinition != null && builderTypeDefinition != null)
                {
                    var mb = builderTypeDefinition.GetMethods().FirstOrDefault(m =>
                    {
                        var parameters = m.GetParameters();

                        return m.Name == "CreateRange" &&
                               parameters.Length == 1 &&
                               parameters[0].ParameterType.IsGenericType &&
                               parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
                    });
                    if (mb != null)
                    {
                        createdType = createdTypeDefinition.MakeGenericType(keyItemType, valueItemType);
                        var method = mb.MakeGenericMethod(keyItemType, valueItemType);
                        parameterizedCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(method);
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