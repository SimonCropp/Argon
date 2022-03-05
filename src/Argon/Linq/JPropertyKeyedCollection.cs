﻿// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.Collections.ObjectModel;

class JPropertyKeyedCollection : Collection<JToken>
{
    static readonly IEqualityComparer<string> Comparer = StringComparer.Ordinal;

    Dictionary<string, JToken>? dictionary;

    public JPropertyKeyedCollection() : base(new List<JToken>())
    {
    }

    void AddKey(string key, JToken item)
    {
        EnsureDictionary();
        dictionary![key] = item;
    }

    protected void ChangeItemKey(JToken item, string newKey)
    {
        if (!ContainsItem(item))
        {
            throw new ArgumentException("The specified item does not exist in this KeyedCollection.");
        }

        var keyForItem = GetKeyForItem(item);
        if (!Comparer.Equals(keyForItem, newKey))
        {
            if (newKey != null)
            {
                AddKey(newKey, item);
            }

            if (keyForItem != null)
            {
                RemoveKey(keyForItem);
            }
        }
    }

    protected override void ClearItems()
    {
        base.ClearItems();

        dictionary?.Clear();
    }

    public bool Contains(string key)
    {
        if (dictionary != null)
        {
            return dictionary.ContainsKey(key);
        }

        return false;
    }

    bool ContainsItem(JToken item)
    {
        if (dictionary == null)
        {
            return false;
        }

        var key = GetKeyForItem(item);
        return dictionary.TryGetValue(key, out _);
    }

    void EnsureDictionary()
    {
        dictionary ??= new(Comparer);
    }

    static string GetKeyForItem(JToken item)
    {
        return ((JProperty) item).Name;
    }

    protected override void InsertItem(int index, JToken item)
    {
        AddKey(GetKeyForItem(item), item);
        base.InsertItem(index, item);
    }

    public bool Remove(string key)
    {
        if (dictionary != null)
        {
            return dictionary.TryGetValue(key, out var value) && Remove(value);
        }

        return false;
    }

    protected override void RemoveItem(int index)
    {
        var keyForItem = GetKeyForItem(Items[index]);
        RemoveKey(keyForItem);
        base.RemoveItem(index);
    }

    void RemoveKey(string key)
    {
        dictionary?.Remove(key);
    }

    protected override void SetItem(int index, JToken item)
    {
        var keyForItem = GetKeyForItem(item);
        var keyAtIndex = GetKeyForItem(Items[index]);

        if (Comparer.Equals(keyAtIndex, keyForItem))
        {
            if (dictionary != null)
            {
                dictionary[keyForItem] = item;
            }
        }
        else
        {
            AddKey(keyForItem, item);

            if (keyAtIndex != null)
            {
                RemoveKey(keyAtIndex);
            }
        }

        base.SetItem(index, item);
    }

    public JToken this[string key]
    {
        get
        {
            if (dictionary != null)
            {
                return dictionary[key];
            }

            throw new KeyNotFoundException();
        }
    }

    public bool TryGetValue(string key, [NotNullWhen(true)] out JToken? value)
    {
        if (dictionary == null)
        {
            value = null;
            return false;
        }

        return dictionary.TryGetValue(key, out value);
    }

    public ICollection<string> Keys
    {
        get
        {
            EnsureDictionary();
            return dictionary!.Keys;
        }
    }

    public ICollection<JToken> Values
    {
        get
        {
            EnsureDictionary();
            return dictionary!.Values;
        }
    }

    public int IndexOfReference(JToken t)
    {
        return ((List<JToken>) Items).IndexOfReference(t);
    }

    public bool Compare(JPropertyKeyedCollection other)
    {
        if (this == other)
        {
            return true;
        }

        // dictionaries in JavaScript aren't ordered
        // ignore order when comparing properties
        var d1 = dictionary;
        var d2 = other.dictionary;

        if (d1 == null && d2 == null)
        {
            return true;
        }

        if (d1 == null)
        {
            return d2!.Count == 0;
        }

        if (d2 == null)
        {
            return d1.Count == 0;
        }

        if (d1.Count != d2.Count)
        {
            return false;
        }

        foreach (var keyAndProperty in d1)
        {
            if (!d2.TryGetValue(keyAndProperty.Key, out var secondValue))
            {
                return false;
            }

            var p1 = (JProperty) keyAndProperty.Value;
            var p2 = (JProperty) secondValue;

            if (p1.Value == null)
            {
                return p2.Value == null;
            }

            if (!p1.Value.DeepEquals(p2.Value))
            {
                return false;
            }
        }

        return true;
    }
}