// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[JsonObject(MemberSerialization.OptIn)]
public class B : A
{
    public string B1 { get; set; }

    [JsonProperty("B2")]
    string _B2;

    public string B2
    {
        get => _B2;
        set => _B2 = value;
    }

    [JsonProperty("B3")]
    string B3 { get; set; }
}