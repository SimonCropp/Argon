// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue2484
{
    [Fact]
    public void Test()
    {
        var json = "[]";
        var ex = XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject(json, typeof(JObject)));
        Assert.Equal("Deserialized JSON type 'Argon.JArray' is not compatible with expected type 'Argon.JObject'. Path '', line 1, position 2.", ex.Message);
    }
}