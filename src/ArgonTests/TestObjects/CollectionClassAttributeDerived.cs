// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class CollectionClassAttributeDerived : ClassAttributeBase, ICollection<object>
{
    [JsonProperty]
    public string CollectionDerivedClassValue { get; set; }

    public void Add(object item) =>
        throw new NotImplementedException();

    public void Clear() =>
        throw new NotImplementedException();

    public bool Contains(object item) =>
        throw new NotImplementedException();

    public void CopyTo(object[] array, int arrayIndex) =>
        throw new NotImplementedException();

    public int Count => throw new NotImplementedException();

    public bool IsReadOnly => throw new NotImplementedException();

    public bool Remove(object item) =>
        throw new NotImplementedException();

    public IEnumerator<object> GetEnumerator() =>
        throw new NotImplementedException();

    IEnumerator IEnumerable.GetEnumerator() =>
        throw new NotImplementedException();
}