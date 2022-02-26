// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class BBTestClass : AATestClass
{
    [JsonProperty]
    public int BB_field1;

    public int BB_field2;

    [JsonProperty]
    public int BB_property1 { get; set; }

    [JsonProperty]
    public int BB_property2 { get; private set; }

    [JsonProperty]
    public int BB_property3 { private get; set; }

    [JsonProperty]
    int BB_property4 { get; set; }

    public int BB_property5 { get; private set; }
    public int BB_property6 { private get; set; }

    [JsonProperty]
    public int BB_property7 { protected get; set; }

    public int BB_property8 { protected get; set; }

    public BBTestClass()
    {
    }

    public BBTestClass(int f, int g)
        : base(f)
    {
        BB_field1 = g;
        BB_field2 = g;
        BB_property1 = g;
        BB_property2 = g;
        BB_property3 = g;
        BB_property4 = g;
        BB_property5 = g;
        BB_property6 = g;
        BB_property7 = g;
        BB_property8 = g;
    }
}