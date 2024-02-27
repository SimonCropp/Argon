// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.


public class JRawTests : TestFixtureBase
{
    [Fact]
    public void RawEquals()
    {
        var r1 = new JRaw("raw1");
        var r2 = new JRaw("raw1");
        var r3 = new JRaw("raw2");

        Assert.True(JToken.DeepEquals(r1, r2));
        Assert.False(JToken.DeepEquals(r1, r3));
    }

    [Fact]
    public void RawClone()
    {
        var r1 = new JRaw("raw1");
        var r2 = r1.CloneToken();

        Assert.IsType<JRaw>(r2);
    }

    [Fact]
    public void RawToObject()
    {
        var r1 = new JRaw("1");
        var i = r1.ToObject<int>();

        Assert.Equal(1, i);
    }
}