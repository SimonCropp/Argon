// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[JsonObject(MemberSerialization.OptIn)]
public class DecimalTest : Test<decimal>
{
    protected DecimalTest()
    {
    }

    public DecimalTest(decimal val)
    {
        Value = val;
    }

    [JsonProperty]
    public override decimal Value { get; set; }
}