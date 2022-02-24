﻿#region License
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

using System.Dynamic;
using System.Linq.Expressions;

namespace Argon.Linq;

/// <summary>
/// Represents a JSON object.
/// </summary>
/// <example>
///   <code lang="cs" source="..\src\Tests\Documentation\LinqToJsonTests.cs" region="LinqToJsonCreateParse" title="Parsing a JSON Object from Text" />
/// </example>
public partial class JObject :
    JContainer,
    IDictionary<string, JToken?>
{
    readonly JPropertyKeyedCollection properties = new();

    /// <summary>
    /// Gets the container's children tokens.
    /// </summary>
    protected override IList<JToken> ChildrenTokens => properties;

    /// <summary>
    /// Initializes a new instance of the <see cref="JObject"/> class.
    /// </summary>
    public JObject()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JObject"/> class from another <see cref="JObject"/> object.
    /// </summary>
    /// <param name="other">A <see cref="JObject"/> object to copy from.</param>
    public JObject(JObject other)
        : base(other)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JObject"/> class with the specified content.
    /// </summary>
    public JObject(params object[] content)
        : this((object)content)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JObject"/> class with the specified content.
    /// </summary>
    public JObject(object content)
    {
        Add(content);
    }

    internal override bool DeepEquals(JToken node)
    {
        if (node is not JObject t)
        {
            return false;
        }

        return properties.Compare(t.properties);
    }

    internal override int IndexOfItem(JToken? item)
    {
        if (item == null)
        {
            return -1;
        }

        return properties.IndexOfReference(item);
    }

    internal override bool InsertItem(int index, JToken? item, bool skipParentCheck)
    {
        // don't add comments to JObject, no name to reference comment by
        if (item is {Type: JTokenType.Comment})
        {
            return false;
        }

        return base.InsertItem(index, item, skipParentCheck);
    }

    internal override void ValidateToken(JToken o, JToken? existing)
    {
        if (o.Type != JTokenType.Property)
        {
            throw new ArgumentException($"Can not add {o.GetType()} to {GetType()}.");
        }

        var newProperty = (JProperty)o;

        if (existing != null)
        {
            var existingProperty = (JProperty)existing;

            if (newProperty.Name == existingProperty.Name)
            {
                return;
            }
        }

        if (properties.TryGetValue(newProperty.Name, out existing))
        {
            throw new ArgumentException($"Can not add property {newProperty.Name} to {GetType()}. Property with the same name already exists on object.");
        }
    }

    internal override void MergeItem(object content, JsonMergeSettings? settings)
    {
        if (content is not JObject o)
        {
            return;
        }

        foreach (var contentItem in o)
        {
            var existingProperty = Property(contentItem.Key, settings?.PropertyNameComparison ?? StringComparison.Ordinal);

            if (existingProperty == null)
            {
                Add(contentItem.Key, contentItem.Value);
                continue;
            }

            if (contentItem.Value != null)
            {
                if (existingProperty.Value is JContainer existingContainer &&
                    existingContainer.Type == contentItem.Value.Type)
                {
                    existingContainer.Merge(contentItem.Value, settings);
                }
                else
                {
                    if (!IsNull(contentItem.Value) || settings?.MergeNullValueHandling == MergeNullValueHandling.Merge)
                    {
                        existingProperty.Value = contentItem.Value;
                    }
                }
            }
        }
    }

    static bool IsNull(JToken token)
    {
        if (token.Type == JTokenType.Null)
        {
            return true;
        }

        if (token is JValue {Value: null})
        {
            return true;
        }

        return false;
    }

    internal override JToken CloneToken()
    {
        return new JObject(this);
    }

    /// <summary>
    /// Gets the node type for this <see cref="JToken"/>.
    /// </summary>
    public override JTokenType Type => JTokenType.Object;

    /// <summary>
    /// Gets an <see cref="IEnumerable{T}"/> of <see cref="JProperty"/> of this object's properties.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="JProperty"/> of this object's properties.</returns>
    public IEnumerable<JProperty> Properties()
    {
        return properties.Cast<JProperty>();
    }

    /// <summary>
    /// Gets a <see cref="JProperty"/> with the specified name.
    /// </summary>
    /// <returns>A <see cref="JProperty"/> with the specified name or <c>null</c>.</returns>
    public JProperty? Property(string name)
    {
        return Property(name, StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the <see cref="JProperty"/> with the specified name.
    /// The exact name will be searched for first and if no matching property is found then
    /// the <see cref="StringComparison"/> will be used to match a property.
    /// </summary>
    /// <param name="comparison">One of the enumeration values that specifies how the strings will be compared.</param>
    /// <returns>A <see cref="JProperty"/> matched with the specified name or <c>null</c>.</returns>
    public JProperty? Property(string name, StringComparison comparison)
    {
        if (properties.TryGetValue(name, out var property))
        {
            return (JProperty)property;
        }

        // test above already uses this comparison so no need to repeat
        if (comparison != StringComparison.Ordinal)
        {
            for (var i = 0; i < properties.Count; i++)
            {
                var p = (JProperty)properties[i];
                if (string.Equals(p.Name, name, comparison))
                {
                    return p;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Gets a <see cref="JEnumerable{T}"/> of <see cref="JToken"/> of this object's property values.
    /// </summary>
    /// <returns>A <see cref="JEnumerable{T}"/> of <see cref="JToken"/> of this object's property values.</returns>
    public JEnumerable<JToken> PropertyValues()
    {
        return new JEnumerable<JToken>(Properties().Select(p => p.Value));
    }

    /// <summary>
    /// Gets the <see cref="JToken"/> with the specified key.
    /// </summary>
    public override JToken? this[object key]
    {
        get
        {
            if (key is not string propertyName)
            {
                throw new ArgumentException($"Accessed JObject values with invalid key value: {MiscellaneousUtils.ToString(key)}. Object property name expected.");
            }

            return this[propertyName];
        }
        set
        {
            if (key is not string propertyName)
            {
                throw new ArgumentException($"Set JObject values with invalid key value: {MiscellaneousUtils.ToString(key)}. Object property name expected.");
            }

            this[propertyName] = value;
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="JToken"/> with the specified property name.
    /// </summary>
    public JToken? this[string propertyName]
    {
        get
        {
            var property = Property(propertyName, StringComparison.Ordinal);

            return property?.Value;
        }
        set
        {
            var property = Property(propertyName, StringComparison.Ordinal);
            if (property == null)
            {
                Add(propertyName, value);
            }
            else
            {
                property.Value = value!;
            }
        }
    }

    /// <summary>
    /// Loads a <see cref="JObject"/> from a <see cref="JsonReader"/>.
    /// </summary>
    /// <param name="reader">A <see cref="JsonReader"/> that will be read for the content of the <see cref="JObject"/>.</param>
    /// <returns>A <see cref="JObject"/> that contains the JSON that was read from the specified <see cref="JsonReader"/>.</returns>
    /// <exception cref="JsonReaderException">
    ///     <paramref name="reader"/> is not valid JSON.
    /// </exception>
    public new static JObject Load(JsonReader reader)
    {
        return Load(reader, null);
    }

    /// <summary>
    /// Loads a <see cref="JObject"/> from a <see cref="JsonReader"/>.
    /// </summary>
    /// <param name="reader">A <see cref="JsonReader"/> that will be read for the content of the <see cref="JObject"/>.</param>
    /// <param name="settings">The <see cref="JsonLoadSettings"/> used to load the JSON.
    /// If this is <c>null</c>, default load settings will be used.</param>
    /// <returns>A <see cref="JObject"/> that contains the JSON that was read from the specified <see cref="JsonReader"/>.</returns>
    /// <exception cref="JsonReaderException">
    ///     <paramref name="reader"/> is not valid JSON.
    /// </exception>
    public new static JObject Load(JsonReader reader, JsonLoadSettings? settings)
    {
        if (reader.TokenType == JsonToken.None)
        {
            if (!reader.Read())
            {
                throw JsonReaderException.Create(reader, "Error reading JObject from JsonReader.");
            }
        }

        reader.MoveToContent();

        if (reader.TokenType != JsonToken.StartObject)
        {
            throw JsonReaderException.Create(reader, $"Error reading JObject from JsonReader. Current JsonReader item is not an object: {reader.TokenType}");
        }

        var o = new JObject();
        o.SetLineInfo(reader as IJsonLineInfo, settings);

        o.ReadTokenFrom(reader, settings);

        return o;
    }

    /// <summary>
    /// Load a <see cref="JObject"/> from a string that contains JSON.
    /// </summary>
    /// <param name="json">A <see cref="String"/> that contains JSON.</param>
    /// <returns>A <see cref="JObject"/> populated from the string that contains JSON.</returns>
    /// <exception cref="JsonReaderException">
    ///     <paramref name="json"/> is not valid JSON.
    /// </exception>
    /// <example>
    ///   <code lang="cs" source="..\src\Tests\Documentation\LinqToJsonTests.cs" region="LinqToJsonCreateParse" title="Parsing a JSON Object from Text" />
    /// </example>
    public new static JObject Parse(string json)
    {
        return Parse(json, null);
    }

    /// <summary>
    /// Load a <see cref="JObject"/> from a string that contains JSON.
    /// </summary>
    /// <param name="json">A <see cref="String"/> that contains JSON.</param>
    /// <param name="settings">The <see cref="JsonLoadSettings"/> used to load the JSON.
    /// If this is <c>null</c>, default load settings will be used.</param>
    /// <returns>A <see cref="JObject"/> populated from the string that contains JSON.</returns>
    /// <exception cref="JsonReaderException">
    ///     <paramref name="json"/> is not valid JSON.
    /// </exception>
    /// <example>
    ///   <code lang="cs" source="..\src\Tests\Documentation\LinqToJsonTests.cs" region="LinqToJsonCreateParse" title="Parsing a JSON Object from Text" />
    /// </example>
    public new static JObject Parse(string json, JsonLoadSettings? settings)
    {
        using JsonReader reader = new JsonTextReader(new StringReader(json));
        var o = Load(reader, settings);

        while (reader.Read())
        {
            // Any content encountered here other than a comment will throw in the reader.
        }

        return o;
    }

    /// <summary>
    /// Creates a <see cref="JObject"/> from an object.
    /// </summary>
    /// <param name="o">The object that will be used to create <see cref="JObject"/>.</param>
    /// <returns>A <see cref="JObject"/> with the values of the specified object.</returns>
    public new static JObject FromObject(object o)
    {
        return FromObject(o, JsonSerializer.CreateDefault());
    }

    /// <summary>
    /// Creates a <see cref="JObject"/> from an object.
    /// </summary>
    /// <param name="o">The object that will be used to create <see cref="JObject"/>.</param>
    /// <param name="jsonSerializer">The <see cref="JsonSerializer"/> that will be used to read the object.</param>
    /// <returns>A <see cref="JObject"/> with the values of the specified object.</returns>
    public new static JObject FromObject(object o, JsonSerializer jsonSerializer)
    {
        var token = FromObjectInternal(o, jsonSerializer);

        if (token.Type != JTokenType.Object)
        {
            throw new ArgumentException($"Object serialized to {token.Type}. JObject instance expected.");
        }

        return (JObject)token;
    }

    /// <summary>
    /// Writes this token to a <see cref="JsonWriter"/>.
    /// </summary>
    public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
    {
        writer.WriteStartObject();

        for (var i = 0; i < properties.Count; i++)
        {
            properties[i].WriteTo(writer, converters);
        }

        writer.WriteEndObject();
    }

    /// <summary>
    /// Gets the <see cref="Argon.Linq.JToken"/> with the specified property name.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>The <see cref="Argon.Linq.JToken"/> with the specified property name.</returns>
    public JToken? GetValue(string? propertyName)
    {
        return GetValue(propertyName, StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the <see cref="Argon.Linq.JToken"/> with the specified property name.
    /// The exact property name will be searched for first and if no matching property is found then
    /// the <see cref="StringComparison"/> will be used to match a property.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="comparison">One of the enumeration values that specifies how the strings will be compared.</param>
    /// <returns>The <see cref="Argon.Linq.JToken"/> with the specified property name.</returns>
    public JToken? GetValue(string? propertyName, StringComparison comparison)
    {
        if (propertyName == null)
        {
            return null;
        }

        // attempt to get value via dictionary first for performance
        var property = Property(propertyName, comparison);

        return property?.Value;
    }

    /// <summary>
    /// Tries to get the <see cref="Argon.Linq.JToken"/> with the specified property name.
    /// The exact property name will be searched for first and if no matching property is found then
    /// the <see cref="StringComparison"/> will be used to match a property.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="comparison">One of the enumeration values that specifies how the strings will be compared.</param>
    /// <returns><c>true</c> if a value was successfully retrieved; otherwise, <c>false</c>.</returns>
    public bool TryGetValue(string propertyName, StringComparison comparison, [NotNullWhen(true)]out JToken? value)
    {
        value = GetValue(propertyName, comparison);
        return value != null;
    }

    #region IDictionary<string,JToken> Members
    /// <summary>
    /// Adds the specified property name.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    public void Add(string propertyName, JToken? value)
    {
        Add(new JProperty(propertyName, value));
    }

    /// <summary>
    /// Determines whether the JSON object has the specified property name.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns><c>true</c> if the JSON object has the specified property name; otherwise, <c>false</c>.</returns>
    public bool ContainsKey(string propertyName)
    {
        return properties.Contains(propertyName);
    }

    ICollection<string> IDictionary<string, JToken?>.Keys => properties.Keys;

    /// <summary>
    /// Removes the property with the specified name.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns><c>true</c> if item was successfully removed; otherwise, <c>false</c>.</returns>
    public bool Remove(string propertyName)
    {
        var property = Property(propertyName, StringComparison.Ordinal);
        if (property == null)
        {
            return false;
        }

        property.Remove();
        return true;
    }

    /// <summary>
    /// Tries to get the <see cref="Argon.Linq.JToken"/> with the specified property name.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns><c>true</c> if a value was successfully retrieved; otherwise, <c>false</c>.</returns>
    public bool TryGetValue(string propertyName, [NotNullWhen(true)]out JToken? value)
    {
        var property = Property(propertyName, StringComparison.Ordinal);
        if (property == null)
        {
            value = null;
            return false;
        }

        value = property.Value;
        return true;
    }

    ICollection<JToken?> IDictionary<string, JToken?>.Values => throw new NotImplementedException();

    #endregion

    #region ICollection<KeyValuePair<string,JToken>> Members
    void ICollection<KeyValuePair<string, JToken?>>.Add(KeyValuePair<string, JToken?> item)
    {
        Add(new JProperty(item.Key, item.Value));
    }

    void ICollection<KeyValuePair<string, JToken?>>.Clear()
    {
        RemoveAll();
    }

    bool ICollection<KeyValuePair<string, JToken?>>.Contains(KeyValuePair<string, JToken?> item)
    {
        var property = Property(item.Key, StringComparison.Ordinal);
        if (property == null)
        {
            return false;
        }

        return property.Value == item.Value;
    }

    void ICollection<KeyValuePair<string, JToken?>>.CopyTo(KeyValuePair<string, JToken?>[] array, int arrayIndex)
    {
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array));
        }
        if (arrayIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), "arrayIndex is less than 0.");
        }
        if (arrayIndex >= array.Length && arrayIndex != 0)
        {
            throw new ArgumentException("arrayIndex is equal to or greater than the length of array.");
        }
        if (Count > array.Length - arrayIndex)
        {
            throw new ArgumentException("The number of elements in the source JObject is greater than the available space from arrayIndex to the end of the destination array.");
        }

        var index = 0;
        foreach (JProperty property in properties)
        {
            array[arrayIndex + index] = new KeyValuePair<string, JToken?>(property.Name, property.Value);
            index++;
        }
    }

    bool ICollection<KeyValuePair<string, JToken?>>.IsReadOnly => false;

    bool ICollection<KeyValuePair<string, JToken?>>.Remove(KeyValuePair<string, JToken?> item)
    {
        if (!((ICollection<KeyValuePair<string, JToken?>>)this).Contains(item))
        {
            return false;
        }

        ((IDictionary<string, JToken>)this).Remove(item.Key);
        return true;
    }
    #endregion

    internal override int GetDeepHashCode()
    {
        return ContentsHashCode();
    }

    /// <summary>
    /// Returns an enumerator that can be used to iterate through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    public IEnumerator<KeyValuePair<string, JToken?>> GetEnumerator()
    {
        foreach (JProperty property in properties)
        {
            yield return new KeyValuePair<string, JToken?>(property.Name, property.Value);
        }
    }

    /// <summary>
    /// Returns the <see cref="DynamicMetaObject"/> responsible for binding operations performed on this object.
    /// </summary>
    /// <param name="parameter">The expression tree representation of the runtime value.</param>
    /// <returns>
    /// The <see cref="DynamicMetaObject"/> to bind this object.
    /// </returns>
    protected override DynamicMetaObject GetMetaObject(Expression parameter)
    {
        return new DynamicProxyMetaObject<JObject>(parameter, this, new JObjectDynamicProxy());
    }

    class JObjectDynamicProxy : DynamicProxy<JObject>
    {
        public override bool TryGetMember(JObject instance, GetMemberBinder binder, out object? result)
        {
            // result can be null
            result = instance[binder.Name];
            return true;
        }

        public override bool TrySetMember(JObject instance, SetMemberBinder binder, object value)
        {
            // this can throw an error if value isn't a valid for a JValue
            if (value is not JToken v)
            {
                v = new JValue(value);
            }

            instance[binder.Name] = v;
            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames(JObject instance)
        {
            return instance.Properties().Select(p => p.Name);
        }
    }
}