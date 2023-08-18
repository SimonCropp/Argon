// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[JsonObject(MemberSerialization.OptIn)]
public class GameObject
{
    [JsonProperty]
    public string Id { get; set; }

    [JsonProperty]
    public string Name { get; set; }

    [JsonProperty]
    public ConcurrentDictionary<string, Component> Components = new();
}