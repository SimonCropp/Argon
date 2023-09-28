// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Contract details for a <see cref="System.Type" /> used by the <see cref="JsonSerializer" />.
/// </summary>
public class JsonContainerContract : JsonContract
{
    JsonContract? itemContract;

    // will be null for containers that don't have an item type (e.g. IList) or for complex objects
    internal JsonContract? ItemContract
    {
        get => itemContract;
        set
        {
            itemContract = value;
            if (itemContract != null)
            {
                FinalItemContract = itemContract.UnderlyingType.IsSealed ? itemContract : null;
            }
            else
            {
                FinalItemContract = null;
            }
        }
    }

    // the final (i.e. can't be inherited from like a sealed class or valuetype) item contract
    internal JsonContract? FinalItemContract { get; private set; }

    /// <summary>
    /// Gets or sets the default collection items <see cref="JsonConverter" />.
    /// </summary>
    public JsonConverter? ItemConverter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the collection items preserve object references.
    /// </summary>
    public bool? ItemIsReference { get; set; }

    /// <summary>
    /// Gets or sets the collection item reference loop handling.
    /// </summary>
    public ReferenceLoopHandling? ItemReferenceLoopHandling { get; set; }

    /// <summary>
    /// Gets or sets the collection item type name handling.
    /// </summary>
    public TypeNameHandling? ItemTypeNameHandling { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonContainerContract" /> class.
    /// </summary>
    internal JsonContainerContract(Type underlyingType)
        : base(underlyingType)
    {
        var info = TypeAttributeCache.Get(underlyingType);
        var containerAttribute = info.JsonContainer;

        if (containerAttribute != null)
        {
            if (containerAttribute.ItemConverterType != null)
            {
                ItemConverter = JsonTypeReflector.CreateJsonConverterInstance(containerAttribute.ItemConverterType);
            }

            ItemIsReference = containerAttribute.itemIsReference;
            ItemReferenceLoopHandling = containerAttribute.itemReferenceLoopHandling;
            ItemTypeNameHandling = containerAttribute.itemTypeNameHandling;
        }
    }
}