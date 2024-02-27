// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue2165
{
    [Fact]
    public void Test_Deserializer()
    {
        var exception = Assert.Throws<JsonWriterException>(() => JsonConvert.DeserializeObject<JObject>("{"));
        Assert.Equal("Unexpected end when reading token. Path ''.", exception.Message);
    }

    [Fact]
    public void Test()
    {
        var w = new StringWriter();
        var writer = new JsonTextWriter(w);

        var jsonReader = new JsonTextReader(new StringReader("{"));
        jsonReader.Read();

        var exception = Assert.Throws<JsonWriterException>(() => writer.WriteToken(jsonReader));
        Assert.Equal("Unexpected end when reading token. Path ''.", exception.Message);
    }
}