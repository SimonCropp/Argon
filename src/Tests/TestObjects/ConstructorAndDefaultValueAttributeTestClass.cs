// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.ComponentModel;

namespace TestObjects;

public sealed class ConstructorAndDefaultValueAttributeTestClass
{
    public ConstructorAndDefaultValueAttributeTestClass(string testProperty1)
    {
        TestProperty1 = testProperty1;
    }

    public string TestProperty1 { get; set; }

    [DefaultValue(21)]
    public int TestProperty2 { get; set; }
}