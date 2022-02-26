// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Allows users to control class loading and mandate what class to load.
/// </summary>
public interface ISerializationBinder
{
    /// <summary>
    /// When implemented, controls the binding of a serialized object to a type.
    /// </summary>
    /// <param name="assemblyName">Specifies the <see cref="Assembly"/> name of the serialized object.</param>
    /// <param name="typeName">Specifies the <see cref="System.Type"/> name of the serialized object</param>
    /// <returns>The type of the object the formatter creates a new instance of.</returns>
    Type BindToType(string? assemblyName, string typeName);

    /// <summary>
    /// When implemented, controls the binding of a serialized object to a type.
    /// </summary>
    /// <param name="serializedType">The type of the object the formatter creates a new instance of.</param>
    /// <param name="assemblyName">Specifies the <see cref="Assembly"/> name of the serialized object.</param>
    /// <param name="typeName">Specifies the <see cref="System.Type"/> name of the serialized object.</param>
    void BindToName(Type serializedType, out string? assemblyName, out string? typeName);
}