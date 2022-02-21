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

using Microsoft.FSharp.Collections;

namespace Argon.Tests.Serialization;

public class FSharpTests : TestFixtureBase
{
    [Fact]
    public void List()
    {
        var l = ListModule.OfSeq(new List<int> { 1, 2, 3 });

        var json = JsonConvert.SerializeObject(l, Formatting.Indented);

        XUnitAssert.AreEqualNormalized(@"[
  1,
  2,
  3
]", json);

        var l2 = JsonConvert.DeserializeObject<FSharpList<int>>(json);

        Assert.Equal(l.Length, l2.Length);
        Assert.Equal(l, l2);
    }

    [Fact]
    public void Set()
    {
        var l = SetModule.OfSeq(new List<int> { 1, 2, 3 });

        var json = JsonConvert.SerializeObject(l, Formatting.Indented);

        XUnitAssert.AreEqualNormalized(@"[
  1,
  2,
  3
]", json);

        var l2 = JsonConvert.DeserializeObject<FSharpSet<int>>(json);

        Assert.Equal(l.Count, l2.Count);
        Assert.Equal(l, l2);
    }

    [Fact]
    public void Map()
    {
        var m1 = MapModule.OfSeq(new List<Tuple<string, int>> { Tuple.Create("one", 1), Tuple.Create("II", 2), Tuple.Create("3", 3) });

        var json = JsonConvert.SerializeObject(m1, Formatting.Indented);

        var m2 = JsonConvert.DeserializeObject<FSharpMap<string, int>>(json);

        Assert.Equal(m1.Count, m2.Count);
        Assert.Equal(1, m2["one"]);
        Assert.Equal(2, m2["II"]);
        Assert.Equal(3, m2["3"]);
    }
}