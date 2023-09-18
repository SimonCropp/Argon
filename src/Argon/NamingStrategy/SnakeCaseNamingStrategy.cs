// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// A snake case naming strategy.
/// </summary>
public class SnakeCaseNamingStrategy : NamingStrategy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SnakeCaseNamingStrategy" /> class.
    /// </summary>
    /// <param name="processDictionaryKeys">
    /// A flag indicating whether dictionary keys should be processed.
    /// </param>
    /// <param name="overrideSpecifiedNames">
    /// A flag indicating whether explicitly specified property names should be processed,
    /// e.g. a property name customized with a <see cref="JsonPropertyAttribute" />.
    /// </param>
    public SnakeCaseNamingStrategy(bool processDictionaryKeys, bool overrideSpecifiedNames)
    {
        ProcessDictionaryKeys = processDictionaryKeys;
        OverrideSpecifiedNames = overrideSpecifiedNames;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SnakeCaseNamingStrategy" /> class.
    /// </summary>
    public SnakeCaseNamingStrategy()
    {
    }

    /// <summary>
    /// Resolves the specified property name.
    /// </summary>
    protected override string ResolvePropertyName(string name) =>
        StringUtils.ToSnakeCase(name);
}