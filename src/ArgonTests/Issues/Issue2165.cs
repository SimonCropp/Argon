// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue2165
{
    [Fact]
    public void Test_Deserializer() =>
        XUnitAssert.Throws<JsonWriterException>(
            () => JsonConvert.DeserializeObject<JObject>("{"),
            "Unexpected end when reading token. Path ''.");

    [Fact]
    public void Test()
    {
        var w = new StringWriter();
        var writer = new JsonTextWriter(w);

        var jsonReader = new JsonTextReader(new StringReader("{"));
        jsonReader.Read();

        XUnitAssert.Throws<JsonWriterException>(
            () => writer.WriteToken(jsonReader),
            "Unexpected end when reading token. Path ''.");
    }
}