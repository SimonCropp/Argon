﻿// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

/// <summary>
/// Represents a token that can contain other tokens.
/// </summary>
public abstract partial class JContainer :
    JToken,
    IList<JToken>
{
    /// <summary>
    /// Gets the container's children tokens.
    /// </summary>
    protected abstract IList<JToken> ChildrenTokens { get; }

    internal JContainer()
    {
    }

    internal JContainer(JContainer other)
        : this()
    {
        var i = 0;
        foreach (var child in other)
        {
            TryAddInternal(i, child, false);
            i++;
        }

        SetLineInfo(this, null);
    }

    /// <summary>
    /// Gets a value indicating whether this token has child tokens.
    /// </summary>
    public override bool HasValues => ChildrenTokens.Count > 0;

    internal bool ContentsEqual(JContainer container)
    {
        if (container == this)
        {
            return true;
        }

        var t1 = ChildrenTokens;
        var t2 = container.ChildrenTokens;

        if (t1.Count != t2.Count)
        {
            return false;
        }

        for (var i = 0; i < t1.Count; i++)
        {
            if (!t1[i].DeepEquals(t2[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Get the first child token of this token.
    /// </summary>
    public override JToken? First
    {
        get
        {
            var children = ChildrenTokens;
            return children.Count > 0 ? children[0] : null;
        }
    }

    /// <summary>
    /// Get the last child token of this token.
    /// </summary>
    public override JToken? Last
    {
        get
        {
            var children = ChildrenTokens;
            var count = children.Count;
            return count > 0 ? children[count - 1] : null;
        }
    }

    /// <summary>
    /// Returns a collection of the child tokens of this token, in document order.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="JToken"/> containing the child tokens of this <see cref="JToken"/>, in document order.
    /// </returns>
    public override JEnumerable<JToken> Children()
    {
        return new JEnumerable<JToken>(ChildrenTokens);
    }

    /// <summary>
    /// Returns a collection of the child values of this token, in document order.
    /// </summary>
    /// <typeparam name="T">The type to convert the values to.</typeparam>
    /// <returns>
    /// A <see cref="IEnumerable{T}"/> containing the child values of this <see cref="JToken"/>, in document order.
    /// </returns>
    public override IEnumerable<T?> Values<T>() where T : default
    {
        return ChildrenTokens.Convert<JToken, T>();
    }

    /// <summary>
    /// Returns a collection of the descendant tokens for this token in document order.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="JToken"/> containing the descendant tokens of the <see cref="JToken"/>.</returns>
    public IEnumerable<JToken> Descendants()
    {
        return GetDescendants(false);
    }

    /// <summary>
    /// Returns a collection of the tokens that contain this token, and all descendant tokens of this token, in document order.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="JToken"/> containing this token, and all the descendant tokens of the <see cref="JToken"/>.</returns>
    public IEnumerable<JToken> DescendantsAndSelf()
    {
        return GetDescendants(true);
    }

    internal IEnumerable<JToken> GetDescendants(bool self)
    {
        if (self)
        {
            yield return this;
        }

        foreach (var o in ChildrenTokens)
        {
            yield return o;
            if (o is JContainer c)
            {
                foreach (var d in c.Descendants())
                {
                    yield return d;
                }
            }
        }
    }

    internal bool IsMultiContent([NotNullWhen(true)]object? content)
    {
        return content is IEnumerable && content is not string && content is not JToken && content is not byte[];
    }

    internal JToken EnsureParentToken(JToken? item, bool skipParentCheck)
    {
        if (item == null)
        {
            return JValue.CreateNull();
        }

        if (skipParentCheck)
        {
            return item;
        }

        // to avoid a token having multiple parents or creating a recursive loop, create a copy if...
        // the item already has a parent
        // the item is being added to itself
        // the item is being added to the root parent of itself
        if (item.Parent != null || item == this || (item.HasValues && Root == item))
        {
            item = item.CloneToken();
        }

        return item;
    }

    internal abstract int IndexOfItem(JToken? item);

    internal virtual bool InsertItem(int index, JToken? item, bool skipParentCheck)
    {
        var children = ChildrenTokens;

        if (index > children.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be within the bounds of the List.");
        }

        item = EnsureParentToken(item, skipParentCheck);

        var previous = index == 0 ? null : children[index - 1];
        // haven't inserted new token yet so next token is still at the inserting index
        var next = index == children.Count ? null : children[index];

        ValidateToken(item, null);

        item.Parent = this;

        item.Previous = previous;
        if (previous != null)
        {
            previous.Next = item;
        }

        item.Next = next;
        if (next != null)
        {
            next.Previous = item;
        }

        children.Insert(index, item);

        return true;
    }

    internal virtual void RemoveItemAt(int index)
    {
        var children = ChildrenTokens;

        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is less than 0.");
        }
        if (index >= children.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is equal to or greater than Count.");
        }

        var item = children[index];
        var previous = index == 0 ? null : children[index - 1];
        var next = index == children.Count - 1 ? null : children[index + 1];

        if (previous != null)
        {
            previous.Next = next;
        }
        if (next != null)
        {
            next.Previous = previous;
        }

        item.Parent = null;
        item.Previous = null;
        item.Next = null;

        children.RemoveAt(index);
    }

    internal virtual bool RemoveItem(JToken? item)
    {
        if (item != null)
        {
            var index = IndexOfItem(item);
            if (index >= 0)
            {
                RemoveItemAt(index);
                return true;
            }
        }

        return false;
    }

    internal virtual JToken GetItem(int index)
    {
        return ChildrenTokens[index];
    }

    internal virtual void SetItem(int index, JToken? item)
    {
        var children = ChildrenTokens;

        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is less than 0.");
        }
        if (index >= children.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is equal to or greater than Count.");
        }

        var existing = children[index];

        if (IsTokenUnchanged(existing, item))
        {
            return;
        }

        item = EnsureParentToken(item, false);

        ValidateToken(item, existing);

        var previous = index == 0 ? null : children[index - 1];
        var next = index == children.Count - 1 ? null : children[index + 1];

        item.Parent = this;

        item.Previous = previous;
        if (previous != null)
        {
            previous.Next = item;
        }

        item.Next = next;
        if (next != null)
        {
            next.Previous = item;
        }

        children[index] = item;

        existing.Parent = null;
        existing.Previous = null;
        existing.Next = null;
    }

    internal virtual void ClearItems()
    {
        var children = ChildrenTokens;

        foreach (var item in children)
        {
            item.Parent = null;
            item.Previous = null;
            item.Next = null;
        }

        children.Clear();

    }

    internal virtual void ReplaceItem(JToken existing, JToken replacement)
    {
        if (existing.Parent != this)
        {
            return;
        }

        var index = IndexOfItem(existing);
        SetItem(index, replacement);
    }

    internal virtual bool ContainsItem(JToken? item)
    {
        return IndexOfItem(item) != -1;
    }

    internal virtual void CopyItemsTo(Array array, int arrayIndex)
    {
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
        foreach (var token in ChildrenTokens)
        {
            array.SetValue(token, arrayIndex + index);
            index++;
        }
    }

    internal static bool IsTokenUnchanged(JToken currentValue, JToken? newValue)
    {
        if (currentValue is JValue v1)
        {
            if (newValue == null)
            {
                // null will get turned into a JValue of type null
                return v1.Type == JTokenType.Null;
            }

            return v1.Equals(newValue);
        }

        return false;
    }

    internal virtual void ValidateToken(JToken o, JToken? existing)
    {
        if (o.Type == JTokenType.Property)
        {
            throw new ArgumentException($"Can not add {o.GetType()} to {GetType()}.");
        }
    }

    /// <summary>
    /// Adds the specified content as children of this <see cref="JToken"/>.
    /// </summary>
    /// <param name="content">The content to be added.</param>
    public virtual void Add(object? content)
    {
        TryAddInternal(ChildrenTokens.Count, content, false);
    }

    internal bool TryAdd(object? content)
    {
        return TryAddInternal(ChildrenTokens.Count, content, false);
    }

    internal void AddAndSkipParentCheck(JToken token)
    {
        TryAddInternal(ChildrenTokens.Count, token, true);
    }

    /// <summary>
    /// Adds the specified content as the first children of this <see cref="JToken"/>.
    /// </summary>
    /// <param name="content">The content to be added.</param>
    public void AddFirst(object? content)
    {
        TryAddInternal(0, content, false);
    }

    internal bool TryAddInternal(int index, object? content, bool skipParentCheck)
    {
        if (IsMultiContent(content))
        {
            var enumerable = (IEnumerable)content;

            var multiIndex = index;
            foreach (var c in enumerable)
            {
                TryAddInternal(multiIndex, c, skipParentCheck);
                multiIndex++;
            }

            return true;
        }

        var item = CreateFromContent(content);

        return InsertItem(index, item, skipParentCheck);
    }

    internal static JToken CreateFromContent(object? content)
    {
        if (content is JToken token)
        {
            return token;
        }

        return new JValue(content);
    }

    /// <summary>
    /// Creates a <see cref="JsonWriter"/> that can be used to add tokens to the <see cref="JToken"/>.
    /// </summary>
    /// <returns>A <see cref="JsonWriter"/> that is ready to have content written to it.</returns>
    public JsonWriter CreateWriter()
    {
        return new JTokenWriter(this);
    }

    /// <summary>
    /// Replaces the child nodes of this token with the specified content.
    /// </summary>
    public void ReplaceAll(object content)
    {
        ClearItems();
        Add(content);
    }

    /// <summary>
    /// Removes the child nodes from this token.
    /// </summary>
    public void RemoveAll()
    {
        ClearItems();
    }

    internal abstract void MergeItem(object content, JsonMergeSettings? settings);

    /// <summary>
    /// Merge the specified content into this <see cref="JToken"/>.
    /// </summary>
    public void Merge(object? content)
    {
        if (content == null)
        {
            return;
        }

        ValidateContent(content);
        MergeItem(content, null);
    }

    /// <summary>
    /// Merge the specified content into this <see cref="JToken"/> using <see cref="JsonMergeSettings"/>.
    /// </summary>
    public void Merge(object? content, JsonMergeSettings? settings)
    {
        if (content == null)
        {
            return;
        }

        ValidateContent(content);
        MergeItem(content, settings);
    }

    void ValidateContent(object content)
    {
        if (content.GetType().IsSubclassOf(typeof(JToken)))
        {
            return;
        }
        if (IsMultiContent(content))
        {
            return;
        }

        throw new ArgumentException($"Could not determine JSON object type for type {content.GetType()}.", nameof(content));
    }

    internal void ReadTokenFrom(JsonReader reader, JsonLoadSettings? options)
    {
        var startDepth = reader.Depth;

        if (!reader.Read())
        {
            throw JsonReaderException.Create(reader, $"Error reading {GetType().Name} from JsonReader.");
        }

        ReadContentFrom(reader, options);

        var endDepth = reader.Depth;

        if (endDepth > startDepth)
        {
            throw JsonReaderException.Create(reader, $"Unexpected end of content while loading {GetType().Name}.");
        }
    }

    internal void ReadContentFrom(JsonReader r, JsonLoadSettings? settings)
    {
        var lineInfo = r as IJsonLineInfo;

        var parent = this;

        do
        {
            if (parent is JProperty {Value: { }})
            {
                if (parent == this)
                {
                    return;
                }

                parent = parent.Parent;
            }

            MiscellaneousUtils.Assert(parent != null);

            switch (r.TokenType)
            {
                case JsonToken.None:
                    // new reader. move to actual content
                    break;
                case JsonToken.StartArray:
                    var a = new JArray();
                    a.SetLineInfo(lineInfo, settings);
                    parent.Add(a);
                    parent = a;
                    break;

                case JsonToken.EndArray:
                    if (parent == this)
                    {
                        return;
                    }

                    parent = parent.Parent;
                    break;
                case JsonToken.StartObject:
                    var o = new JObject();
                    o.SetLineInfo(lineInfo, settings);
                    parent.Add(o);
                    parent = o;
                    break;
                case JsonToken.EndObject:
                    if (parent == this)
                    {
                        return;
                    }

                    parent = parent.Parent;
                    break;
                case JsonToken.String:
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.Date:
                case JsonToken.Boolean:
                case JsonToken.Bytes:
                    var v = new JValue(r.Value);
                    v.SetLineInfo(lineInfo, settings);
                    parent.Add(v);
                    break;
                case JsonToken.Comment:
                    if (settings is {CommentHandling: CommentHandling.Load})
                    {
                        v = JValue.CreateComment(r.Value!.ToString());
                        v.SetLineInfo(lineInfo, settings);
                        parent.Add(v);
                    }
                    break;
                case JsonToken.Null:
                    v = JValue.CreateNull();
                    v.SetLineInfo(lineInfo, settings);
                    parent.Add(v);
                    break;
                case JsonToken.Undefined:
                    v = JValue.CreateUndefined();
                    v.SetLineInfo(lineInfo, settings);
                    parent.Add(v);
                    break;
                case JsonToken.PropertyName:
                    var property = ReadProperty(r, settings, lineInfo, parent);
                    if (property != null)
                    {
                        parent = property;
                    }
                    else
                    {
                        r.Skip();
                    }
                    break;
                default:
                    throw new InvalidOperationException($"The JsonReader should not be on a token of type {r.TokenType}.");
            }
        } while (r.Read());
    }

    static JProperty? ReadProperty(JsonReader r, JsonLoadSettings? settings, IJsonLineInfo? lineInfo, JContainer parent)
    {
        var duplicatePropertyNameHandling = settings?.DuplicatePropertyNameHandling ?? DuplicatePropertyNameHandling.Replace;

        var parentObject = (JObject)parent;
        var propertyName = r.Value!.ToString()!;
        var existingPropertyWithName = parentObject.Property(propertyName, StringComparison.Ordinal);
        if (existingPropertyWithName != null)
        {
            if (duplicatePropertyNameHandling == DuplicatePropertyNameHandling.Ignore)
            {
                return null;
            }

            if (duplicatePropertyNameHandling == DuplicatePropertyNameHandling.Error)
            {
                throw JsonReaderException.Create(r, $"Property with the name '{propertyName}' already exists in the current JSON object.");
            }
        }

        var property = new JProperty(propertyName);
        property.SetLineInfo(lineInfo, settings);
        // handle multiple properties with the same name in JSON
        if (existingPropertyWithName == null)
        {
            parent.Add(property);
        }
        else
        {
            existingPropertyWithName.Replace(property);
        }

        return property;
    }

    internal int ContentsHashCode()
    {
        var hashCode = 0;
        foreach (var item in ChildrenTokens)
        {
            hashCode ^= item.GetDeepHashCode();
        }
        return hashCode;
    }

    #region IList<JToken> Members
    int IList<JToken>.IndexOf(JToken item)
    {
        return IndexOfItem(item);
    }

    void IList<JToken>.Insert(int index, JToken item)
    {
        InsertItem(index, item, false);
    }

    void IList<JToken>.RemoveAt(int index)
    {
        RemoveItemAt(index);
    }

    JToken IList<JToken>.this[int index]
    {
        get => GetItem(index);
        set => SetItem(index, value);
    }
    #endregion

    #region ICollection<JToken> Members
    void ICollection<JToken>.Add(JToken item)
    {
        Add(item);
    }

    void ICollection<JToken>.Clear()
    {
        ClearItems();
    }

    bool ICollection<JToken>.Contains(JToken item)
    {
        return ContainsItem(item);
    }

    void ICollection<JToken>.CopyTo(JToken[] array, int arrayIndex)
    {
        CopyItemsTo(array, arrayIndex);
    }

    bool ICollection<JToken>.IsReadOnly => false;

    bool ICollection<JToken>.Remove(JToken item)
    {
        return RemoveItem(item);
    }
    #endregion

    static JToken? EnsureValue(object? value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is JToken token)
        {
            return token;
        }

        throw new ArgumentException("Argument is not a JToken.");
    }

    /// <summary>
    /// Gets the count of child JSON tokens.
    /// </summary>
    public int Count => ChildrenTokens.Count;

    internal static void MergeEnumerableContent(JContainer target, IEnumerable content, JsonMergeSettings? settings)
    {
        switch (settings?.MergeArrayHandling ?? MergeArrayHandling.Concat)
        {
            case MergeArrayHandling.Concat:
                foreach (var item in content)
                {
                    target.Add(CreateFromContent(item));
                }
                break;
            case MergeArrayHandling.Union:
                var items = new HashSet<JToken>(target, EqualityComparer);

                foreach (var item in content)
                {
                    var contentItem = CreateFromContent(item);

                    if (items.Add(contentItem))
                    {
                        target.Add(contentItem);
                    }
                }
                break;
            case MergeArrayHandling.Replace:
                if (target == content)
                {
                    break;
                }
                target.ClearItems();
                foreach (var item in content)
                {
                    target.Add(CreateFromContent(item));
                }
                break;
            case MergeArrayHandling.Merge:
                var i = 0;
                foreach (var targetItem in content)
                {
                    if (i < target.Count)
                    {
                        var sourceItem = target[i];

                        if (sourceItem is JContainer existingContainer)
                        {
                            existingContainer.Merge(targetItem, settings);
                        }
                        else
                        {
                            if (targetItem != null)
                            {
                                var contentValue = CreateFromContent(targetItem);
                                if (contentValue.Type != JTokenType.Null)
                                {
                                    target[i] = contentValue;
                                }
                            }
                        }
                    }
                    else
                    {
                        target.Add(CreateFromContent(targetItem));
                    }

                    i++;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(settings), "Unexpected merge array handling when merging JSON.");
        }
    }
}