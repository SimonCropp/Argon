// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class IgnoredProperty
{
    [JsonIgnore]
    [JsonProperty(Required = Required.Always)]
    public string StringProp1 { get; set; }

    [JsonIgnore]
    public string StringProp2 { get; set; }
}