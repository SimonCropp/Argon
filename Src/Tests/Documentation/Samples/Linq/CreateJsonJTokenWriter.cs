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

public class CreateJsonJTokenWriter : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region Usage
        var writer = new JTokenWriter();
        writer.WriteStartObject();
        writer.WritePropertyName("name1");
        writer.WriteValue("value1");
        writer.WritePropertyName("name2");
        writer.WriteStartArray();
        writer.WriteValue(1);
        writer.WriteValue(2);
        writer.WriteEndArray();
        writer.WriteEndObject();

        var o = (JObject)writer.Token;

        Console.WriteLine(o.ToString());
        // {
        //   "name1": "value1",
        //   "name2": [
        //     1,
        //     2
        //   ]
        // }
        #endregion

        XUnitAssert.AreEqualNormalized(@"{
  ""name1"": ""value1"",
  ""name2"": [
    1,
    2
  ]
}", o.ToString());
    }
}