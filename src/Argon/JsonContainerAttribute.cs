// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Instructs the <see cref="JsonSerializer" /> how to serialize the object.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public abstract class JsonContainerAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the collection's items converter.
    /// </summary>
    public Type? ItemConverterType { get; set; }

    // yuck. can't set nullable properties on an attribute in C#
    // have to use this approach to get an unset default state
    internal bool? isReference;
    internal bool? itemIsReference;
    internal ReferenceLoopHandling? itemReferenceLoopHandling;
    internal TypeNameHandling? itemTypeNameHandling;

    /// <summary>
    /// Gets or sets a value that indicates whether to preserve object references.
    /// </summary>
    public bool IsReference
    {
        get => isReference ?? default;
        set => isReference = value;
    }

    /// <summary>
    /// Gets or sets a value that indicates whether to preserve collection's items references.
    /// </summary>
    public bool ItemIsReference
    {
        get => itemIsReference ?? default;
        set => itemIsReference = value;
    }

    /// <summary>
    /// Gets or sets the reference loop handling used when serializing the collection's items.
    /// </summary>
    public ReferenceLoopHandling ItemReferenceLoopHandling
    {
        get => itemReferenceLoopHandling ?? default;
        set => itemReferenceLoopHandling = value;
    }

    /// <summary>
    /// Gets or sets the type name handling used when serializing the collection's items.
    /// </summary>
    public TypeNameHandling ItemTypeNameHandling
    {
        get => itemTypeNameHandling ?? default;
        set => itemTypeNameHandling = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonContainerAttribute" /> class.
    /// </summary>
    protected JsonContainerAttribute()
    {
    }
}