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

using System.Dynamic;
using Xunit;
using Assert = Argon.Tests.XUnitAssert;

namespace Argon.Tests.Converters;

public class ExpandoObjectConverterTests : TestFixtureBase
{
    public class ExpandoContainer
    {
        public string Before { get; set; }
        public ExpandoObject Expando { get; set; }
        public string After { get; set; }
    }

    [Fact]
    public void SerializeExpandoObject()
    {
        var d = new ExpandoContainer
        {
            Before = "Before!",
            Expando = new ExpandoObject(),
            After = "After!"
        };

        dynamic o = d.Expando;

        o.String = "String!";
        o.Integer = 234;
        o.Float = 1.23d;
        o.List = new List<string> { "First", "Second", "Third" };
        o.Object = new Dictionary<string, object>
        {
            { "First", 1 }
        };

        var json = JsonConvert.SerializeObject(d, Formatting.Indented);

        StringAssert.AreEqual(@"{
  ""Before"": ""Before!"",
  ""Expando"": {
    ""String"": ""String!"",
    ""Integer"": 234,
    ""Float"": 1.23,
    ""List"": [
      ""First"",
      ""Second"",
      ""Third""
    ],
    ""Object"": {
      ""First"": 1
    }
  },
  ""After"": ""After!""
}", json);
    }

    [Fact]
    public void SerializeNullExpandoObject()
    {
        var d = new ExpandoContainer();

        var json = JsonConvert.SerializeObject(d, Formatting.Indented);

        StringAssert.AreEqual(@"{
  ""Before"": null,
  ""Expando"": null,
  ""After"": null
}", json);
    }

    [Fact]
    public void DeserializeExpandoObject()
    {
        var json = @"{
  ""Before"": ""Before!"",
  ""Expando"": {
    ""String"": ""String!"",
    ""Integer"": 234,
    ""Float"": 1.23,
    ""List"": [
      ""First"",
      ""Second"",
      ""Third""
    ],
    ""Object"": {
      ""First"": 1
    }
  },
  ""After"": ""After!""
}";

        var o = JsonConvert.DeserializeObject<ExpandoContainer>(json);

        Xunit.Assert.Equal(o.Before, "Before!");
        Xunit.Assert.Equal(o.After, "After!");
        Xunit.Assert.NotNull(o.Expando);

        dynamic d = o.Expando;
        Xunit.Assert.IsType(typeof(ExpandoObject), d);

        Xunit.Assert.Equal("String!", d.String);
        Xunit.Assert.IsType(typeof(string), d.String);

        Xunit.Assert.Equal(234, d.Integer);
        Xunit.Assert.IsType(typeof(long), d.Integer);

        Xunit.Assert.Equal(1.23, d.Float);
        Xunit.Assert.IsType(typeof(double), d.Float);

        Xunit.Assert.NotNull(d.List);
        Xunit.Assert.Equal(3, d.List.Count);
        Xunit.Assert.IsType(typeof(List<object>), d.List);

        Xunit.Assert.Equal("First", d.List[0]);
        Xunit.Assert.IsType(typeof(string), d.List[0]);

        Xunit.Assert.Equal("Second", d.List[1]);
        Xunit.Assert.Equal("Third", d.List[2]);

        Xunit.Assert.NotNull(d.Object);
        Xunit.Assert.IsType(typeof(ExpandoObject), d.Object);

        Xunit.Assert.Equal(1, d.Object.First);
        Xunit.Assert.IsType(typeof(long), d.Object.First);
    }

    [Fact]
    public void DeserializeNullExpandoObject()
    {
        var json = @"{
  ""Before"": null,
  ""Expando"": null,
  ""After"": null
}";

        var c = JsonConvert.DeserializeObject<ExpandoContainer>(json);

        Xunit.Assert.Equal(null, c.Expando);
    }
}