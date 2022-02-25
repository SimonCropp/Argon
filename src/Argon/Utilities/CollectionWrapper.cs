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

#nullable disable

class CollectionWrapper<T> : ICollection<T>, IWrappedCollection
{
    readonly IList list;
    readonly ICollection<T> genericCollection;
    object syncRoot;

    public CollectionWrapper(IList list)
    {
        if (list is ICollection<T> collection)
        {
            genericCollection = collection;
        }
        else
        {
            this.list = list;
        }
    }

    public CollectionWrapper(ICollection<T> list)
    {
        genericCollection = list;
    }

    public virtual void Add(T item)
    {
        if (genericCollection != null)
        {
            genericCollection.Add(item);
        }
        else
        {
            list!.Add(item);
        }
    }

    public virtual void Clear()
    {
        if (genericCollection != null)
        {
            genericCollection.Clear();
        }
        else
        {
            list!.Clear();
        }
    }

    public virtual bool Contains(T item)
    {
        if (genericCollection != null)
        {
            return genericCollection.Contains(item);
        }

        return list!.Contains(item);
    }

    public virtual void CopyTo(T[] array, int arrayIndex)
    {
        if (genericCollection == null)
        {
            list!.CopyTo(array, arrayIndex);
        }
        else
        {
            genericCollection.CopyTo(array, arrayIndex);
        }
    }

    public virtual int Count
    {
        get
        {
            if (genericCollection == null)
            {
                return list!.Count;
            }

            return genericCollection.Count;
        }
    }

    public virtual bool IsReadOnly
    {
        get
        {
            if (genericCollection == null)
            {
                return list!.IsReadOnly;
            }

            return genericCollection.IsReadOnly;
        }
    }

    public virtual bool Remove(T item)
    {
        if (genericCollection != null)
        {
            return genericCollection.Remove(item);
        }

        var contains = list!.Contains(item);

        if (contains)
        {
            list!.Remove(item);
        }

        return contains;
    }

    public virtual IEnumerator<T> GetEnumerator()
    {
        return (genericCollection ?? list.Cast<T>()).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)genericCollection! ?? list!).GetEnumerator();
    }

    int IList.Add(object value)
    {
        VerifyValueType(value);
        Add((T)value);

        return Count - 1;
    }

    bool IList.Contains(object value)
    {
        return IsCompatibleObject(value) &&
               Contains((T)value);
    }

    int IList.IndexOf(object value)
    {
        if (genericCollection != null)
        {
            throw new InvalidOperationException("Wrapped ICollection<T> does not support IndexOf.");
        }

        if (IsCompatibleObject(value))
        {
            return list!.IndexOf((T)value);
        }

        return -1;
    }

    void IList.RemoveAt(int index)
    {
        if (genericCollection != null)
        {
            throw new InvalidOperationException("Wrapped ICollection<T> does not support RemoveAt.");
        }

        list!.RemoveAt(index);
    }

    void IList.Insert(int index, object value)
    {
        if (genericCollection != null)
        {
            throw new InvalidOperationException("Wrapped ICollection<T> does not support Insert.");
        }

        VerifyValueType(value);
        list!.Insert(index, (T)value);
    }

    bool IList.IsFixedSize
    {
        get
        {
            if (genericCollection != null)
            {
                // ICollection<T> only has IsReadOnly
                return genericCollection.IsReadOnly;
            }

            return list!.IsFixedSize;
        }
    }

    void IList.Remove(object value)
    {
        if (IsCompatibleObject(value))
        {
            Remove((T)value);
        }
    }

    object IList.this[int index]
    {
        get
        {
            if (genericCollection != null)
            {
                throw new InvalidOperationException("Wrapped ICollection<T> does not support indexer.");
            }

            return list![index];
        }
        set
        {
            if (genericCollection != null)
            {
                throw new InvalidOperationException("Wrapped ICollection<T> does not support indexer.");
            }

            VerifyValueType(value);
            list![index] = (T)value;
        }
    }

    void ICollection.CopyTo(Array array, int arrayIndex)
    {
        CopyTo((T[])array, arrayIndex);
    }

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot
    {
        get
        {
            if (syncRoot == null)
            {
                Interlocked.CompareExchange(ref syncRoot, new object(), null);
            }

            return syncRoot;
        }
    }

    static void VerifyValueType(object value)
    {
        if (!IsCompatibleObject(value))
        {
            throw new ArgumentException($"The value '{value}' is not of type '{typeof(T)}' and cannot be used in this generic collection.", nameof(value));
        }
    }

    static bool IsCompatibleObject(object value)
    {
        return value is T ||
               (value == null && (!typeof(T).IsValueType || ReflectionUtils.IsNullableType(typeof(T))));
    }

    public object UnderlyingCollection => (object)genericCollection! ?? list!;
}