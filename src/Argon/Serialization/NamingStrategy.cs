// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// A base class for resolving how property names and dictionary keys are serialized.
/// </summary>
public abstract class NamingStrategy
{
    /// <summary>
    /// A flag indicating whether dictionary keys should be processed.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool ProcessDictionaryKeys { get; set; }

    /// <summary>
    /// A flag indicating whether extension data names should be processed.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool ProcessExtensionDataNames { get; set; }

    /// <summary>
    /// A flag indicating whether explicitly specified property names,
    /// e.g. a property name customized with a <see cref="JsonPropertyAttribute"/>, should be processed.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool OverrideSpecifiedNames { get; set; }

    /// <summary>
    /// Gets the serialized name for a given property name.
    /// </summary>
    /// <param name="name">The initial property name.</param>
    /// <param name="hasSpecifiedName">A flag indicating whether the property has had a name explicitly specified.</param>
    /// <returns>The serialized property name.</returns>
    public virtual string GetPropertyName(string name, bool hasSpecifiedName)
    {
        if (hasSpecifiedName && !OverrideSpecifiedNames)
        {
            return name;
        }

        return ResolvePropertyName(name);
    }

    /// <summary>
    /// Gets the serialized name for a given extension data name.
    /// </summary>
    /// <param name="name">The initial extension data name.</param>
    /// <returns>The serialized extension data name.</returns>
    public virtual string GetExtensionDataName(string name)
    {
        if (ProcessExtensionDataNames)
        {
            return ResolvePropertyName(name);
        }

        return name;
    }

    /// <summary>
    /// Gets the serialized key for a given dictionary key.
    /// </summary>
    /// <param name="key">The initial dictionary key.</param>
    /// <returns>The serialized dictionary key.</returns>
    public virtual string GetDictionaryKey(string key)
    {
        if (ProcessDictionaryKeys)
        {
            return ResolvePropertyName(key);
        }

        return key;
    }

    /// <summary>
    /// Resolves the specified property name.
    /// </summary>
    protected abstract string ResolvePropertyName(string name);

    /// <summary>
    /// Hash code calculation
    /// </summary>
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = GetType().GetHashCode();     // make sure different types do not result in equal values
            hashCode = (hashCode * 397) ^ ProcessDictionaryKeys.GetHashCode();
            hashCode = (hashCode * 397) ^ ProcessExtensionDataNames.GetHashCode();
            hashCode = (hashCode * 397) ^ OverrideSpecifiedNames.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    /// Object equality implementation
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as NamingStrategy);

    /// <summary>
    /// Compare to another NamingStrategy
    /// </summary>
    protected bool Equals(NamingStrategy? other)
    {
        if (other == null)
        {
            return false;
        }

        return GetType() == other.GetType() &&
               ProcessDictionaryKeys == other.ProcessDictionaryKeys &&
               ProcessExtensionDataNames == other.ProcessExtensionDataNames &&
               OverrideSpecifiedNames == other.OverrideSpecifiedNames;
    }
}