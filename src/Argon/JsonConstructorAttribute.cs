// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Instructs the <see cref="JsonSerializer" /> to use the specified constructor when deserializing that object.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public sealed class JsonConstructorAttribute : Attribute
{
}