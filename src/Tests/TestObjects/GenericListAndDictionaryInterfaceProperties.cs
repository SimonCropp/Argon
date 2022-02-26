// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class GenericListAndDictionaryInterfaceProperties
{
    public IEnumerable<int> IEnumerableProperty { get; set; }
    public IList<int> IListProperty { get; set; }
    public IDictionary<string, int> IDictionaryProperty { get; set; }
}