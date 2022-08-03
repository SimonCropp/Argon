// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Instructs the <see cref="JsonSerializer" /> how to serialize the collection.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public sealed class JsonArrayAttribute : JsonContainerAttribute
{
    /// <summary>
    /// Gets or sets a value indicating whether null items are allowed in the collection.
    /// </summary>
    public bool AllowNullItems { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonArrayAttribute" /> class.
    /// </summary>
    public JsonArrayAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonObjectAttribute" /> class with a flag indicating whether the array can contain null items.
    /// </summary>
    /// <param name="allowNullItems">A flag indicating whether the array can contain null items.</param>
    public JsonArrayAttribute(bool allowNullItems) =>
        AllowNullItems = allowNullItems;
}