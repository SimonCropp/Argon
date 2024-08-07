// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public sealed class ConstructorAndRequiredTestClass(string testProperty1)
{
    public string TestProperty1 { get; set; } = testProperty1;

    [JsonProperty(Required = Required.AllowNull)]
    public int TestProperty2 { get; set; }
}