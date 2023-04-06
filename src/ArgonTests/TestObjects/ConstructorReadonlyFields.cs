// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class ConstructorReadonlyFields
{
    public readonly string A;
    public readonly int B;

    public ConstructorReadonlyFields(string a, int b)
    {
        A = a;
        B = b;
    }
}