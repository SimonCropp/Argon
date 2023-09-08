// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class StringWriterConverterTests : TestFixtureBase
{
    [Fact]
    public void ImplicitConverter()
    {
        var target = new StringWriter(new StringBuilder("value"));
        var json = JsonConvert.SerializeObject(target);
        var target2 = JsonConvert.DeserializeObject<StringWriter>(json);
        var json2 = JsonConvert.SerializeObject(target2);

        Assert.Equal(json, json2);
    }

    public class Target
    {
        public StringWriter Writer { get; set; }
    }

    [Fact]
    public void Nested()
    {
        var target = new Target
        {
            Writer = new(new StringBuilder("value"))
        };
        var json = JsonConvert.SerializeObject(target);
        var target2 = JsonConvert.DeserializeObject<Target>(json);
        var json2 = JsonConvert.SerializeObject(target2);

        Assert.Equal(json, json2);
    }
}