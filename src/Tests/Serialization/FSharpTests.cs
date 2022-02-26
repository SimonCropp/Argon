// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using Microsoft.FSharp.Collections;

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