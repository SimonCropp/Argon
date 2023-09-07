// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1561 : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        var data = new Data
        {
            Value = 1.1m
        };

        var serialized = JsonConvert.SerializeObject(data);

        Assert.Equal("""{"Value":1.1}""", serialized);
    }

    public class Data
    {
        public decimal? Value { get; set; }
    }
}