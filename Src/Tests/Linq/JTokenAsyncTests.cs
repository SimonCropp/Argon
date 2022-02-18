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
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;

namespace Argon.Tests.Linq;

[TestFixture]
public class JTokenAsyncTests : TestFixtureBase
{
    [Fact]
    public async Task ReadFromAsync()
    {
        var o = (JObject)await JToken.ReadFromAsync(new JsonTextReader(new StringReader("{'pie':true}")));
        Assert.True( (bool)o["pie"]);

        var a = (JArray)await JToken.ReadFromAsync(new JsonTextReader(new StringReader("[1,2,3]")));
        Assert.AreEqual(1, (int)a[0]);
        Assert.AreEqual(2, (int)a[1]);
        Assert.AreEqual(3, (int)a[2]);

        JsonReader reader = new JsonTextReader(new StringReader("{'pie':true}"));
        await reader.ReadAsync();
        await reader.ReadAsync();

        var p = (JProperty)await JToken.ReadFromAsync(reader);
        Assert.AreEqual("pie", p.Name);
        Assert.True( (bool)p.Value);

        var c = (JConstructor)await JToken.ReadFromAsync(new JsonTextReader(new StringReader("new Date(1)")));
        Assert.AreEqual("Date", c.Name);
        Assert.IsTrue(JToken.DeepEquals(new JValue(1), c.Values().ElementAt(0)));

        var v = (JValue)await JToken.ReadFromAsync(new JsonTextReader(new StringReader(@"""stringvalue""")));
        Assert.AreEqual("stringvalue", (string)v);

        v = (JValue)await JToken.ReadFromAsync(new JsonTextReader(new StringReader(@"1")));
        Assert.AreEqual(1, (int)v);

        v = (JValue)await JToken.ReadFromAsync(new JsonTextReader(new StringReader(@"1.1")));
        Assert.AreEqual(1.1, (double)v);

        v = (JValue)await JToken.ReadFromAsync(new JsonTextReader(new StringReader(@"""1970-01-01T00:00:00+12:31"""))
        {
            DateParseHandling = DateParseHandling.DateTimeOffset
        });
        Assert.AreEqual(typeof(DateTimeOffset), v.Value.GetType());
        Assert.AreEqual(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, new TimeSpan(12, 31, 0)), v.Value);
    }

    [Fact]
    public async Task LoadAsync()
    {
        var o = (JObject)await JToken.LoadAsync(new JsonTextReader(new StringReader("{'pie':true}")));
        Assert.True( (bool)o["pie"]);
    }

    [Fact]
    public async Task CreateWriterAsync()
    {
        var a =
            new JArray(
                5,
                new JArray(1),
                new JArray(1, 2),
                new JArray(1, 2, 3)
            );

        var writer = a.CreateWriter();
        Assert.IsNotNull(writer);
        Assert.AreEqual(4, a.Count);

        await writer.WriteValueAsync("String");
        Assert.AreEqual(5, a.Count);
        Assert.AreEqual("String", (string)a[4]);

        await writer.WriteStartObjectAsync();
        await writer.WritePropertyNameAsync("Property");
        await writer.WriteValueAsync("PropertyValue");
        await writer.WriteEndAsync();

        Assert.AreEqual(6, a.Count);
        Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("Property", "PropertyValue")), a[5]));
    }
}