// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Instructs the <see cref="JsonSerializer"/> how to serialize the object.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public sealed class JsonObjectAttribute : JsonContainerAttribute
{
    internal MissingMemberHandling? missingMemberHandling;

    // yuck. can't set nullable properties on an attribute in C#
    // have to use this approach to get an unset default state
    internal Required? itemRequired;
    internal NullValueHandling? itemNullValueHandling;

    /// <summary>
    /// Gets or sets the member serialization.
    /// </summary>
    public MemberSerialization MemberSerialization { get; set; } = MemberSerialization.OptOut;

    /// <summary>
    /// Gets or sets the missing member handling used when deserializing this object.
    /// </summary>
    public MissingMemberHandling MissingMemberHandling
    {
        get => missingMemberHandling ?? default;
        set => missingMemberHandling = value;
    }

    /// <summary>
    /// Gets or sets how the object's properties with null values are handled during serialization and deserialization.
    /// </summary>
    public NullValueHandling ItemNullValueHandling
    {
        get => itemNullValueHandling ?? default;
        set => itemNullValueHandling = value;
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the object's properties are required.
    /// </summary>
    public Required ItemRequired
    {
        get => itemRequired ?? default;
        set => itemRequired = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonObjectAttribute"/> class.
    /// </summary>
    public JsonObjectAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonObjectAttribute"/> class with the specified member serialization.
    /// </summary>
    public JsonObjectAttribute(MemberSerialization memberSerialization)
    {
        MemberSerialization = memberSerialization;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonObjectAttribute"/> class with the specified container Id.
    /// </summary>
    public JsonObjectAttribute(string id)
        : base(id)
    {
    }
}