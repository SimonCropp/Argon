// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using TestObjects;

public class JObjectAsyncTests : TestFixtureBase
{
    [Fact]
    public async Task ReadWithSupportMultipleContentAsync()
    {
        var json = @"{ 'name': 'Admin' }{ 'name': 'Publisher' }";

        var roles = new List<JObject>();

        var reader = new JsonTextReader(new StringReader(json));
        reader.SupportMultipleContent = true;

        while (true)
        {
            var role = (JObject) await JToken.ReadFromAsync(reader);

            roles.Add(role);

            if (!await reader.ReadAsync())
            {
                break;
            }
        }

        Assert.Equal(2, roles.Count);
        Assert.Equal("Admin", (string) roles[0]["name"]);
        Assert.Equal("Publisher", (string) roles[1]["name"]);
    }

    [Fact]
    public async Task JTokenReaderAsync()
    {
        var raw = new PersonRaw
        {
            FirstName = "FirstNameValue",
            RawContent = new("[1,2,3,4,5]"),
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
        var jsonText = """
            {
              "short":
              {
                "error":
                {
                  "code":0,
                  "msg":"No action taken"
                }
              }
            }
            """;

        var reader = new JsonTextReader(new StringReader(jsonText));
        await reader.ReadAsync();
        await reader.ReadAsync();
        await reader.ReadAsync();
        await reader.ReadAsync();
        await reader.ReadAsync();

        var o = (JObject) await JToken.ReadFromAsync(reader);
        Assert.NotNull(o);
        XUnitAssert.AreEqualNormalized(@"{
  ""code"": 0,
  ""msg"": ""No action taken""
}", o.ToString(Formatting.Indented));
    }

    [Fact]
    public async Task LoadFromNestedObjectIncompleteAsync() =>
        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            async () =>
            {
                var jsonText = @"{
  ""short"":
  {
    ""error"":
    {
      ""code"":0";

                var reader = new JsonTextReader(new StringReader(jsonText));
                await reader.ReadAsync();
                await reader.ReadAsync();
                await reader.ReadAsync();
                await reader.ReadAsync();
                await reader.ReadAsync();

                await JToken.ReadFromAsync(reader);
            },
            "Unexpected end of content while loading JObject. Path 'short.error.code', line 6, position 14.");
}