// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[JsonObject(IsReference = true)]
public class EmployeeReference
{
    public string Name { get; set; }
    public EmployeeReference Manager { get; set; }
}