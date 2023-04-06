// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.ComponentModel;

namespace TestObjects;

public class ListSourceTest : IListSource
{
    string str;

    public string strprop
    {
        get => str;
        set => str = value;
    }

    [JsonIgnore]
    public bool ContainsListCollection => false;

    public IList GetList() =>
        new List<string>();
}