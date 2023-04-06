// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1362 : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        var js = "[2.8144, 2.8144, 2.6962, 2.6321, 2.5693, 2.5243, 2.5087, 2.5504, 2.535, 2.5506, 2.532, 2.491, 2.3533]";
        var values = (double[])JsonConvert.DeserializeObject(js, typeof(double[]));

        var value = values[7];

        Assert.Equal(2.5504d, value);
        Assert.Equal("2.5504", value.ToString(InvariantCulture));
    }
}