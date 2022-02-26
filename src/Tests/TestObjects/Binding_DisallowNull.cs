// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class Binding_DisallowNull
{
    [JsonProperty(Required = Required.DisallowNull)]
    public Binding RequiredProperty { get; set; }
}