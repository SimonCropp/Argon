// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[JsonObject(ItemRequired = Required.Always)]
public class RequiredObject
{
    public int? NonAttributeProperty { get; set; }

    [JsonProperty]
    public int? UnsetProperty { get; set; }

    [JsonProperty(Required = Required.Default)]
    public int? DefaultProperty { get; set; }

    [JsonProperty(Required = Required.AllowNull)]
    public int? AllowNullProperty { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int? AlwaysProperty { get; set; }
}