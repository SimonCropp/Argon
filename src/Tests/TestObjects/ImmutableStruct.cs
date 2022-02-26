// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public struct ImmutableStruct
{
    public ImmutableStruct(string value)
    {
        Value = value;
        Value2 = 0;
    }

    public string Value { get; }
    public int Value2 { get; set; }
}