// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// When applied to a method, specifies that the method is called when an error occurs serializing an object.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class OnErrorAttribute : Attribute
{
}