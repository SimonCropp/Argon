// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class TestObject
{
    public TestObject()
    {
    }

    public TestObject(string name, byte[] data)
    {
        Name = name;
        Data = data;
    }

    public string Name { get; set; }
    public byte[] Data { get; set; }
}