// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class CreateJsonCollectionInitializer : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region CreateJsonCollectionInitializer

        var o = new JObject
        {
            {"Cpu", "Intel"},
            {"Memory", 32},
            {
                "Drives", new JArray
                {
                    "DVD",
                    "SSD"
                }
            }
        };

        Console.WriteLine(o.ToString());
        // {
        //   "Cpu": "Intel",
        //   "Memory": 32,
        //   "Drives": [
        //     "DVD",
        //     "SSD"
        //   ]
        // }

        #endregion

        XUnitAssert.AreEqualNormalized(
            """
            {
              "Cpu": "Intel",
              "Memory": 32,
              "Drives": [
                "DVD",
                "SSD"
              ]
            }
            """,
            o.ToString());
    }
}