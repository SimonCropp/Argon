// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1874
{
    [Fact]
    public void Test()
    {
        var something = new Something();

        JsonConvert.PopulateObject(@"{""Foo"": 1, ""Bar"": 2}", something);

        Assert.Equal(1, something.Extra.Count);
        Assert.Equal(2, (int)something.Extra["Bar"]);

        JsonConvert.PopulateObject(@"{""Foo"": 2, ""Bar"": 3}", something);

        Assert.Equal(1, something.Extra.Count);
        Assert.Equal(3, (int)something.Extra["Bar"]);
    }

    public class Something
    {
        public int Foo { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> Extra { get; set; }
    }
}