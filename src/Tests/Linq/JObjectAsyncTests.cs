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

using TestObjects;

namespace Argon.Tests.Linq;

public class JObjectAsyncTests : TestFixtureBase
{
    [Fact]
    public async Task ReadWithSupportMultipleContentAsync()
    {
        var json = @"{ 'name': 'Admin' }{ 'name': 'Publisher' }";

        IList<JObject> roles = new List<JObject>();

        var reader = new JsonTextReader(new StringReader(json));
        reader.SupportMultipleContent = true;

        while (true)
        {
            var role = (JObject)await JToken.ReadFromAsync(reader);

            roles.Add(role);

            if (!await reader.ReadAsync())
            {
                break;
            }
        }

        Assert.Equal(2, roles.Count);
        Assert.Equal("Admin", (string)roles[0]["name"]);
        Assert.Equal("Publisher", (string)roles[1]["name"]);
    }

    [Fact]
    public async Task JTokenReaderAsync()
    {
        var raw = new PersonRaw
        {
            FirstName = "FirstNameValue",
            RawContent = new JRaw("[1,2,3,4,5]"),
            LastName = "LastNameValue"
        };

        var o = JObject.FromObject(raw);

        JsonReader reader = new JTokenReader(o);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Raw, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task LoadFromNestedObjectAsync()
    {
        var jsonText = @"{
  ""short"":
  {
    ""error"":
    {
      ""code"":0,
      ""msg"":""No action taken""
    }
  }
}";

        JsonReader reader = new JsonTextReader(new StringReader(jsonText));
        await reader.ReadAsync();
        await reader.ReadAsync();
        await reader.ReadAsync();
        await reader.ReadAsync();
        await reader.ReadAsync();

        var o = (JObject)await JToken.ReadFromAsync(reader);
        Assert.NotNull(o);
        XUnitAssert.AreEqualNormalized(@"{
  ""code"": 0,
  ""msg"": ""No action taken""
}", o.ToString(Formatting.Indented));
    }

    [Fact]
    public async Task LoadFromNestedObjectIncompleteAsync()
    {
        await XUnitAssert.ThrowsAsync<JsonReaderException>(async () =>
        {
            var jsonText = @"{
  ""short"":
  {
    ""error"":
    {
      ""code"":0";

            JsonReader reader = new JsonTextReader(new StringReader(jsonText));
            await reader.ReadAsync();
            await reader.ReadAsync();
            await reader.ReadAsync();
            await reader.ReadAsync();
            await reader.ReadAsync();

            await JToken.ReadFromAsync(reader);
        }, "Unexpected end of content while loading JObject. Path 'short.error.code', line 6, position 14.");
    }

    [Fact]
    public async Task ParseMultipleProperties_EmptySettingsAsync()
    {
        var json = @"{
        ""Name"": ""Name1"",
        ""Name"": ""Name2""
      }";

        var reader = new JsonTextReader(new StringReader(json));
        var o = (JObject)await JToken.ReadFromAsync(reader, new JsonLoadSettings());
        var value = (string)o["Name"];

        Assert.Equal("Name2", value);
    }

    [Fact]
    public async Task ParseMultipleProperties_IgnoreDuplicateSettingAsync()
    {
        var json = @"{
        ""Name"": ""Name1"",
        ""Name"": ""Name2""
      }";

        var reader = new JsonTextReader(new StringReader(json));
        var o = (JObject)await JToken.ReadFromAsync(reader, new JsonLoadSettings
        {
            DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Ignore
        });
        var value = (string)o["Name"];

        Assert.Equal("Name1", value);
    }
}