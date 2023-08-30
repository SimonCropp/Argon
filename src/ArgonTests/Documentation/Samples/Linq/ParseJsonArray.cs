// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon.Tests.Documentation.Samples.Linq;

public class ParseJsonArray : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region ParseJsonArray

        var json = """
                   [
                     'Small',
                     'Medium',
                     'Large'
                   ]
                   """;

        var a = JArray.Parse(json);

        Console.WriteLine(a.ToString());
        // [
        //   "Small",
        //   "Medium",
        //   "Large"
        // ]

        #endregion

        XUnitAssert.AreEqualNormalized(
            """
            [
              "Small",
              "Medium",
              "Large"
            ]
            """,
            a.ToString());
    }
}