// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon.Tests.Documentation.Samples.Linq;

public class ParseJsonAny : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region ParseJsonAny

        var t1 = JToken.Parse("{}");

        Console.WriteLine(t1.Type);
        // Object

        var t2 = JToken.Parse("[]");

        Console.WriteLine(t2.Type);
        // Array

        var t3 = JToken.Parse("null");

        Console.WriteLine(t3.Type);
        // Null

        var t4 = JToken.Parse("'A string!'");

        Console.WriteLine(t4.Type);
        // String

        #endregion

        Assert.Equal(JTokenType.String, t4.Type);
    }
}