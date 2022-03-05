// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon.Tests.Documentation.Samples.Linq;

public class ParseJsonObject : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region ParseJsonObject

        var json = @"{
              CPU: 'Intel',
              Drives: [
                'DVD read/writer',
                '500 gigabyte hard drive'
              ]
            }";

        var o = JObject.Parse(json);

        Console.WriteLine(o.ToString());
        // {
        //   "CPU": "Intel",
        //   "Drives": [
        //     "DVD read/writer",
        //     "500 gigabyte hard drive"
        //   ]
        // }

        #endregion

        XUnitAssert.AreEqualNormalized(@"{
  ""CPU"": ""Intel"",
  ""Drives"": [
    ""DVD read/writer"",
    ""500 gigabyte hard drive""
  ]
}", o.ToString());
    }
}