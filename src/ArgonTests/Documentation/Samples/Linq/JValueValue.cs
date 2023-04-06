// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon.Tests.Documentation.Samples.Linq;

public class JValueValue : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region JValueValue

        var s = new JValue("A string value");

        Console.WriteLine(s.Value.GetType().Name);
        // String
        Console.WriteLine(s.Value);
        // A string value

        var u = new JValue(new Uri("http://www.google.com/"));

        Console.WriteLine(u.Value.GetType().Name);
        // Uri
        Console.WriteLine(u.Value);
        // http://www.google.com/

        #endregion

        Assert.Equal(new Uri("http://www.google.com/"), u.Value);
    }
}