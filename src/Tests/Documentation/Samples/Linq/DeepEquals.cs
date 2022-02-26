// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class DeepEquals : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region DeepEquals
        var s1 = new JValue("A string");
        var s2 = new JValue("A string");
        var s3 = new JValue("A STRING");

        Console.WriteLine(JToken.DeepEquals(s1, s2));
        // true

        Console.WriteLine(JToken.DeepEquals(s2, s3));
        // false

        var o1 = new JObject
        {
            { "Integer", 12345 },
            { "String", "A string" },
            { "Items", new JArray(1, 2) }
        };

        var o2 = new JObject
        {
            { "Integer", 12345 },
            { "String", "A string" },
            { "Items", new JArray(1, 2) }
        };

        Console.WriteLine(JToken.DeepEquals(o1, o2));
        // true

        Console.WriteLine(JToken.DeepEquals(s1, o1["String"]));
        // true
        #endregion

        Assert.True(JToken.DeepEquals(o1, o2));
        Assert.True(JToken.DeepEquals(s1, o1["String"]));
    }
}