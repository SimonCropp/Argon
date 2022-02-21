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

namespace Argon.Tests.Documentation.Samples.Linq;

public class MergeJson : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region MergeJson
        var o1 = JObject.Parse(@"{
              'FirstName': 'John',
              'LastName': 'Smith',
              'Enabled': false,
              'Roles': [ 'User' ]
            }");
        var o2 = JObject.Parse(@"{
              'Enabled': true,
              'Roles': [ 'User', 'Admin' ]
            }");

        o1.Merge(o2, new JsonMergeSettings
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

        XUnitAssert.AreEqualNormalized(@"{
  ""FirstName"": ""John"",
  ""LastName"": ""Smith"",
  ""Enabled"": true,
  ""Roles"": [
    ""User"",
    ""Admin""
  ]
}", json);
    }
}