// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue1321 : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            MaxDepth = 1024
        };
        var exception = Assert.Throws<JsonWriterException>(() => JsonConvert.DeserializeObject("""["1",""", settings));
        Assert.Equal("Unexpected end when reading token. Path ''.", exception.Message);
    }

    [Fact]
    public void Test2()
    {
        var a = new JArray();

        var writer = a.CreateWriter();

        var reader = new JsonTextReader(new StringReader("""["1","""));

        var exception = Assert.Throws<JsonWriterException>(() => writer.WriteToken(reader));
        Assert.Equal("Unexpected end when reading token. Path ''.", exception.Message);
    }

    [Fact]
    public void Test3()
    {
        var a = new JArray();

        var writer = a.CreateWriter();

        var reader = new JsonTextReader(new StringReader("""["1","""));
        reader.Read();

        var exception = Assert.Throws<JsonWriterException>(() => writer.WriteToken(reader));
        Assert.Equal("Unexpected end when reading token. Path ''.", exception.Message);
    }

    [Fact]
    public void Test4()
    {
        var a = new JArray();

        var writer = a.CreateWriter();

        var reader = new JsonTextReader(new StringReader("""[["1","""));
        reader.Read();
        reader.Read();

        var exception = Assert.Throws<JsonWriterException>(() => writer.WriteToken(reader));
        Assert.Equal("Unexpected end when reading token. Path ''.", exception.Message);
    }

    [Fact]
    public void Test5()
    {
        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter);
        jsonWriter.WriteStartArray();

        var reader = new JsonTextReader(new StringReader("""[["1","""));
        reader.Read();
        reader.Read();

        var exception = Assert.Throws<JsonWriterException>(() => jsonWriter.WriteToken(reader));
        Assert.Equal("Unexpected end when reading token. Path '[0]'.", exception.Message);
    }
}