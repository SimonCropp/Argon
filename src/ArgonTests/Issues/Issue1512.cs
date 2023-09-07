// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.Collections.Immutable;

public class Issue1512 : TestFixtureBase
{
    [Fact]
    public void Test_Constructor()
    {
        var json = """
                   [
                     {
                       "Inners": ["hi","bye"]
                     }
                   ]
                   """;
        var result = JsonConvert.DeserializeObject<ImmutableArray<Outer>>(json);

        Assert.Equal(1, result.Length);
        Assert.Equal(2, result[0].Inners.Value.Length);
        Assert.Equal("hi", result[0].Inners.Value[0]);
        Assert.Equal("bye", result[0].Inners.Value[1]);
    }

    [Fact]
    public void Test_Property()
    {
        var json = """
                   [
                     {
                       "Inners": ["hi","bye"]
                     }
                   ]
                   """;
        var result = JsonConvert.DeserializeObject<ImmutableArray<OuterProperty>>(json);

        Assert.Equal(1, result.Length);
        Assert.Equal(2, result[0].Inners.Value.Length);
        Assert.Equal("hi", result[0].Inners.Value[0]);
        Assert.Equal("bye", result[0].Inners.Value[1]);
    }
}

public sealed class Outer
{
    public Outer(ImmutableArray<string>? inners) =>
        Inners = inners;

    public ImmutableArray<string>? Inners { get; }
}

public sealed class OuterProperty
{
    public ImmutableArray<string>? Inners { get; set; }
}