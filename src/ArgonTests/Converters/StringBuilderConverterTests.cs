// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class StringBuilderConverterTests : TestFixtureBase
{
    [Fact]
    public void ImplicitConverter()
    {
        var builder = new StringBuilder("value");
        var json = JsonConvert.SerializeObject(builder);
        var builder2 = JsonConvert.DeserializeObject<StringBuilder>(json);
        var json2 = JsonConvert.SerializeObject(builder2);

        Assert.Equal(json, json2);
    }

    public class Target
    {
        public StringBuilder Builder { get; set; }
    }

    [Fact]
    public void Nested()
    {
        var target = new Target
        {
            Builder = new("value")
        };
        var json = JsonConvert.SerializeObject(target);
        var builder2 = JsonConvert.DeserializeObject<Target>(json);
        var json2 = JsonConvert.SerializeObject(builder2);

        Assert.Equal(json, json2);
    }
}