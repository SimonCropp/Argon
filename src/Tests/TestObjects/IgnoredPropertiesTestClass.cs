// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class IgnoredPropertiesTestClass
{
    [JsonIgnore]
    public Version IgnoredProperty { get; set; }

    [JsonIgnore]
    public List<Version> IgnoredList { get; set; }

    [JsonIgnore]
    public Dictionary<string, Version> IgnoredDictionary { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string Name { get; set; }
}