// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local
namespace TestObjects;

public class PrivateConstructorTestClass
{
    public string Name { get; set; }
    public int Age { get; set; }

    PrivateConstructorTestClass()
    {
    }

    // multiple constructors with arguments so the serializer doesn't know what to fall back to
    PrivateConstructorTestClass(object a)
    {
    }

    // multiple constructors with arguments so the serializer doesn't know what to fall back to
    PrivateConstructorTestClass(object a, object b)
    {
    }
}