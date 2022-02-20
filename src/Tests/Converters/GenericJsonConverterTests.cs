#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using Xunit;

namespace Argon.Tests.Converters;

public class GenericJsonConverterTests : TestFixtureBase
{
    public class TestGenericConverter : JsonConverter<string>
    {
        public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }

        public override string ReadJson(JsonReader reader, Type objectType, string existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return (string)reader.Value + existingValue;
        }
    }

    [Fact]
    public void WriteJsonObject()
    {
        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter);

        var converter = new TestGenericConverter();
        converter.WriteJson(jsonWriter, (object)"String!", null);

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
            () => { converter.WriteJson(jsonWriter, 123, null); },
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
        var s = (string)converter.ReadJson(jsonReader, typeof(string), null, null);

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
            () => { converter.ReadJson(jsonReader, typeof(string), 12345, null); },
            "Converter cannot read JSON with the specified existing value. System.String is required.");
    }
}