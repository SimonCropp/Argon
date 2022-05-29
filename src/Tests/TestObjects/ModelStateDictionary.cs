// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class ModelStateDictionary<T> : IDictionary<string, T>
{
    readonly Dictionary<string, T> innerDictionary = new(StringComparer.OrdinalIgnoreCase);

    public ModelStateDictionary()
    {
    }

    public ModelStateDictionary(ModelStateDictionary<T> dictionary)
    {
        foreach (var entry in dictionary)
        {
            innerDictionary.Add(entry.Key, entry.Value);
        }
    }

    public int Count => innerDictionary.Count;

    public bool IsReadOnly => ((IDictionary<string, T>)innerDictionary).IsReadOnly;

    public ICollection<string> Keys => innerDictionary.Keys;

    public T this[string key]
    {
        get
        {
            innerDictionary.TryGetValue(key, out var value);
            return value;
        }
        set => innerDictionary[key] = value;
    }

    public ICollection<T> Values => innerDictionary.Values;

    public void Add(KeyValuePair<string, T> item) =>
        ((IDictionary<string, T>)innerDictionary).Add(item);

    public void Add(string key, T value) =>
        innerDictionary.Add(key, value);

    public void Clear() =>
        innerDictionary.Clear();

    public bool Contains(KeyValuePair<string, T> item) =>
        ((IDictionary<string, T>)innerDictionary).Contains(item);

    public bool ContainsKey(string key) =>
        innerDictionary.ContainsKey(key);

    public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex) =>
        ((IDictionary<string, T>)innerDictionary).CopyTo(array, arrayIndex);

    public IEnumerator<KeyValuePair<string, T>> GetEnumerator() =>
        innerDictionary.GetEnumerator();

    public void Merge(ModelStateDictionary<T> dictionary)
    {
        if (dictionary == null)
        {
            return;
        }

        foreach (var entry in dictionary)
        {
            this[entry.Key] = entry.Value;
        }
    }

    public bool Remove(KeyValuePair<string, T> item) =>
        ((IDictionary<string, T>)innerDictionary).Remove(item);

    public bool Remove(string key) =>
        innerDictionary.Remove(key);

    public bool TryGetValue(string key, out T value) =>
        innerDictionary.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator() =>
        ((IEnumerable)innerDictionary).GetEnumerator();
}