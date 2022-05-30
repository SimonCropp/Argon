// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class GenericJsonConverterTests : TestFixtureBase
{
    public class TestGenericConverter : JsonConverter<string>
    {
        public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer) =>
            writer.WriteValue(value);

        public override string ReadJson(JsonReader reader, Type type, string existingValue, bool hasExisting, JsonSerializer serializer) =>
            (string) reader.Value + existingValue;
    }

    [Fact]
    public void WriteJsonObject()
    {
        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter);

        var converter = new TestGenericConverter();
        converter.WriteJson(jsonWriter, (object) "String!", null);

        Assert.Equal(@"""String!""", stringWriter.ToString());
    }

    [Fact]
    public void WriteJsonGeneric()
    {
        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter);

        var converter = new TestGenericConverter();
        converter.WriteJson(jsonWriter, "String!", null);

        Assert.Equal(@"""String!""", stringWriter.ToString());
    }

    [Fact]
    public void WriteJsonBadType()
    {
        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter);

        var converter = new TestGenericConverter();

        XUnitAssert.Throws<JsonSerializationException>(
            () => converter.WriteJson(jsonWriter, 123, null),
            "Converter cannot write specified value to JSON. System.String is required.");
    }

    [Fact]
    public void ReadJsonGenericExistingValueNull()
    {
        var sr = new StringReader("'String!'");
        var jsonReader = new JsonTextReader(sr);
        jsonReader.Read();

        var converter = new TestGenericConverter();
        var s = converter.ReadJson(jsonReader, typeof(string), null, false, null);

        Assert.Equal(@"String!", s);
    }

    [Fact]
    public void ReadJsonGenericExistingValueString()
    {
        var sr = new StringReader("'String!'");
        var jsonReader = new JsonTextReader(sr);
        jsonReader.Read();

        var converter = new TestGenericConverter();
        var s = converter.ReadJson(jsonReader, typeof(string), "Existing!", true, null);

        Assert.Equal(@"String!Existing!", s);
    }

    [Fact]
    public void ReadJsonObjectExistingValueNull()
    {
        var sr = new StringReader("'String!'");
        var jsonReader = new JsonTextReader(sr);
        jsonReader.Read();

        var converter = new TestGenericConverter();
        var s = (string) converter.ReadJson(jsonReader, typeof(string), null, null);

        Assert.Equal(@"String!", s);
    }

    [Fact]
    public void ReadJsonObjectExistingValueWrongType()
    {
        var sr = new StringReader("'String!'");
        var jsonReader = new JsonTextReader(sr);
        jsonReader.Read();

        var converter = new TestGenericConverter();

        XUnitAssert.Throws<JsonSerializationException>(
            () => converter.ReadJson(jsonReader, typeof(string), 12345, null),
            "Converter cannot read JSON with the specified existing value. System.String is required.");
    }
}