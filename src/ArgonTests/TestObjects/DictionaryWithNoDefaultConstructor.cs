// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class DictionaryWithNoDefaultConstructor : Dictionary<string, string>
{
    public DictionaryWithNoDefaultConstructor(IEnumerable<KeyValuePair<string, string>> initial)
    {
        foreach (var pair in initial)
        {
            Add(pair.Key, pair.Value);
        }
    }
}