// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon.Tests.Documentation.Samples.Linq;

public class JValueCast : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region JValueCast

        var v1 = new JValue("1");
        var i = (int) v1;

        Console.WriteLine(i);
        // 1

        var v2 = new JValue(true);
        var b = (bool) v2;

        Console.WriteLine(b);
        // true

        var v3 = new JValue("19.95");
        var d = (decimal) v3;

        Console.WriteLine(d);
        // 19.95

        var v4 = new JValue(new DateTime(2013, 1, 21));
        var s = (string) v4;

        Console.WriteLine(s);
        // 01/21/2013 00:00:00

        var v5 = new JValue("http://www.bing.com");
        var u = (Uri) v5;

        Console.WriteLine(u);
        // http://www.bing.com/

        var v6 = JValue.CreateNull();
        u = (Uri) v6;

        Console.WriteLine(u != null ? u.ToString() : "{null}");
        // {null}

        var dt = (DateTime?) v6;

        Console.WriteLine(dt != null ? dt.ToString() : "{null}");
        // {null}

        #endregion

        Assert.Equal("01/21/2013 00:00:00", s);
    }
}