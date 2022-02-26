// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Represents a method that constructs an object.
/// </summary>
/// <typeparam name="T">The object type to create.</typeparam>
public delegate object ObjectConstructor<T>(params object?[] args);