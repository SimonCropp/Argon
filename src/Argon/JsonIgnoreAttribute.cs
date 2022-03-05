// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Instructs the <see cref="JsonSerializer" /> not to serialize the public field or public read/write property value.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class JsonIgnoreAttribute : Attribute
{
}