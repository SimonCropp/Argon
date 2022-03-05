// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class JPropertyTests : TestFixtureBase
{
    [Fact]
    public void NullValue()
    {
        var p = new JProperty("TestProperty", null);
        Assert.NotNull(p.Value);
        Assert.Equal(JTokenType.Null, p.Value.Type);
        Assert.Equal(p, p.Value.Parent);

        p.Value = null;
        Assert.NotNull(p.Value);
        Assert.Equal(JTokenType.Null, p.Value.Type);
        Assert.Equal(p, p.Value.Parent);
    }

    [Fact]
    public void IListCount()
    {
        var p = new JProperty("TestProperty", null);
        Assert.Equal(1, p.Count);
    }

    [Fact]
    public void IListClear()
    {
        var p = (IList<JToken>) new JProperty("TestProperty", null);

        XUnitAssert.Throws<JsonException>(
            () => p.Clear(),
            "Cannot add or remove items from Argon.JProperty.");
    }

    [Fact]
    public void IListAdd()
    {
        var p = (IList<JToken>) new JProperty("TestProperty", null);

        XUnitAssert.Throws<JsonException>(
            () => p.Add(null),
            "Argon.JProperty cannot have multiple values.");
    }

    [Fact]
    public void IListRemoveAt()
    {
        var p = (IList<JToken>) new JProperty("TestProperty", null);

        XUnitAssert.Throws<JsonException>(
            () => p.RemoveAt(0),
            "Cannot add or remove items from Argon.JProperty.");
    }

    [Fact]
    public void JPropertyLinq()
    {
        var p = new JProperty("TestProperty", null);
        var result = p.ToList();
        Assert.Equal(1, result.Count);
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
        var p = (IList<JToken>) new JProperty("TestProperty", v);
        Assert.Equal(0, p.IndexOf(v));
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

        Assert.Equal(JsonToken.StartObject, reader.TokenType);
        reader.Read();

        var property = JProperty.Load(reader);
        Assert.Equal("propertyname", property.Name);
        Assert.True(JToken.DeepEquals(JArray.Parse("['value1']"), property.Value));

        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        reader = new JsonTextReader(new StringReader("{'propertyname':null}"));
        reader.Read();

        Assert.Equal(JsonToken.StartObject, reader.TokenType);
        reader.Read();

        property = JProperty.Load(reader);
        Assert.Equal("propertyname", property.Name);
        Assert.True(JToken.DeepEquals(JValue.CreateNull(), property.Value));

        Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public void MultiContentConstructor()
    {
        var p = new JProperty("error", new List<string> {"one", "two"});
        var a = (JArray) p.Value;

        Assert.Equal(a.Count, 2);
        Assert.Equal("one", (string) a[0]);
        Assert.Equal("two", (string) a[1]);
    }

    [Fact]
    public void IListGenericAdd()
    {
        IList<JToken> t = new JProperty("error", new List<string> {"one", "two"});

        XUnitAssert.Throws<JsonException>(
            () => t.Add(1),
            "Argon.JProperty cannot have multiple values.");
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

        var property = obj.PropertyOrNull("prop1");
        var value = property.Value;

        // remove value so it has no parent
        property.Value = null;

        property.Remove();
        obj.Add(new JProperty("prop2", value));

        Assert.Equal(((JProperty) value.Parent).Name, "prop2");
    }
}