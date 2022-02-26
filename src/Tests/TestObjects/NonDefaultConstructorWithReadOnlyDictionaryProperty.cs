// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class NonDefaultConstructorWithReadOnlyDictionaryProperty
{
    public string Title { get; set; }
    public IDictionary<string, int> Categories { get; }

    public NonDefaultConstructorWithReadOnlyDictionaryProperty(string title)
    {
        Title = title;
        Categories = new Dictionary<string, int>();
    }
}