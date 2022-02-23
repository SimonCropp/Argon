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

namespace Argon;

/// <summary>
/// Contract details for a <see cref="System.Type"/> used by the <see cref="JsonSerializer"/>.
/// </summary>
public class JsonContainerContract : JsonContract
{
    JsonContract? itemContract;

    // will be null for containers that don't have an item type (e.g. IList) or for complex objects
    internal JsonContract? ItemContract
    {
        get => itemContract;
        set
        {
            itemContract = value;
            if (itemContract != null)
            {
                FinalItemContract = itemContract.UnderlyingType.IsSealed ? itemContract : null;
            }
            else
            {
                FinalItemContract = null;
            }
        }
    }

    // the final (i.e. can't be inherited from like a sealed class or valuetype) item contract
    internal JsonContract? FinalItemContract { get; private set; }

    /// <summary>
    /// Gets or sets the default collection items <see cref="JsonConverter" />.
    /// </summary>
    public JsonConverter? ItemConverter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the collection items preserve object references.
    /// </summary>
    public bool? ItemIsReference { get; set; }

    /// <summary>
    /// Gets or sets the collection item reference loop handling.
    /// </summary>
    public ReferenceLoopHandling? ItemReferenceLoopHandling { get; set; }

    /// <summary>
    /// Gets or sets the collection item type name handling.
    /// </summary>
    public TypeNameHandling? ItemTypeNameHandling { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonContainerContract"/> class.
    /// </summary>
    internal JsonContainerContract(Type underlyingType)
        : base(underlyingType)
    {
        var jsonContainerAttribute = JsonTypeReflector.GetCachedAttribute<JsonContainerAttribute>(underlyingType);

        if (jsonContainerAttribute != null)
        {
            if (jsonContainerAttribute.ItemConverterType != null)
            {
                ItemConverter = JsonTypeReflector.CreateJsonConverterInstance(
                    jsonContainerAttribute.ItemConverterType,
                    jsonContainerAttribute.ItemConverterParameters);
            }

            ItemIsReference = jsonContainerAttribute.itemIsReference;
            ItemReferenceLoopHandling = jsonContainerAttribute.itemReferenceLoopHandling;
            ItemTypeNameHandling = jsonContainerAttribute.itemTypeNameHandling;
        }
    }
}