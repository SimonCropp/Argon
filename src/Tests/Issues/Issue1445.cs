#region License
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

using System.Data;

namespace Argon.Tests.Issues;

public class Issue1445 : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        var dt = new DataTable();
        dt.Columns.Add("First", typeof(string));
        dt.Columns.Add("Second", typeof(string));

        dt.Rows.Add("string1", "string2");
        dt.Rows.Add("string1", null);

        var data = dt.Select().Select(r => r.ItemArray).ToArray();

        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"[
  [
    ""string1"",
    ""string2""
  ],
  [
    ""string1"",
    null
  ]
]", json);
    }
}