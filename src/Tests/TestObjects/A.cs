// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[JsonObject(MemberSerialization.OptIn)]
public class A
{
    [JsonProperty("A1")]
    string _A1;

    public string A1
    {
        get => _A1;
        set => _A1 = value;
    }

    [JsonProperty("A2")]
    string A2 { get; set; }
}