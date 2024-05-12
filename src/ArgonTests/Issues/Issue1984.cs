// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1984
{
    [Fact]
    public void Test_NullValue()
    {
        var actual = JsonConvert.DeserializeObject<A>("{ Values: null}");
        Assert.NotNull(actual);
        Assert.Null(actual.Values);
    }

    [Fact]
    public void Test_WithoutValue()
    {
        var actual = JsonConvert.DeserializeObject<A>("{ }");
        Assert.NotNull(actual);
        Assert.Null(actual.Values);
    }

    public class A
    {
        public ImmutableArray<string>? Values { get; set; }
    }
}