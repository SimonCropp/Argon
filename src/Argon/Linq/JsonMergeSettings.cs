// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Specifies the settings used when merging JSON.
/// </summary>
public class JsonMergeSettings
{
    MergeArrayHandling mergeArrayHandling;
    MergeNullValueHandling mergeNullValueHandling;
    StringComparison propertyNameComparison;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonMergeSettings" /> class.
    /// </summary>
    public JsonMergeSettings() =>
        propertyNameComparison = StringComparison.Ordinal;

    /// <summary>
    /// Gets or sets the method used when merging JSON arrays.
    /// </summary>
    public MergeArrayHandling MergeArrayHandling
    {
        get => mergeArrayHandling;
        set
        {
            if (value is < MergeArrayHandling.Concat or > MergeArrayHandling.Merge)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            mergeArrayHandling = value;
        }
    }

    /// <summary>
    /// Gets or sets how null value properties are merged.
    /// </summary>
    public MergeNullValueHandling MergeNullValueHandling
    {
        get => mergeNullValueHandling;
        set
        {
            if (value is < MergeNullValueHandling.Ignore or > MergeNullValueHandling.Merge)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            mergeNullValueHandling = value;
        }
    }

    /// <summary>
    /// Gets or sets the comparison used to match property names while merging.
    /// The exact property name will be searched for first and if no matching property is found then
    /// the <see cref="StringComparison" /> will be used to match a property.
    /// </summary>
    public StringComparison PropertyNameComparison
    {
        get => propertyNameComparison;
        set
        {
            if (value is < StringComparison.CurrentCulture or > StringComparison.OrdinalIgnoreCase)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            propertyNameComparison = value;
        }
    }
}