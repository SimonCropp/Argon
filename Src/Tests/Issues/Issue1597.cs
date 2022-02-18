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

using Xunit;

namespace Argon.Tests.Issues;

public class Issue1597 : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        var json = @"{
    ""wish"": 264,
    ""collect"": 7498,
    ""doing"": 385,
    ""on_hold"": 285,
    ""dropped"": 221
}";

        var o = JsonConvert.DeserializeObject<IReadOnlyDictionary<CollectionStatus, int>>(json);

        Xunit.Assert.Equal(264, o[CollectionStatus.Wish]);
        Xunit.Assert.Equal(7498, o[CollectionStatus.Collect]);
        Xunit.Assert.Equal(385, o[CollectionStatus.Doing]);
        Xunit.Assert.Equal(285, o[CollectionStatus.OnHold]);
        Xunit.Assert.Equal(221, o[CollectionStatus.Dropped]);
    }

    [Fact]
    public void Test_WithNumbers()
    {
        var json = @"{
    ""0"": 264,
    ""1"": 7498,
    ""2"": 385,
    ""3"": 285,
    ""4"": 221
}";

        var o = JsonConvert.DeserializeObject<IReadOnlyDictionary<CollectionStatus, int>>(json);

        Xunit.Assert.Equal(264, o[CollectionStatus.Wish]);
        Xunit.Assert.Equal(7498, o[CollectionStatus.Collect]);
        Xunit.Assert.Equal(385, o[CollectionStatus.Doing]);
        Xunit.Assert.Equal(285, o[CollectionStatus.OnHold]);
        Xunit.Assert.Equal(221, o[CollectionStatus.Dropped]);
    }

    [Fact]
    public void Test_Serialize()
    {
        var o = new Dictionary<CollectionStatus, int>
        {
            [CollectionStatus.Wish] = 264,
            [CollectionStatus.Collect] = 7498,
            [CollectionStatus.Doing] = 385,
            [CollectionStatus.OnHold] = 285,
            [CollectionStatus.Dropped] = 221,
            [(CollectionStatus) int.MaxValue] = int.MaxValue
        };

        var json = JsonConvert.SerializeObject(o, Formatting.Indented);

        StringAssert.AreEqual(@"{
  ""Wish"": 264,
  ""Collect"": 7498,
  ""Doing"": 385,
  ""on_hold"": 285,
  ""Dropped"": 221,
  ""2147483647"": 2147483647
}", json);
    }

    public enum CollectionStatus
    {
        Wish,
        Collect,
        Doing,
        [EnumMember(Value = "on_hold")]
        OnHold,
        Dropped
    }
}