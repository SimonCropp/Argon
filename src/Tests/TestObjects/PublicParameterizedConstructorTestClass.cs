// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class PublicParameterizedConstructorTestClass
{
    readonly string _name;

    public PublicParameterizedConstructorTestClass(string name)
    {
        _name = name;
    }

    public string Name => _name;
}