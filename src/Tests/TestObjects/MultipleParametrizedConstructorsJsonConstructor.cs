// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class MultipleParametrizedConstructorsJsonConstructor
{
    public string Value { get; }
    public int Age { get; }
    public string Constructor { get; }

    public MultipleParametrizedConstructorsJsonConstructor(string value)
    {
        Value = value;
        Constructor = "Public Parameterized 1";
    }

    [Argon.JsonConstructor]
    public MultipleParametrizedConstructorsJsonConstructor(string value, int age)
    {
        Value = value;
        Age = age;
        Constructor = "Public Parameterized 2";
    }
}