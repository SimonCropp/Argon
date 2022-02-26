// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class CircularConstructor1
{
    public CircularConstructor2 C2 { get; internal set; }
    public string StringProperty { get; set; }

    public CircularConstructor1(CircularConstructor2 c2)
    {
        C2 = c2;
    }
}