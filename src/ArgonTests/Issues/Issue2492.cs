// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue2492
{
    [Fact]
    public void Test_Object()
    {
        var jsontext = @"{ ""ABC"": //DEF
{}}";

        using var stringReader = new StringReader(jsontext);
        using var jsonReader = new JsonTextReader(stringReader);

        var serializer = JsonSerializer.Create();
        var x = serializer.Deserialize<JToken>(jsonReader);

        Assert.Equal(JTokenType.Object, x["ABC"].Type);
    }

    [Fact]
    public void Test_Integer()
    {
        var jsontext = "{ \"ABC\": /*DEF*/ 1}";

        using var stringReader = new StringReader(jsontext);
        using var jsonReader = new JsonTextReader(stringReader);

        var serializer = JsonSerializer.Create();
        var x = serializer.Deserialize<JToken>(jsonReader);

        Assert.Equal(JTokenType.Integer, x["ABC"].Type);
    }
}