// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class CreateJsonManually : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region CreateJsonManually

        var array = new JArray
        {
            "Manual text",
            new DateTime(2000, 5, 23)
        };

        var o = new JObject
        {
            ["MyArray"] = array
        };

        var json = o.ToString();
        // {
        //   "MyArray": [
        //     "Manual text",
        //     "2000-05-23T00:00:00"
        //   ]
        // }

        #endregion
    }
}