// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class ToObjectType : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region ToObjectType

        var v1 = new JValue(true);

        var b = (bool) v1.ToObject(typeof(bool));

        Console.WriteLine(b);
        // true

        var i = (int) v1.ToObject(typeof(int));

        Console.WriteLine(i);
        // 1

        var s = (string) v1.ToObject(typeof(string));

        Console.WriteLine(s);
        // "True"

        #endregion

        Assert.Equal("True", s);
    }
}