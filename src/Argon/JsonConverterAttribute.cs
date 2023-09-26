// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Instructs the <see cref="JsonSerializer" /> to use the specified <see cref="JsonConverter" /> when serializing the member or class.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Parameter)]
public sealed class JsonConverterAttribute : Attribute
{
    /// <summary>
    /// Gets the <see cref="Type" /> of the <see cref="JsonConverter" />.
    /// </summary>
    public Type ConverterType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonConverterAttribute" /> class.
    /// </summary>
    /// <param name="converterType">Type of the <see cref="JsonConverter" />.</param>
    public JsonConverterAttribute(Type converterType) =>
        ConverterType = converterType;
}