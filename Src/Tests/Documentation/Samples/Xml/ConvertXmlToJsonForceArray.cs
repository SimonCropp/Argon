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

using System.Xml;
using Xunit;

namespace Argon.Tests.Documentation.Samples.Xml;

public class ConvertXmlToJsonForceArray : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region Usage
        var xml = @"<person id='1'>
              <name>Alan</name>
              <url>http://www.google.com</url>
              <role>Admin1</role>
            </person>";

        var doc = new XmlDocument();
        doc.LoadXml(xml);

        var json = JsonConvert.SerializeXmlNode(doc);

        Console.WriteLine(json);
        // {
        //   "person": {
        //     "@id": "1",
        //     "name": "Alan",
        //     "url": "http://www.google.com",
        //     "role": "Admin1"
        //   }
        // }

        xml = @"<person xmlns:json='http://james.newtonking.com/projects/json' id='1'>
              <name>Alan</name>
              <url>http://www.google.com</url>
              <role json:Array='true'>Admin</role>
            </person>";

        doc = new XmlDocument();
        doc.LoadXml(xml);

        json = JsonConvert.SerializeXmlNode(doc);

        Console.WriteLine(json);
        // {
        //   "person": {
        //     "@id": "1",
        //     "name": "Alan",
        //     "url": "http://www.google.com",
        //     "role": [
        //       "Admin"
        //     ]
        //   }
        // }
        #endregion

        Assert.Equal(@"{""person"":{""@id"":""1"",""name"":""Alan"",""url"":""http://www.google.com"",""role"":[""Admin""]}}", json);
    }
}