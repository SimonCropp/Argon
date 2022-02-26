// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class PersonReference
{
    internal Guid Id { get; set; }
    public string Name { get; set; }
    public PersonReference Spouse { get; set; }
}