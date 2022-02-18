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

using System.ComponentModel;
using Xunit;
using Assert = Argon.Tests.XUnitAssert;

namespace Argon.Tests.Linq;

public class JPropertyTests : TestFixtureBase
{
    [Fact]
    public void NullValue()
    {
        var p = new JProperty("TestProperty", null);
        Xunit.Assert.NotNull(p.Value);
        Xunit.Assert.Equal(JTokenType.Null, p.Value.Type);
        Xunit.Assert.Equal(p, p.Value.Parent);

        p.Value = null;
        Xunit.Assert.NotNull(p.Value);
        Xunit.Assert.Equal(JTokenType.Null, p.Value.Type);
        Xunit.Assert.Equal(p, p.Value.Parent);
    }

    [Fact]
    public void ListChanged()
    {
        var p = new JProperty("TestProperty", null);
        IBindingList l = p;

        ListChangedType? listChangedType = null;
        int? index = null;

        l.ListChanged += (_, args) =>
        {
            listChangedType = args.ListChangedType;
            index = args.NewIndex;
        };

        p.Value = 1;

        Xunit.Assert.Equal(ListChangedType.ItemChanged, listChangedType.Value);
        Xunit.Assert.Equal(0, index.Value);
    }

    [Fact]
    public void IListCount()
    {
        var p = new JProperty("TestProperty", null);
        IList l = p;

        Xunit.Assert.Equal(1, l.Count);
    }

    [Fact]
    public void IListClear()
    {
        var p = new JProperty("TestProperty", null);
        IList l = p;

        ExceptionAssert.Throws<JsonException>(() => { l.Clear(); }, "Cannot add or remove items from Argon.Linq.JProperty.");
    }

    [Fact]
    public void IListAdd()
    {
        var p = new JProperty("TestProperty", null);
        IList l = p;

        ExceptionAssert.Throws<JsonException>(() => { l.Add(null); }, "Argon.Linq.JProperty cannot have multiple values.");
    }

    [Fact]
    public void IListRemove()
    {
        var p = new JProperty("TestProperty", null);
        IList l = p;

        ExceptionAssert.Throws<JsonException>(() => { l.Remove(p.Value); }, "Cannot add or remove items from Argon.Linq.JProperty.");
    }

    [Fact]
    public void IListRemoveAt()
    {
        var p = new JProperty("TestProperty", null);
        IList l = p;

        ExceptionAssert.Throws<JsonException>(() => { l.RemoveAt(0); }, "Cannot add or remove items from Argon.Linq.JProperty.");
    }

    [Fact]
    public void JPropertyLinq()
    {
        var p = new JProperty("TestProperty", null);
        IList l = p;

        var result = l.Cast<JToken>().ToList();
        Xunit.Assert.Equal(1, result.Count);
    }

    [Fact]
    public void JPropertyDeepEquals()
    {
        var p1 = new JProperty("TestProperty", null);
        var p2 = new JProperty("TestProperty", null);

        XUnitAssert.True(JToken.DeepEquals(p1, p2));
    }

    [Fact]
    public void JPropertyIndexOf()
    {
        var v = new JValue(1);
        var p1 = new JProperty("TestProperty", v);

        IList l1 = p1;
        Xunit.Assert.Equal(0, l1.IndexOf(v));

        IList<JToken> l2 = p1;
        Xunit.Assert.Equal(0, l2.IndexOf(v));
    }

    [Fact]
    public void JPropertyContains()
    {
        var v = new JValue(1);
        var p = new JProperty("TestProperty", v);

        XUnitAssert.True(p.Contains(v));
        XUnitAssert.False(p.Contains(new JValue(1)));
    }

    [Fact]
    public void Load()
    {
        JsonReader reader = new JsonTextReader(new StringReader("{'propertyname':['value1']}"));
        reader.Read();

        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);
        reader.Read();

        var property = JProperty.Load(reader);
        Xunit.Assert.Equal("propertyname", property.Name);
        Xunit.Assert.True(JToken.DeepEquals(JArray.Parse("['value1']"), property.Value));

        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);

        reader = new JsonTextReader(new StringReader("{'propertyname':null}"));
        reader.Read();

        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);
        reader.Read();

        property = JProperty.Load(reader);
        Xunit.Assert.Equal("propertyname", property.Name);
        Xunit.Assert.True(JToken.DeepEquals(JValue.CreateNull(), property.Value));

        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public void MultiContentConstructor()
    {
        var p = new JProperty("error", new List<string> { "one", "two" });
        var a = (JArray)p.Value;

        Xunit.Assert.Equal(a.Count, 2);
        Xunit.Assert.Equal("one", (string)a[0]);
        Xunit.Assert.Equal("two", (string)a[1]);
    }

    [Fact]
    public void IListGenericAdd()
    {
        IList<JToken> t = new JProperty("error", new List<string> { "one", "two" });

        ExceptionAssert.Throws<JsonException>(() => { t.Add(1); }, "Argon.Linq.JProperty cannot have multiple values.");
    }

    [Fact]
    public void NullParent()
    {
        var json = @"{
                ""prop1"": {
                    ""foo"": ""bar""
                },
            }";

        var obj = JsonConvert.DeserializeObject<JObject>(json);

        var property = obj.Property("prop1");
        var value = property.Value;

        // remove value so it has no parent
        property.Value = null;

        property.Remove();
        obj.Add(new JProperty("prop2", value));

        Xunit.Assert.Equal(((JProperty)value.Parent).Name, "prop2");
    }
}