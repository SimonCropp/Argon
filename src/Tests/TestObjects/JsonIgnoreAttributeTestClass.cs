// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class JsonIgnoreAttributeTestClass
{
    public int Field;

    public int Property { get; } = 21;

    [JsonIgnore]
    public int IgnoredField;

    [JsonIgnore]
    public int IgnoredProperty { get; } = 12;

    [JsonIgnore]
    public Product IgnoredObject = new();
}