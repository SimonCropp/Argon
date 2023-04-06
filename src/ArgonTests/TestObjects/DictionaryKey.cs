// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class DictionaryKey
{
    public string Value { get; set; }

    public override string ToString() =>
        Value;

    public static implicit operator DictionaryKey(string value) =>
        new() { Value = value };
}