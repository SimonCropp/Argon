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

using Argon.Linq;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Argon.Tests.Documentation.Samples.JsonPath
{
    [TestFixture]
    public class RegexQuery : TestFixtureBase
    {
        [Fact]
        public void Example()
        {
            #region Usage
            JArray array = JArray.Parse(@"[
              {
                'PackageId': 'Argon',
                'Version': '11.0.1',
                'ReleaseDate': '2018-02-17T00:00:00'
              },
              {
                'PackageId': 'NUnit',
                'Version': '3.9.0',
                'ReleaseDate': '2017-11-10T00:00:00'
              }
            ]");

            // Find packages
            List<JToken> packages = array.SelectTokens(@"$.[?(@.PackageId =~ /^Argon/)]").ToList();

            foreach (JToken item in packages)
            {
                Console.WriteLine((string) item["PackageId"]);
            }
            // Argon
            #endregion

            Assert.AreEqual(1, packages.Count);
            Assert.AreEqual("Argon", (string)packages[0]["PackageId"]);
        }
    }
}