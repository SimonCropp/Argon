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
    /// Gets or sets the id.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the collection's items converter.
    /// </summary>
    public Type? ItemConverterType { get; set; }

    /// <summary>
    /// The parameter list to use when constructing the <see cref="JsonConverter" /> described by <see cref="ItemConverterType" />.
    /// If <c>null</c>, the default constructor is used.
    /// When non-<c>null</c>, there must be a constructor defined in the <see cref="JsonConverter" /> that exactly matches the number,
    /// order, and type of these parameters.
    /// </summary>
    /// <example>
    /// <code>
    /// [JsonContainer(ItemConverterType = typeof(MyContainerConverter), ItemConverterParameters = new object[] { 123, "Four" })]
    /// </code>
    /// </example>
    public object[]? ItemConverterParameters { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Type" /> of the <see cref="NamingStrategy" />.
    /// </summary>
    public Type? NamingStrategyType
    {
        get => namingStrategyType;
        set
        {
            namingStrategyType = value;
            NamingStrategyInstance = null;
        }
    }

    /// <summary>
    /// The parameter list to use when constructing the <see cref="NamingStrategy" /> described by <see cref="NamingStrategyType" />.
    /// If <c>null</c>, the default constructor is used.
    /// When non-<c>null</c>, there must be a constructor defined in the <see cref="NamingStrategy" /> that exactly matches the number,
    /// order, and type of these parameters.
    /// </summary>
    /// <example>
    /// <code>
    /// [JsonContainer(NamingStrategyType = typeof(MyNamingStrategy), NamingStrategyParameters = new object[] { 123, "Four" })]
    /// </code>
    /// </example>
    public object[]? NamingStrategyParameters
    {
        get => namingStrategyParameters;
        set
        {
            namingStrategyParameters = value;
            NamingStrategyInstance = null;
        }
    }

    internal NamingStrategy? NamingStrategyInstance { get; set; }

    // yuck. can't set nullable properties on an attribute in C#
    // have to use this approach to get an unset default state
    internal bool? isReference;
    internal bool? itemIsReference;
    internal ReferenceLoopHandling? itemReferenceLoopHandling;
    internal TypeNameHandling? itemTypeNameHandling;
    Type? namingStrategyType;
    object[]? namingStrategyParameters;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonContainerAttribute" /> class with the specified container Id.
    /// </summary>
    protected JsonContainerAttribute(string id) =>
        Id = id;
}