// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon.Tests.Documentation.Samples.Linq;

public class Clone : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region Clone

        var o1 = new JObject
        {
            {"String", "A string!"},
            {"Items", new JArray(1, 2)}
        };

        Console.WriteLine(o1.ToString());
        // {
        //   "String": "A string!",
        //   "Items": [
        //     1,
        //     2
        //   ]
        // }

        var o2 = (JObject) o1.DeepClone();

        Console.WriteLine(o2.ToString());
        // {
        //   "String": "A string!",
        //   "Items": [
        //     1,
        //     2
        //   ]
        // }

        Console.WriteLine(JToken.DeepEquals(o1, o2));
        // true

        Console.WriteLine(ReferenceEquals(o1, o2));
        // false

        #endregion

        Assert.True(JToken.DeepEquals(o1, o2));
        Assert.False(ReferenceEquals(o1, o2));
    }
}