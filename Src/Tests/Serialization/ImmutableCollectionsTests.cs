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

using System.Collections.Immutable;
using Xunit;

namespace Argon.Tests.Serialization;

public class ImmutableCollectionsTests : TestFixtureBase
{
    #region List
    [Fact]
    public void SerializeList()
    {
        var l = ImmutableList.CreateRange(new List<string>
        {
            "One",
            "II",
            "3"
        });

        var json = JsonConvert.SerializeObject(l, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"[
  ""One"",
  ""II"",
  ""3""
]", json);
    }

    [Fact]
    public void DeserializeList()
    {
        var json = @"[
  ""One"",
  ""II"",
  ""3""
]";

        var l = JsonConvert.DeserializeObject<ImmutableList<string>>(json);

        Assert.Equal(3, l.Count);
        Assert.Equal("One", l[0]);
        Assert.Equal("II", l[1]);
        Assert.Equal("3", l[2]);
    }

    [Fact]
    public void DeserializeListInterface()
    {
        var json = @"[
        'Volibear',
        'Teemo',
        'Katarina'
      ]";

        // what sorcery is this?!
        var champions = JsonConvert.DeserializeObject<IImmutableList<string>>(json);

        Assert.Equal(3, champions.Count);
        Assert.Equal("Volibear", champions[0]);
        Assert.Equal("Teemo", champions[1]);
        Assert.Equal("Katarina", champions[2]);
    }
    #endregion

    #region Array
    [Fact]
    public void SerializeArray()
    {
        var l = ImmutableArray.CreateRange(new List<string>
        {
            "One",
            "II",
            "3"
        });

        var json = JsonConvert.SerializeObject(l, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"[
  ""One"",
  ""II"",
  ""3""
]", json);
    }

    [Fact]
    public void DeserializeArray()
    {
        var json = @"[
          ""One"",
          ""II"",
          ""3""
        ]";

        var l = JsonConvert.DeserializeObject<ImmutableArray<string>>(json);

        Assert.Equal(3, l.Length);
        Assert.Equal("One", l[0]);
        Assert.Equal("II", l[1]);
        Assert.Equal("3", l[2]);
    }

    [Fact]
    public void SerializeDefaultArray()
    {
        XUnitAssert.Throws<InvalidOperationException>(
            () => JsonConvert.SerializeObject(default(ImmutableArray<int>), Formatting.Indented),
            "This operation cannot be performed on a default instance of ImmutableArray<T>.  Consider initializing the array, or checking the ImmutableArray<T>.IsDefault property.");
    }
    #endregion

    #region Queue
    [Fact]
    public void SerializeQueue()
    {
        var l = ImmutableQueue.CreateRange(new List<string>
        {
            "One",
            "II",
            "3"
        });

        var json = JsonConvert.SerializeObject(l, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"[
  ""One"",
  ""II"",
  ""3""
]", json);
    }

    [Fact]
    public void DeserializeQueue()
    {
        var json = @"[
  ""One"",
  ""II"",
  ""3""
]";

        var l = JsonConvert.DeserializeObject<ImmutableQueue<string>>(json);

        Assert.Equal(3, l.Count());
        Assert.Equal("One", l.ElementAt(0));
        Assert.Equal("II", l.ElementAt(1));
        Assert.Equal("3", l.ElementAt(2));
    }

    [Fact]
    public void DeserializeQueueInterface()
    {
        var json = @"[
  ""One"",
  ""II"",
  ""3""
]";

        var l = JsonConvert.DeserializeObject<IImmutableQueue<string>>(json);

        Assert.Equal(3, l.Count());
        Assert.Equal("One", l.ElementAt(0));
        Assert.Equal("II", l.ElementAt(1));
        Assert.Equal("3", l.ElementAt(2));
    }
    #endregion

    #region Stack
    [Fact]
    public void SerializeStack()
    {
        var l = ImmutableStack.CreateRange(new List<string>
        {
            "One",
            "II",
            "3"
        });

        var json = JsonConvert.SerializeObject(l, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"[
  ""3"",
  ""II"",
  ""One""
]", json);
    }

    [Fact]
    public void DeserializeStack()
    {
        var json = @"[
  ""One"",
  ""II"",
  ""3""
]";

        var l = JsonConvert.DeserializeObject<ImmutableStack<string>>(json);

        Assert.Equal(3, l.Count());
        Assert.Equal("3", l.ElementAt(0));
        Assert.Equal("II", l.ElementAt(1));
        Assert.Equal("One", l.ElementAt(2));
    }

    [Fact]
    public void DeserializeStackInterface()
    {
        var json = @"[
  ""One"",
  ""II"",
  ""3""
]";

        var l = JsonConvert.DeserializeObject<IImmutableStack<string>>(json);

        Assert.Equal(3, l.Count());
        Assert.Equal("3", l.ElementAt(0));
        Assert.Equal("II", l.ElementAt(1));
        Assert.Equal("One", l.ElementAt(2));
    }
    #endregion

    #region HashSet
    [Fact]
    public void SerializeHashSet()
    {
        var l = ImmutableHashSet.CreateRange(new List<string>
        {
            "One",
            "II",
            "3"
        });

        var json = JsonConvert.SerializeObject(l, Formatting.Indented);

        var a = JArray.Parse(json);
        Assert.Equal(3, a.Count);
        Assert.True(a.Any(t => t.DeepEquals("One")));
        Assert.True(a.Any(t => t.DeepEquals("II")));
        Assert.True(a.Any(t => t.DeepEquals("3")));
    }

    [Fact]
    public void DeserializeHashSet()
    {
        var json = @"[
  ""One"",
  ""II"",
  ""3""
]";

        var l = JsonConvert.DeserializeObject<ImmutableHashSet<string>>(json);

        Assert.Equal(3, l.Count());
        Assert.True(l.Contains("3"));
        Assert.True(l.Contains("II"));
        Assert.True(l.Contains("One"));
    }

    [Fact]
    public void DeserializeHashSetInterface()
    {
        var json = @"[
  ""One"",
  ""II"",
  ""3""
]";

        var l = JsonConvert.DeserializeObject<IImmutableSet<string>>(json);

        Assert.Equal(3, l.Count());
        Assert.True(l.Contains("3"));
        Assert.True(l.Contains("II"));
        Assert.True(l.Contains("One"));

        Assert.True(l is ImmutableHashSet<string>);
    }
    #endregion

    #region SortedSet
    [Fact]
    public void SerializeSortedSet()
    {
        var l = ImmutableSortedSet.CreateRange(new List<string>
        {
            "One",
            "II",
            "3"
        });

        var json = JsonConvert.SerializeObject(l, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"[
  ""3"",
  ""II"",
  ""One""
]", json);
    }

    [Fact]
    public void DeserializeSortedSet()
    {
        var json = @"[
  ""One"",
  ""II"",
  ""3""
]";

        var l = JsonConvert.DeserializeObject<ImmutableSortedSet<string>>(json);

        Assert.Equal(3, l.Count());
        Assert.True(l.Contains("3"));
        Assert.True(l.Contains("II"));
        Assert.True(l.Contains("One"));
    }
    #endregion

    #region Dictionary
    [Fact]
    public void SerializeDictionary()
    {
        var l = ImmutableDictionary.CreateRange(new Dictionary<int, string>
        {
            { 1, "One" },
            { 2, "II" },
            { 3, "3" }
        });

        var json = JsonConvert.SerializeObject(l, Formatting.Indented);
        var a = JObject.Parse(json);
        Assert.Equal(3, a.Count);
        Assert.Equal("One", (string)a["1"]);
        Assert.Equal("II", (string)a["2"]);
        Assert.Equal("3", (string)a["3"]);
    }

    [Fact]
    public void DeserializeDictionary()
    {
        var json = @"{
  ""1"": ""One"",
  ""2"": ""II"",
  ""3"": ""3""
}";

        var l = JsonConvert.DeserializeObject<ImmutableDictionary<int, string>>(json);

        Assert.Equal(3, l.Count);
        Assert.Equal("One", l[1]);
        Assert.Equal("II", l[2]);
        Assert.Equal("3", l[3]);
    }

    [Fact]
    public void DeserializeDictionaryInterface()
    {
        var json = @"{
  ""1"": ""One"",
  ""2"": ""II"",
  ""3"": ""3""
}";

        var l = JsonConvert.DeserializeObject<IImmutableDictionary<int, string>>(json);

        Assert.Equal(3, l.Count);
        Assert.Equal("One", l[1]);
        Assert.Equal("II", l[2]);
        Assert.Equal("3", l[3]);

        Assert.True(l is ImmutableDictionary<int, string>);
    }
    #endregion

    #region SortedDictionary
    [Fact]
    public void SerializeSortedDictionary()
    {
        var l = ImmutableSortedDictionary.CreateRange(new SortedDictionary<int, string>
        {
            { 1, "One" },
            { 2, "II" },
            { 3, "3" }
        });

        var json = JsonConvert.SerializeObject(l, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"{
  ""1"": ""One"",
  ""2"": ""II"",
  ""3"": ""3""
}", json);
    }

    [Fact]
    public void DeserializeSortedDictionary()
    {
        var json = @"{
  ""1"": ""One"",
  ""2"": ""II"",
  ""3"": ""3""
}";

        var l = JsonConvert.DeserializeObject<ImmutableSortedDictionary<int, string>>(json);

        Assert.Equal(3, l.Count);
        Assert.Equal("One", l[1]);
        Assert.Equal("II", l[2]);
        Assert.Equal("3", l[3]);
    }
    #endregion
}