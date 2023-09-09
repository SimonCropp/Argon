// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class ConstructorComplexIgnoredProperty(string first, int second)
{
    [JsonIgnore]
    public Product Ignored { get; set; }

    public string First { get; set; } = first;
    public int Second { get; set; } = second;
}