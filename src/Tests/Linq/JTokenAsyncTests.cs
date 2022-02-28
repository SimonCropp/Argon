// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.


public class JTokenAsyncTests : TestFixtureBase
{
    [Fact]
    public async Task ReadFromAsync()
    {
        var o = (JObject)await JToken.ReadFromAsync(new JsonTextReader(new StringReader("{'pie':true}")));
        XUnitAssert.True((bool)o["pie"]);

        var a = (JArray)await JToken.ReadFromAsync(new JsonTextReader(new StringReader("[1,2,3]")));
        Assert.Equal(1, (int)a[0]);
        Assert.Equal(2, (int)a[1]);
        Assert.Equal(3, (int)a[2]);

        JsonReader reader = new JsonTextReader(new StringReader("{'pie':true}"));
        await reader.ReadAsync();
        await reader.ReadAsync();

        var p = (JProperty)await JToken.ReadFromAsync(reader);
        Assert.Equal("pie", p.Name);
        XUnitAssert.True((bool)p.Value);

        var v = (JValue)await JToken.ReadFromAsync(new JsonTextReader(new StringReader(@"""stringvalue""")));
        Assert.Equal("stringvalue", (string)v);

        v = (JValue)await JToken.ReadFromAsync(new JsonTextReader(new StringReader(@"1")));
        Assert.Equal(1, (int)v);

        v = (JValue)await JToken.ReadFromAsync(new JsonTextReader(new StringReader(@"1.1")));
        Assert.Equal(1.1, (double)v);

        v = (JValue)await JToken.ReadFromAsync(new JsonTextReader(new StringReader(@"""1970-01-01T00:00:00+12:31"""))
        {
            DateParseHandling = DateParseHandling.DateTimeOffset
        });
        Assert.Equal(typeof(DateTimeOffset), v.Value.GetType());
        Assert.Equal(new DateTimeOffset(ParseTests.InitialJavaScriptDateTicks, new TimeSpan(12, 31, 0)), v.Value);
    }

    [Fact]
    public async Task LoadAsync()
    {
        var o = (JObject)await JToken.LoadAsync(new JsonTextReader(new StringReader("{'pie':true}")));
        XUnitAssert.True((bool)o["pie"]);
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
        Assert.NotNull(writer);
        Assert.Equal(4, a.Count);

        await writer.WriteValueAsync("String");
        Assert.Equal(5, a.Count);
        Assert.Equal("String", (string)a[4]);

        await writer.WriteStartObjectAsync();
        await writer.WritePropertyNameAsync("Property");
        await writer.WriteValueAsync("PropertyValue");
        await writer.WriteEndAsync();

        Assert.Equal(6, a.Count);
        Assert.True(JToken.DeepEquals(new JObject(new JProperty("Property", "PropertyValue")), a[5]));
    }
}