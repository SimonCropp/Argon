// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class EnumerableArrayPropertyClass
{
    public IEnumerable<int> Numbers => new[] { 1, 2, 3 }; //fails
    //return new List<int>(new[] { 1, 2, 3 }); //works
}