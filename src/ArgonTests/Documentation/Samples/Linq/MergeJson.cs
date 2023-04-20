// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon.Tests.Documentation.Samples.Linq;

public class MergeJson : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region MergeJson

        var o1 = JObject.Parse(
            """
            {
              'FirstName': 'John',
              'LastName': 'Smith',
              'Enabled': false,
              'Roles': [ 'User' ]
            }
            """);
        var o2 = JObject.Parse(
            """
            {
              'Enabled': true,
              'Roles': [ 'User', 'Admin' ]
            }
            """);

        o1.Merge(o2, new()
        {
            // union array values together to avoid duplicates
            MergeArrayHandling = MergeArrayHandling.Union
        });

        var json = o1.ToString();
        // {
        //   "FirstName": "John",
        //   "LastName": "Smith",
        //   "Enabled": true,
        //   "Roles": [
        //     "User",
        //     "Admin"
        //   ]
        // }

        #endregion

        XUnitAssert.AreEqualNormalized(
            """
            {
              "FirstName": "John",
              "LastName": "Smith",
              "Enabled": true,
              "Roles": [
                "User",
                "Admin"
              ]
            }
            """,
            json);
    }
}