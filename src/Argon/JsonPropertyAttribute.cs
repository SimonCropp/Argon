// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Instructs the <see cref="JsonSerializer" /> to always serialize the member with the specified name.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class JsonPropertyAttribute : Attribute
{
    // yuck. can't set nullable properties on an attribute in C#
    // have to use this approach to get an unset default state
    internal NullValueHandling? nullValueHandling;
    internal DefaultValueHandling? defaultValueHandling;
    internal ReferenceLoopHandling? referenceLoopHandling;
    internal ObjectCreationHandling? objectCreationHandling;
    internal TypeNameHandling? typeNameHandling;
    internal bool? isReference;
    internal int? order;
    internal Required? required;
    internal bool? itemIsReference;
    internal ReferenceLoopHandling? itemReferenceLoopHandling;
    internal TypeNameHandling? itemTypeNameHandling;

    /// <summary>
    /// Gets or sets the <see cref="JsonConverter" /> type used when serializing the property's collection items.
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
    /// [JsonProperty(ItemConverterType = typeof(MyContainerConverter), ItemConverterParameters = new object[] { 123, "Four" })]
    /// </code>
    /// </example>
    public object[]? ItemConverterParameters { get; set; }

    /// <summary>
    /// Gets or sets the null value handling used when serializing this property.
    /// </summary>
    public NullValueHandling NullValueHandling
    {
        get => nullValueHandling ?? default;
        set => nullValueHandling = value;
    }

    /// <summary>
    /// Gets or sets the default value handling used when serializing this property.
    /// </summary>
    public DefaultValueHandling DefaultValueHandling
    {
        get => defaultValueHandling ?? default;
        set => defaultValueHandling = value;
    }

    /// <summary>
    /// Gets or sets the reference loop handling used when serializing this property.
    /// </summary>
    public ReferenceLoopHandling ReferenceLoopHandling
    {
        get => referenceLoopHandling ?? default;
        set => referenceLoopHandling = value;
    }

    /// <summary>
    /// Gets or sets the object creation handling used when deserializing this property.
    /// </summary>
    public ObjectCreationHandling ObjectCreationHandling
    {
        get => objectCreationHandling ?? default;
        set => objectCreationHandling = value;
    }

    /// <summary>
    /// Gets or sets the type name handling used when serializing this property.
    /// </summary>
    public TypeNameHandling TypeNameHandling
    {
        get => typeNameHandling ?? default;
        set => typeNameHandling = value;
    }

    /// <summary>
    /// Gets or sets whether this property's value is serialized as a reference.
    /// </summary>
    public bool IsReference
    {
        get => isReference ?? default;
        set => isReference = value;
    }

    /// <summary>
    /// Gets or sets the order of serialization of a member.
    /// </summary>
    public int Order
    {
        get => order ?? default;
        set => order = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether this property is required.
    /// </summary>
    public Required Required
    {
        get => required ?? Required.Default;
        set => required = value;
    }

    /// <summary>
    /// Gets or sets the name of the property.
    /// </summary>
    public string? PropertyName { get; set; }

    /// <summary>
    /// Gets or sets the reference loop handling used when serializing the property's collection items.
    /// </summary>
    public ReferenceLoopHandling ItemReferenceLoopHandling
    {
        get => itemReferenceLoopHandling ?? default;
        set => itemReferenceLoopHandling = value;
    }

    /// <summary>
    /// Gets or sets the type name handling used when serializing the property's collection items.
    /// </summary>
    public TypeNameHandling ItemTypeNameHandling
    {
        get => itemTypeNameHandling ?? default;
        set => itemTypeNameHandling = value;
    }

    /// <summary>
    /// Gets or sets whether this property's collection items are serialized as a reference.
    /// </summary>
    public bool ItemIsReference
    {
        get => itemIsReference ?? default;
        set => itemIsReference = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonPropertyAttribute" /> class.
    /// </summary>
    public JsonPropertyAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonPropertyAttribute" /> class with the specified name.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    public JsonPropertyAttribute(string propertyName) =>
        PropertyName = propertyName;
}