// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

class DictionaryKeyCast
{
    string _name;
    int _number;

    public DictionaryKeyCast(string name, int number)
    {
        _name = name;
        _number = number;
    }

    public override string ToString() =>
        $"{_name} {_number}";

    public static implicit operator DictionaryKeyCast(string dictionaryKey)
    {
        var strings = dictionaryKey.Split(' ');
        return new(strings[0], Convert.ToInt32(strings[1]));
    }
}