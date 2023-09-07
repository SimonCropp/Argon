// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local
namespace TestObjects;

public class PrivateConstructorWithPublicParameterizedConstructorTestClass
{
    public string Name { get; set; }
    public int Age { get; set; }

    PrivateConstructorWithPublicParameterizedConstructorTestClass() =>
        Age = 1;

    public PrivateConstructorWithPublicParameterizedConstructorTestClass(string dummy) =>
        throw new("Should never get here.");
}