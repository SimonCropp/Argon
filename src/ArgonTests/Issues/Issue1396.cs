// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1396 : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        using var stringReader = new StringReader(",");
        using var jsonReader = new JsonTextReader(stringReader);
        jsonReader.SupportMultipleContent = true;
        Assert.True(jsonReader.Read());
        Assert.Equal(JsonToken.Undefined, jsonReader.TokenType);
        Assert.False(jsonReader.Read());
    }
}