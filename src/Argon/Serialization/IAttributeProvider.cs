// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Provides methods to get attributes.
/// </summary>
public interface IAttributeProvider
{
    /// <summary>
    /// Returns a collection of all of the attributes, or an empty collection if there are no attributes.
    /// </summary>
    /// <param name="inherit">When <c>true</c>, look up the hierarchy chain for the inherited custom attribute.</param>
    /// <returns>A collection of <see cref="Attribute" />s, or an empty collection.</returns>
    IList<Attribute> GetAttributes(bool inherit);

    /// <summary>
    /// Returns a collection of attributes, identified by type, or an empty collection if there are no attributes.
    /// </summary>
    /// <param name="inherit">When <c>true</c>, look up the hierarchy chain for the inherited custom attribute.</param>
    /// <returns>A collection of <see cref="Attribute" />s, or an empty collection.</returns>
    IList<Attribute> GetAttributes(Type attributeType, bool inherit);
}