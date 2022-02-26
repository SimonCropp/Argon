// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Provides methods to get attributes from a <see cref="System.Type"/>, <see cref="MemberInfo"/>, <see cref="ParameterInfo"/> or <see cref="Assembly"/>.
/// </summary>
public class ReflectionAttributeProvider : IAttributeProvider
{
    readonly ICustomAttributeProvider attributeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReflectionAttributeProvider"/> class.
    /// </summary>
    /// <param name="attributeProvider">The instance to get attributes for. This parameter should be a <see cref="System.Type"/>, <see cref="MemberInfo"/>, <see cref="ParameterInfo"/> or <see cref="Assembly"/>.</param>
    public ReflectionAttributeProvider(ICustomAttributeProvider attributeProvider)
    {
        this.attributeProvider = attributeProvider;
    }

    /// <summary>
    /// Returns a collection of all of the attributes, or an empty collection if there are no attributes.
    /// </summary>
    /// <param name="inherit">When <c>true</c>, look up the hierarchy chain for the inherited custom attribute.</param>
    /// <returns>A collection of <see cref="Attribute"/>s, or an empty collection.</returns>
    public IList<Attribute> GetAttributes(bool inherit)
    {
        return ReflectionUtils.GetAttributes(attributeProvider, null, inherit);
    }

    /// <summary>
    /// Returns a collection of attributes, identified by type, or an empty collection if there are no attributes.
    /// </summary>
    /// <param name="inherit">When <c>true</c>, look up the hierarchy chain for the inherited custom attribute.</param>
    /// <returns>A collection of <see cref="Attribute"/>s, or an empty collection.</returns>
    public IList<Attribute> GetAttributes(Type attributeType, bool inherit)
    {
        return ReflectionUtils.GetAttributes(attributeProvider, attributeType, inherit);
    }
}