// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[JsonObject(Id = "MyExplicitId")]
public class CircularReferenceWithIdClass
{
    [JsonProperty(Required = Required.AllowNull)]
    public string Name { get; set; }

    public CircularReferenceWithIdClass Child { get; set; }
}