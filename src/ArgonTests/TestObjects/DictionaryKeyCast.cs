// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

class DictionaryKeyCast(string name, int number)
{
    public override string ToString() =>
        $"{name} {number}";

    public static implicit operator DictionaryKeyCast(string dictionaryKey)
    {
        var strings = dictionaryKey.Split(' ');
        return new(strings[0], Convert.ToInt32(strings[1]));
    }
}