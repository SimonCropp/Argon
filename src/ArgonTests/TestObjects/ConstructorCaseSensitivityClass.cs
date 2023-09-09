// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class ConstructorCaseSensitivityClass(string param1, string Param1, string param2)
{
    public string param1 { get; set; } = param1;
    public string Param1 { get; set; } = Param1;
    public string Param2 { get; set; } = param2;
}