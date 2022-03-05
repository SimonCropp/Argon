// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.Collections.Immutable;

public class DeserializeImmutableCollections : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region DeserializeImmutableCollections

        var json = @"[
              'One',
              'II',
              '3'
            ]";

        var l = JsonConvert.DeserializeObject<ImmutableList<string>>(json);

        foreach (var s in l)
        {
            Console.WriteLine(s);
        }

        // One
        // II
        // 3

        #endregion

        Assert.Equal(3, l.Count);
        Assert.Equal("One", l[0]);
        Assert.Equal("II", l[1]);
        Assert.Equal("3", l[2]);
    }
}