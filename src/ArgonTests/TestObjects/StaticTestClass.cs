// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[JsonObject(MemberSerialization.OptIn)]
public class StaticTestClass
{
    [JsonProperty]
    public int x = 1;

    [JsonProperty]
    public static int y = 2;

    [JsonProperty]
    public static int z { get; set; }

    static StaticTestClass() =>
        z = 3;
}