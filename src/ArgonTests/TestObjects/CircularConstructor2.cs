// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class CircularConstructor2(CircularConstructor1 c1)
{
    public CircularConstructor1 C1 { get; internal set; } = c1;
    public int IntProperty { get; set; }
}