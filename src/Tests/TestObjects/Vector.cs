// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public struct Vector
{
    public float X;
    public float Y;
    public float Z;

    public override string ToString() =>
        $"({X},{Y},{Z})";
}