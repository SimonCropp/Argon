// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Represents a JSON constructor.
/// </summary>
public partial class JConstructor : JContainer
{
    readonly List<JToken> values = new();

    /// <summary>
    /// Gets the container's children tokens.
    /// </summary>
    protected override IList<JToken> ChildrenTokens => values;

    internal override int IndexOfItem(JToken? item)
    {
        if (item == null)
        {
            return -1;
        }

        return values.IndexOfReference(item);
    }

    internal override void MergeItem(object content, JsonMergeSettings? settings)
    {
        if (content is not JConstructor constructor)
        {
            return;
        }

        if (constructor.Name != null)
        {
            Name = constructor.Name;
        }
        MergeEnumerableContent(this, constructor, settings);
    }

    /// <summary>
    /// Gets or sets the name of this constructor.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets the node type for this <see cref="JToken"/>.
    /// </summary>
    public override JTokenType Type => JTokenType.Constructor;

    /// <summary>
    /// Initializes a new instance of the <see cref="JConstructor"/> class.
    /// </summary>
    public JConstructor()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JConstructor"/> class from another <see cref="JConstructor"/> object.
    /// </summary>
    /// <param name="other">A <see cref="JConstructor"/> object to copy from.</param>
    public JConstructor(JConstructor other)
        : base(other)
    {
        Name = other.Name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JConstructor"/> class with the specified name and content.
    /// </summary>
    public JConstructor(string name, params object[] content)
        : this(name, (object)content)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JConstructor"/> class with the specified name and content.
    /// </summary>
    public JConstructor(string name, object content)
        : this(name)
    {
        Add(content);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JConstructor"/> class with the specified name.
    /// </summary>
    public JConstructor(string name)
    {
        if (name.Length == 0)
        {
            throw new ArgumentException("Constructor name cannot be empty.", nameof(name));
        }

        Name = name;
    }

    internal override bool DeepEquals(JToken node)
    {
        return node is JConstructor c && Name == c.Name && ContentsEqual(c);
    }

    internal override JToken CloneToken()
    {
        return new JConstructor(this);
    }

    /// <summary>
    /// Writes this token to a <see cref="JsonWriter"/>.
    /// </summary>
    public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
    {
        writer.WriteStartConstructor(Name!);

        var count = values.Count;
        for (var i = 0; i < count; i++)
        {
            values[i].WriteTo(writer, converters);
        }

        writer.WriteEndConstructor();
    }

    /// <summary>
    /// Gets the <see cref="JToken"/> with the specified key.
    /// </summary>
    public override JToken? this[object key]
    {
        get
        {
            if (key is not int i)
            {
                throw new ArgumentException($"Accessed JConstructor values with invalid key value: {MiscellaneousUtils.ToString(key)}. Argument position index expected.");
            }

            return GetItem(i);
        }
        set
        {
            if (key is not int i)
            {
                throw new ArgumentException($"Set JConstructor values with invalid key value: {MiscellaneousUtils.ToString(key)}. Argument position index expected.");
            }

            SetItem(i, value);
        }
    }

    internal override int GetDeepHashCode()
    {
        return (Name?.GetHashCode() ?? 0) ^ ContentsHashCode();
    }

    /// <summary>
    /// Loads a <see cref="JConstructor"/> from a <see cref="JsonReader"/>.
    /// </summary>
    /// <param name="reader">A <see cref="JsonReader"/> that will be read for the content of the <see cref="JConstructor"/>.</param>
    /// <returns>A <see cref="JConstructor"/> that contains the JSON that was read from the specified <see cref="JsonReader"/>.</returns>
    public new static JConstructor Load(JsonReader reader)
    {
        return Load(reader, null);
    }

    /// <summary>
    /// Loads a <see cref="JConstructor"/> from a <see cref="JsonReader"/>.
    /// </summary>
    /// <param name="reader">A <see cref="JsonReader"/> that will be read for the content of the <see cref="JConstructor"/>.</param>
    /// <param name="settings">The <see cref="JsonLoadSettings"/> used to load the JSON.
    /// If this is <c>null</c>, default load settings will be used.</param>
    /// <returns>A <see cref="JConstructor"/> that contains the JSON that was read from the specified <see cref="JsonReader"/>.</returns>
    public new static JConstructor Load(JsonReader reader, JsonLoadSettings? settings)
    {
        if (reader.TokenType == JsonToken.None)
        {
            if (!reader.Read())
            {
                throw JsonReaderException.Create(reader, "Error reading JConstructor from JsonReader.");
            }
        }

        reader.MoveToContent();

        if (reader.TokenType != JsonToken.StartConstructor)
        {
            throw JsonReaderException.Create(reader, $"Error reading JConstructor from JsonReader. Current JsonReader item is not a constructor: {reader.TokenType}");
        }

        var constructor = new JConstructor((string)reader.Value!);
        constructor.SetLineInfo(reader as IJsonLineInfo, settings);

        constructor.ReadTokenFrom(reader, settings);

        return constructor;
    }
}