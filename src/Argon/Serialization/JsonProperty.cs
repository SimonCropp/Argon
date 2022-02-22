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
/// Maps a JSON property to a .NET member or constructor parameter.
/// </summary>
public class JsonProperty
{
    internal Required? _required;
    internal bool _hasExplicitDefaultValue;

    object? _defaultValue;
    bool _hasGeneratedDefaultValue;
    string? _propertyName;
    internal bool _skipPropertyNameEscape;
    Type? _propertyType;

    // use to cache contract during deserialization
    internal JsonContract? PropertyContract { get; set; }

    /// <summary>
    /// Gets or sets the name of the property.
    /// </summary>
    public string? PropertyName
    {
        get => _propertyName;
        set
        {
            _propertyName = value;
            _skipPropertyNameEscape = !JavaScriptUtils.ShouldEscapeJavaScriptString(_propertyName, JavaScriptUtils.HtmlCharEscapeFlags);
        }
    }

    /// <summary>
    /// Gets or sets the type that declared this property.
    /// </summary>
    public Type? DeclaringType { get; set; }

    /// <summary>
    /// Gets or sets the order of serialization of a member.
    /// </summary>
    public int? Order { get; set; }

    /// <summary>
    /// Gets or sets the name of the underlying member or parameter.
    /// </summary>
    public string? UnderlyingName { get; set; }

    /// <summary>
    /// Gets the <see cref="IValueProvider"/> that will get and set the <see cref="JsonProperty"/> during serialization.
    /// </summary>
    public IValueProvider? ValueProvider { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="IAttributeProvider"/> for this property.
    /// </summary>
    public IAttributeProvider? AttributeProvider { get; set; }

    /// <summary>
    /// Gets or sets the type of the property.
    /// </summary>
    public Type? PropertyType
    {
        get => _propertyType;
        set
        {
            if (_propertyType != value)
            {
                _propertyType = value;
                _hasGeneratedDefaultValue = false;
            }
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="JsonConverter" /> for the property.
    /// If set this converter takes precedence over the contract converter for the property type.
    /// </summary>
    public JsonConverter? Converter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="JsonProperty"/> is ignored.
    /// </summary>
    public bool Ignored { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="JsonProperty"/> is readable.
    /// </summary>
    public bool Readable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="JsonProperty"/> is writable.
    /// </summary>
    public bool Writable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="JsonProperty"/> has a member attribute.
    /// </summary>
    public bool HasMemberAttribute { get; set; }

    /// <summary>
    /// Gets the default value.
    /// </summary>
    public object? DefaultValue
    {
        get
        {
            if (_hasExplicitDefaultValue)
            {
                return _defaultValue;
            }

            return null;
        }
        set
        {
            _hasExplicitDefaultValue = true;
            _defaultValue = value;
        }
    }

    internal object? GetResolvedDefaultValue()
    {
        if (_propertyType == null)
        {
            return null;
        }

        if (!_hasExplicitDefaultValue && !_hasGeneratedDefaultValue)
        {
            _defaultValue = ReflectionUtils.GetDefaultValue(_propertyType);
            _hasGeneratedDefaultValue = true;
        }

        return _defaultValue;
    }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="JsonProperty"/> is required.
    /// </summary>
    public Required Required
    {
        get => _required ?? Required.Default;
        set => _required = value;
    }

    /// <summary>
    /// Gets a value indicating whether <see cref="Required"/> has a value specified.
    /// </summary>
    public bool IsRequiredSpecified => _required != null;

    /// <summary>
    /// Gets or sets a value indicating whether this property preserves object references.
    /// </summary>
    public bool? IsReference { get; set; }

    /// <summary>
    /// Gets or sets the property null value handling.
    /// </summary>
    public NullValueHandling? NullValueHandling { get; set; }

    /// <summary>
    /// Gets or sets the property default value handling.
    /// </summary>
    public DefaultValueHandling? DefaultValueHandling { get; set; }

    /// <summary>
    /// Gets or sets the property reference loop handling.
    /// </summary>
    public ReferenceLoopHandling? ReferenceLoopHandling { get; set; }

    /// <summary>
    /// Gets or sets the property object creation handling.
    /// </summary>
    public ObjectCreationHandling? ObjectCreationHandling { get; set; }

    /// <summary>
    /// Gets or sets or sets the type name handling.
    /// </summary>
    public TypeNameHandling? TypeNameHandling { get; set; }

    /// <summary>
    /// Gets or sets a predicate used to determine whether the property should be serialized.
    /// </summary>
    public Predicate<object>? ShouldSerialize { get; set; }

    /// <summary>
    /// Gets or sets a predicate used to determine whether the property should be deserialized.
    /// </summary>
    public Predicate<object>? ShouldDeserialize { get; set; }

    /// <summary>
    /// Gets or sets a predicate used to determine whether the property should be serialized.
    /// </summary>
    public Predicate<object>? GetIsSpecified { get; set; }

    /// <summary>
    /// Gets or sets an action used to set whether the property has been deserialized.
    /// </summary>
    public Action<object, object?>? SetIsSpecified { get; set; }

    /// <summary>
    /// Returns a <see cref="String"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return PropertyName ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the converter used when serializing the property's collection items.
    /// </summary>
    public JsonConverter? ItemConverter { get; set; }

    /// <summary>
    /// Gets or sets whether this property's collection items are serialized as a reference.
    /// </summary>
    public bool? ItemIsReference { get; set; }

    /// <summary>
    /// Gets or sets the type name handling used when serializing the property's collection items.
    /// </summary>
    public TypeNameHandling? ItemTypeNameHandling { get; set; }

    /// <summary>
    /// Gets or sets the reference loop handling used when serializing the property's collection items.
    /// </summary>
    public ReferenceLoopHandling? ItemReferenceLoopHandling { get; set; }

    internal void WritePropertyName(JsonWriter writer)
    {
        var propertyName = PropertyName;
        MiscellaneousUtils.Assert(propertyName != null);

        if (_skipPropertyNameEscape)
        {
            writer.WritePropertyName(propertyName, false);
        }
        else
        {
            writer.WritePropertyName(propertyName);
        }
    }
}