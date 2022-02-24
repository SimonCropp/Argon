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

    public void Add(KeyValuePair<string, T> item)
    {
        ((IDictionary<string, T>)innerDictionary).Add(item);
    }

    public void Add(string key, T value)
    {
        innerDictionary.Add(key, value);
    }

    public void Clear()
    {
        innerDictionary.Clear();
    }

    public bool Contains(KeyValuePair<string, T> item)
    {
        return ((IDictionary<string, T>)innerDictionary).Contains(item);
    }

    public bool ContainsKey(string key)
    {
        return innerDictionary.ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
    {
        ((IDictionary<string, T>)innerDictionary).CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
    {
        return innerDictionary.GetEnumerator();
    }

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

    public bool Remove(KeyValuePair<string, T> item)
    {
        return ((IDictionary<string, T>)innerDictionary).Remove(item);
    }

    public bool Remove(string key)
    {
        return innerDictionary.Remove(key);
    }

    public bool TryGetValue(string key, out T value)
    {
        return innerDictionary.TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)innerDictionary).GetEnumerator();
    }
}