// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1711 : TestFixtureBase
{
    [Fact]
    public void Test_Raw()
    {
        var c = JsonConvert.DeserializeObject<FooClass>(@"{ ""Value"" : 96.014e-05 }");
        Assert.Equal(0.00096014m, c.Value);
    }

    [Fact]
    public void Test_String()
    {
        var c = JsonConvert.DeserializeObject<FooClass>(@"{ ""Value"" : ""96.014e-05"" }");
        Assert.Equal(0.00096014m, c.Value);
    }

    public class FooClass
    {
        public decimal? Value { get; set; }
    }
}