// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.


public class JConstructorAsyncTests : TestFixtureBase
{
    [Fact]
    public async Task LoadAsync()
    {
        JsonReader reader = new JsonTextReader(new StringReader("new Date(123)"));
        await reader.ReadAsync();

        var constructor = await JConstructor.LoadAsync(reader);
        Assert.Equal("Date", constructor.Name);
        Assert.True(JToken.DeepEquals(new JValue(123), constructor.Values().ElementAt(0)));
    }
}