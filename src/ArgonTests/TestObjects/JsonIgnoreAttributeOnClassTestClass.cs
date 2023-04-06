// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[JsonObject(MemberSerialization.OptIn)]
public class JsonIgnoreAttributeOnClassTestClass
{
    [JsonProperty("TheField")]
    public int Field;

    [JsonProperty]
    public int Property { get; } = 21;

    public int IgnoredField;

    [JsonProperty]
    [JsonIgnore] // JsonIgnore should take priority
    public int IgnoredProperty { get; } = 12;
}