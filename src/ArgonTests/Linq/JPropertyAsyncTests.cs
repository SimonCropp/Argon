// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.


public class JPropertyAsyncTests : TestFixtureBase
{
    [Fact]
    public async Task LoadAsync()
    {
        var reader = new JsonTextReader(new StringReader("{'propertyname':['value1']}"));
        await reader.ReadAsync();

        Assert.Equal(JsonToken.StartObject, reader.TokenType);
        await reader.ReadAsync();

        var property = await JProperty.LoadAsync(reader);
        Assert.Equal("propertyname", property.Name);
        Assert.True(JToken.DeepEquals(JArray.Parse("['value1']"), property.Value));

        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        reader = new(new StringReader("{'propertyname':null}"));
        await reader.ReadAsync();

        Assert.Equal(JsonToken.StartObject, reader.TokenType);
        await reader.ReadAsync();

        property = await JProperty.LoadAsync(reader);
        Assert.Equal("propertyname", property.Name);
        Assert.True(JToken.DeepEquals(JValue.CreateNull(), property.Value));

        Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }
}