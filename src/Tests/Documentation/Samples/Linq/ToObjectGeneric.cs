// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon.Tests.Documentation.Samples.Linq;

public class ToObjectGeneric : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region ToObjectGeneric

        var v1 = new JValue(true);

        var b = v1.ToObject<bool>();

        Console.WriteLine(b);
        // true

        var i = v1.ToObject<int>();

        Console.WriteLine(i);
        // 1

        var s = v1.ToObject<string>();

        Console.WriteLine(s);
        // "True"

        #endregion

        Assert.Equal("True", s);
    }
}