﻿#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using Xunit;

namespace Argon.Tests.Documentation.Samples.Linq;

public class FromObject : TestFixtureBase
{
    #region FromObjectTypes
    public class Computer
    {
        public string Cpu { get; set; }
        public int Memory { get; set; }
        public IList<string> Drives { get; set; }
    }
    #endregion

    [Fact]
    public void Example()
    {
        #region FromObjectUsage
        var i = (JValue)JToken.FromObject(12345);

        Console.WriteLine(i.Type);
        // Integer
        Console.WriteLine(i.ToString());
        // 12345

        var s = (JValue)JToken.FromObject("A string");

        Console.WriteLine(s.Type);
        // String
        Console.WriteLine(s.ToString());
        // A string

        var computer = new Computer
        {
            Cpu = "Intel",
            Memory = 32,
            Drives = new List<string>
            {
                "DVD",
                "SSD"
            }
        };

        var o = (JObject)JToken.FromObject(computer);

        Console.WriteLine(o.ToString());
        // {
        //   "Cpu": "Intel",
        //   "Memory": 32,
        //   "Drives": [
        //     "DVD",
        //     "SSD"
        //   ]
        // }

        var a = (JArray)JToken.FromObject(computer.Drives);

        Console.WriteLine(a.ToString());
        // [
        //   "DVD",
        //   "SSD"
        // ]
        #endregion

        XUnitAssert.AreEqualNormalized(@"[
  ""DVD"",
  ""SSD""
]", a.ToString());
    }
}