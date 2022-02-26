// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class AATestClass
{
    [JsonProperty]
    protected int AA_field1;

    protected int AA_field2;

    [JsonProperty]
    protected int AA_property1 { get; set; }

    [JsonProperty]
    protected int AA_property2 { get; private set; }

    [JsonProperty]
    protected int AA_property3 { private get; set; }

    [JsonProperty]
    int AA_property4 { get; set; }

    protected int AA_property5 { get; private set; }
    protected int AA_property6 { private get; set; }

    public AATestClass()
    {
    }

    public AATestClass(int f)
    {
        AA_field1 = f;
        AA_field2 = f;
        AA_property1 = f;
        AA_property2 = f;
        AA_property3 = f;
        AA_property4 = f;
        AA_property5 = f;
        AA_property6 = f;
    }
}