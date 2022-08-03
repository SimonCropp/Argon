// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Instructs the <see cref="JsonSerializer" /> how to serialize the collection.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public sealed class JsonDictionaryAttribute : JsonContainerAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonDictionaryAttribute" /> class.
    /// </summary>
    public JsonDictionaryAttribute()
    {
    }
}