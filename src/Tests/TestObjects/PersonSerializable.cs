// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class PersonSerializable
{
    public string Name { get; set; } = "";

    [field: NonSerialized]
    public int Age { get; set; } = 0;
}