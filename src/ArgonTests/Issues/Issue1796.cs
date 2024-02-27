// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1796 : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        var json = "[{}]";
        var c = JsonConvert.DeserializeObject<TestStack>(json);
        Assert.Single(c);
    }

    [Fact]
    public void Test_Generic()
    {
        var json = "['hi']";
        var c = JsonConvert.DeserializeObject<TestStack<string>>(json);
        Assert.Single(c);
    }

    public class TestStack : SortedSet<object>;

    public class TestStack<T> : SortedSet<T>;
}