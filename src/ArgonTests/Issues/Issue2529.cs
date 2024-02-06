// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue2529 : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        var something = JsonConvert.DeserializeObject<Something>("{ \"foo\": [] }");

        Assert.Equal(JTokenType.Array, something.Foo.Type);
    }

    class Something
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public JToken Foo { get; set; } = JValue.CreateNull();
    }
}