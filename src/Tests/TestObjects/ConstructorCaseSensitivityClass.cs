// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class ConstructorCaseSensitivityClass
{
    public string param1 { get; set; }
    public string Param1 { get; set; }
    public string Param2 { get; set; }

    public ConstructorCaseSensitivityClass(string param1, string Param1, string param2)
    {
        this.param1 = param1;
        this.Param1 = Param1;
        Param2 = param2;
    }
}