// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class ConstructorReadonlyFields(string a, int b)
{
    public readonly string A = a;
    public readonly int B = b;
}