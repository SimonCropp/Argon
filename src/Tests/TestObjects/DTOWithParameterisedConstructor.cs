// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class DTOWithParameterisedConstructor
{
    public DTOWithParameterisedConstructor(string A)
    {
        this.A = A;
        B = 2;
    }

    public string A { get; set; }
    public int? B { get; set; }
}