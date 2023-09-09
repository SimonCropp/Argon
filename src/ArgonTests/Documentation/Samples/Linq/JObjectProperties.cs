// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class JObjectProperties : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region JObjectProperties

        var o = new JObject
        {
            {"name1", "value1"},
            {"name2", "value2"}
        };

        foreach (var property in o.Properties())
        {
            Console.WriteLine($"{property.Name} - {property.Value}");
        }
        // name1 - value1
        // name2 - value2

        foreach (var property in o)
        {
            Console.WriteLine($"{property.Key} - {property.Value}");
        }

        // name1 - value1
        // name2 - value2

        #endregion

        Assert.Equal(2, o.Count);
    }
}