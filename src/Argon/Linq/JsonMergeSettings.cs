#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

namespace Argon.Linq;

/// <summary>
/// Specifies the settings used when merging JSON.
/// </summary>
public class JsonMergeSettings
{
    MergeArrayHandling mergeArrayHandling;
    MergeNullValueHandling mergeNullValueHandling;
    StringComparison propertyNameComparison;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonMergeSettings"/> class.
    /// </summary>
    public JsonMergeSettings()
    {
        propertyNameComparison = StringComparison.Ordinal;
    }

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
    /// the <see cref="StringComparison"/> will be used to match a property.
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