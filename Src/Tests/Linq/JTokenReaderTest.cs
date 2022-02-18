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
using Argon.Tests.Serialization;
using Argon.Tests.TestObjects;

namespace Argon.Tests.Linq;

public class JTokenReaderTest : TestFixtureBase
{
    [Fact]
    public void ConvertBigIntegerToDouble()
    {
        var jObject = JObject.Parse("{ maxValue:10000000000000000000}");

        var reader = jObject.CreateReader();
        Assert.True(reader.Read());
        Assert.True(reader.Read());
        Assert.Equal(10000000000000000000d, reader.ReadAsDouble());
        Assert.True(reader.Read());
    }

    [Fact]
    public void ConvertBigIntegerToDecimal()
    {
        var jObject = JObject.Parse("{ maxValue:10000000000000000000}");

        var reader = jObject.CreateReader();
        Assert.True(reader.Read());
        Assert.True(reader.Read());
        Assert.Equal(10000000000000000000m, reader.ReadAsDecimal());
        Assert.True(reader.Read());
    }

    [Fact]
    public void ErrorTokenIndex()
    {
        var json = JObject.Parse(@"{""IntList"":[1, ""two""]}");

        XUnitAssert.Throws<Exception>(() =>
        {
            var serializer = new JsonSerializer();

            serializer.Deserialize<TraceTestObject>(json.CreateReader());
        }, "Could not convert string to integer: two. Path 'IntList[1]', line 1, position 20.");
    }

    [Fact]
    public void YahooFinance()
    {
        var o =
            new JObject(
                new JProperty("Test1", new DateTime(2000, 10, 15, 5, 5, 5, DateTimeKind.Utc)),
                new JProperty("Test2", new DateTimeOffset(2000, 10, 15, 5, 5, 5, new TimeSpan(11, 11, 0))),
                new JProperty("Test3", "Test3Value"),
                new JProperty("Test4", null)
            );

        using (var jsonReader = new JTokenReader(o))
        {
            IJsonLineInfo lineInfo = jsonReader;

            jsonReader.Read();
            Assert.Equal(JsonToken.StartObject, jsonReader.TokenType);
            XUnitAssert.False(lineInfo.HasLineInfo());

            jsonReader.Read();
            Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
            Assert.Equal("Test1", jsonReader.Value);
            XUnitAssert.False(lineInfo.HasLineInfo());

            jsonReader.Read();
            Assert.Equal(JsonToken.Date, jsonReader.TokenType);
            Assert.Equal(new DateTime(2000, 10, 15, 5, 5, 5, DateTimeKind.Utc), jsonReader.Value);
            XUnitAssert.False(lineInfo.HasLineInfo());
            Assert.Equal(0, lineInfo.LinePosition);
            Assert.Equal(0, lineInfo.LineNumber);

            jsonReader.Read();
            Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
            Assert.Equal("Test2", jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.Date, jsonReader.TokenType);
            Assert.Equal(new DateTimeOffset(2000, 10, 15, 5, 5, 5, new TimeSpan(11, 11, 0)), jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
            Assert.Equal("Test3", jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.String, jsonReader.TokenType);
            Assert.Equal("Test3Value", jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
            Assert.Equal("Test4", jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.Null, jsonReader.TokenType);
            Assert.Equal(null, jsonReader.Value);

            Assert.True(jsonReader.Read());
            Assert.Equal(JsonToken.EndObject, jsonReader.TokenType);

            Assert.False(jsonReader.Read());
            Assert.Equal(JsonToken.None, jsonReader.TokenType);
        }

        using (JsonReader jsonReader = new JTokenReader(o.Property("Test2")))
        {
            Assert.True(jsonReader.Read());
            Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
            Assert.Equal("Test2", jsonReader.Value);

            Assert.True(jsonReader.Read());
            Assert.Equal(JsonToken.Date, jsonReader.TokenType);
            Assert.Equal(new DateTimeOffset(2000, 10, 15, 5, 5, 5, new TimeSpan(11, 11, 0)), jsonReader.Value);

            Assert.False(jsonReader.Read());
            Assert.Equal(JsonToken.None, jsonReader.TokenType);
        }
    }

    [Fact]
    public void ReadAsDateTimeOffsetBadString()
    {
        var json = @"{""Offset"":""blablahbla""}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        XUnitAssert.Throws<JsonReaderException>(() => { reader.ReadAsDateTimeOffset(); }, "Could not convert string to DateTimeOffset: blablahbla. Path 'Offset', line 1, position 22.");
    }

    [Fact]
    public void ReadAsDateTimeOffsetBoolean()
    {
        var json = @"{""Offset"":true}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        XUnitAssert.Throws<JsonReaderException>(() => { reader.ReadAsDateTimeOffset(); }, "Error reading date. Unexpected token: Boolean. Path 'Offset', line 1, position 14.");
    }

    [Fact]
    public void ReadAsDateTimeOffsetString()
    {
        var json = @"{""Offset"":""2012-01-24T03:50Z""}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        reader.ReadAsDateTimeOffset();
        Assert.Equal(JsonToken.Date, reader.TokenType);
        Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        Assert.Equal(new DateTimeOffset(2012, 1, 24, 3, 50, 0, TimeSpan.Zero), reader.Value);
    }

    [Fact]
    public void ReadLineInfo()
    {
        var input = @"{
  CPU: 'Intel',
  Drives: [
    'DVD read/writer',
    ""500 gigabyte hard drive""
  ]
}";

        var o = JObject.Parse(input);

        using (var jsonReader = new JTokenReader(o))
        {
            IJsonLineInfo lineInfo = jsonReader;

            Assert.Equal(jsonReader.TokenType, JsonToken.None);
            Assert.Equal(0, lineInfo.LineNumber);
            Assert.Equal(0, lineInfo.LinePosition);
            XUnitAssert.False(lineInfo.HasLineInfo());
            Assert.Equal(null, jsonReader.CurrentToken);

            jsonReader.Read();
            Assert.Equal(jsonReader.TokenType, JsonToken.StartObject);
            Assert.Equal(1, lineInfo.LineNumber);
            Assert.Equal(1, lineInfo.LinePosition);
            XUnitAssert.True(lineInfo.HasLineInfo());
            Assert.Equal(o, jsonReader.CurrentToken);

            jsonReader.Read();
            Assert.Equal(jsonReader.TokenType, JsonToken.PropertyName);
            Assert.Equal(jsonReader.Value, "CPU");
            Assert.Equal(2, lineInfo.LineNumber);
            Assert.Equal(6, lineInfo.LinePosition);
            XUnitAssert.True(lineInfo.HasLineInfo());
            Assert.Equal(o.Property("CPU"), jsonReader.CurrentToken);

            jsonReader.Read();
            Assert.Equal(jsonReader.TokenType, JsonToken.String);
            Assert.Equal(jsonReader.Value, "Intel");
            Assert.Equal(2, lineInfo.LineNumber);
            Assert.Equal(14, lineInfo.LinePosition);
            XUnitAssert.True(lineInfo.HasLineInfo());
            Assert.Equal(o.Property("CPU").Value, jsonReader.CurrentToken);

            jsonReader.Read();
            Assert.Equal(jsonReader.TokenType, JsonToken.PropertyName);
            Assert.Equal(jsonReader.Value, "Drives");
            Assert.Equal(3, lineInfo.LineNumber);
            Assert.Equal(9, lineInfo.LinePosition);
            XUnitAssert.True(lineInfo.HasLineInfo());
            Assert.Equal(o.Property("Drives"), jsonReader.CurrentToken);

            jsonReader.Read();
            Assert.Equal(jsonReader.TokenType, JsonToken.StartArray);
            Assert.Equal(3, lineInfo.LineNumber);
            Assert.Equal(11, lineInfo.LinePosition);
            XUnitAssert.True(lineInfo.HasLineInfo());
            Assert.Equal(o.Property("Drives").Value, jsonReader.CurrentToken);

            jsonReader.Read();
            Assert.Equal(jsonReader.TokenType, JsonToken.String);
            Assert.Equal(jsonReader.Value, "DVD read/writer");
            Assert.Equal(4, lineInfo.LineNumber);
            Assert.Equal(21, lineInfo.LinePosition);
            XUnitAssert.True(lineInfo.HasLineInfo());
            Assert.Equal(o["Drives"][0], jsonReader.CurrentToken);

            jsonReader.Read();
            Assert.Equal(jsonReader.TokenType, JsonToken.String);
            Assert.Equal(jsonReader.Value, "500 gigabyte hard drive");
            Assert.Equal(5, lineInfo.LineNumber);
            Assert.Equal(29, lineInfo.LinePosition);
            XUnitAssert.True(lineInfo.HasLineInfo());
            Assert.Equal(o["Drives"][1], jsonReader.CurrentToken);

            jsonReader.Read();
            Assert.Equal(jsonReader.TokenType, JsonToken.EndArray);
            Assert.Equal(3, lineInfo.LineNumber);
            Assert.Equal(11, lineInfo.LinePosition);
            XUnitAssert.True(lineInfo.HasLineInfo());
            Assert.Equal(o["Drives"], jsonReader.CurrentToken);

            jsonReader.Read();
            Assert.Equal(jsonReader.TokenType, JsonToken.EndObject);
            Assert.Equal(1, lineInfo.LineNumber);
            Assert.Equal(1, lineInfo.LinePosition);
            XUnitAssert.True(lineInfo.HasLineInfo());
            Assert.Equal(o, jsonReader.CurrentToken);

            jsonReader.Read();
            Assert.Equal(jsonReader.TokenType, JsonToken.None);
            Assert.Equal(null, jsonReader.CurrentToken);

            jsonReader.Read();
            Assert.Equal(jsonReader.TokenType, JsonToken.None);
            Assert.Equal(null, jsonReader.CurrentToken);
        }
    }

    [Fact]
    public void ReadBytes()
    {
        var data = Encoding.UTF8.GetBytes("Hello world!");

        var o =
            new JObject(
                new JProperty("Test1", data)
            );

        using (var jsonReader = new JTokenReader(o))
        {
            jsonReader.Read();
            Assert.Equal(JsonToken.StartObject, jsonReader.TokenType);

            jsonReader.Read();
            Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
            Assert.Equal("Test1", jsonReader.Value);

            var readBytes = jsonReader.ReadAsBytes();
            Assert.Equal(data, readBytes);

            Assert.True(jsonReader.Read());
            Assert.Equal(JsonToken.EndObject, jsonReader.TokenType);

            Assert.False(jsonReader.Read());
            Assert.Equal(JsonToken.None, jsonReader.TokenType);
        }
    }

    [Fact]
    public void ReadBytesFailure()
    {
        XUnitAssert.Throws<JsonReaderException>(() =>
        {
            var o =
                new JObject(
                    new JProperty("Test1", 1)
                );

            using (var jsonReader = new JTokenReader(o))
            {
                jsonReader.Read();
                Assert.Equal(JsonToken.StartObject, jsonReader.TokenType);

                jsonReader.Read();
                Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
                Assert.Equal("Test1", jsonReader.Value);

                jsonReader.ReadAsBytes();
            }
        }, "Error reading bytes. Unexpected token: Integer. Path 'Test1'.");
    }

    public class HasBytes
    {
        public byte[] Bytes { get; set; }
    }

    [Fact]
    public void ReadBytesFromString()
    {
        var bytes = new HasBytes { Bytes = new byte[] { 1, 2, 3, 4 } };
        var json = JsonConvert.SerializeObject(bytes);

        TextReader textReader = new StringReader(json);
        JsonReader jsonReader = new JsonTextReader(textReader);

        var jToken = JToken.ReadFrom(jsonReader);

        jsonReader = new JTokenReader(jToken);

        var result2 = (HasBytes)JsonSerializer.Create(null)
            .Deserialize(jsonReader, typeof(HasBytes));

        Assert.Equal(new byte[] { 1, 2, 3, 4 }, result2.Bytes);
    }

    [Fact]
    public void ReadBytesFromEmptyString()
    {
        var bytes = new HasBytes { Bytes = new byte[0] };
        var json = JsonConvert.SerializeObject(bytes);

        TextReader textReader = new StringReader(json);
        JsonReader jsonReader = new JsonTextReader(textReader);

        var jToken = JToken.ReadFrom(jsonReader);

        jsonReader = new JTokenReader(jToken);

        var result2 = (HasBytes)JsonSerializer.Create(null)
            .Deserialize(jsonReader, typeof(HasBytes));

        Assert.Equal(new byte[0], result2.Bytes);
    }

    public class ReadAsBytesTestObject
    {
        public byte[] Data;
    }

    [Fact]
    public void ReadAsBytesNull()
    {
        var s = new JsonSerializer();

        var nullToken = JToken.ReadFrom(new JsonTextReader(new StringReader("{ Data: null }")));
        var x = s.Deserialize<ReadAsBytesTestObject>(new JTokenReader(nullToken));
        Assert.Null(x.Data);
    }

    [Fact]
    public void DeserializeByteArrayWithTypeNameHandling()
    {
        var test = new TestObject("Test", new byte[] { 72, 63, 62, 71, 92, 55 });

        var json = JsonConvert.SerializeObject(test, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        });

        var o = JObject.Parse(json);

        var serializer = new JsonSerializer
        {
            TypeNameHandling = TypeNameHandling.All
        };

        using (var nodeReader = o.CreateReader())
        {
            // Get exception here
            var newObject = (TestObject)serializer.Deserialize(nodeReader);

            Assert.Equal("Test", newObject.Name);
            Assert.Equal(new byte[] { 72, 63, 62, 71, 92, 55 }, newObject.Data);
        }
    }

    [Fact]
    public void DeserializeStringInt()
    {
        var json = @"{
  ""PreProperty"": ""99"",
  ""PostProperty"": ""-1""
}";

        var o = JObject.Parse(json);

        var serializer = new JsonSerializer();

        using (var nodeReader = o.CreateReader())
        {
            var c = serializer.Deserialize<MyClass>(nodeReader);

            Assert.Equal(99, c.PreProperty);
            Assert.Equal(-1, c.PostProperty);
        }
    }

    [Fact]
    public void ReadAsDecimalInt()
    {
        var json = @"{""Name"":1}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        reader.ReadAsDecimal();
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(typeof(decimal), reader.ValueType);
        Assert.Equal(1m, reader.Value);
    }

    [Fact]
    public void ReadAsInt32Int()
    {
        var json = @"{""Name"":1}";

        var o = JObject.Parse(json);

        var reader = (JTokenReader)o.CreateReader();

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);
        Assert.Equal(o, reader.CurrentToken);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal(o.Property("Name"), reader.CurrentToken);

        reader.ReadAsInt32();
        Assert.Equal(o["Name"], reader.CurrentToken);
        Assert.Equal(JsonToken.Integer, reader.TokenType);
        Assert.Equal(typeof(int), reader.ValueType);
        Assert.Equal(1, reader.Value);
    }

    [Fact]
    public void ReadAsInt32BadString()
    {
        var json = @"{""Name"":""hi""}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        XUnitAssert.Throws<JsonReaderException>(() => { reader.ReadAsInt32(); }, "Could not convert string to integer: hi. Path 'Name', line 1, position 12.");
    }

    [Fact]
    public void ReadAsInt32Boolean()
    {
        var json = @"{""Name"":true}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        XUnitAssert.Throws<JsonReaderException>(() => { reader.ReadAsInt32(); }, "Error reading integer. Unexpected token: Boolean. Path 'Name', line 1, position 12.");
    }

    [Fact]
    public void ReadAsDecimalString()
    {
        var json = @"{""Name"":""1.1""}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        reader.ReadAsDecimal();
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(typeof(decimal), reader.ValueType);
        Assert.Equal(1.1m, reader.Value);
    }

    [Fact]
    public void ReadAsDecimalBadString()
    {
        var json = @"{""Name"":""blah""}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        XUnitAssert.Throws<JsonReaderException>(() => { reader.ReadAsDecimal(); }, "Could not convert string to decimal: blah. Path 'Name', line 1, position 14.");
    }

    [Fact]
    public void ReadAsDecimalBoolean()
    {
        var json = @"{""Name"":true}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        XUnitAssert.Throws<JsonReaderException>(() => { reader.ReadAsDecimal(); }, "Error reading decimal. Unexpected token: Boolean. Path 'Name', line 1, position 12.");
    }

    [Fact]
    public void ReadAsDecimalNull()
    {
        var json = @"{""Name"":null}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        reader.ReadAsDecimal();
        Assert.Equal(JsonToken.Null, reader.TokenType);
        Assert.Equal(null, reader.ValueType);
        Assert.Equal(null, reader.Value);
    }

    [Fact]
    public void InitialPath_PropertyBase_PropertyToken()
    {
        var o = new JObject
        {
            { "prop1", true }
        };

        var reader = new JTokenReader(o, "baseprop");

        Assert.Equal("baseprop", reader.Path);

        Assert.True(reader.Read());
        Assert.Equal("baseprop", reader.Path);

        Assert.True(reader.Read());
        Assert.Equal("baseprop.prop1", reader.Path);

        Assert.True(reader.Read());
        Assert.Equal("baseprop.prop1", reader.Path);

        Assert.True(reader.Read());
        Assert.Equal("baseprop", reader.Path);

        Assert.False(reader.Read());
        Assert.Equal("baseprop", reader.Path);
    }

    [Fact]
    public void InitialPath_ArrayBase_PropertyToken()
    {
        var o = new JObject
        {
            { "prop1", true }
        };

        var reader = new JTokenReader(o, "[0]");

        Assert.Equal("[0]", reader.Path);

        Assert.True(reader.Read());
        Assert.Equal("[0]", reader.Path);

        Assert.True(reader.Read());
        Assert.Equal("[0].prop1", reader.Path);

        Assert.True(reader.Read());
        Assert.Equal("[0].prop1", reader.Path);

        Assert.True(reader.Read());
        Assert.Equal("[0]", reader.Path);

        Assert.False(reader.Read());
        Assert.Equal("[0]", reader.Path);
    }

    [Fact]
    public void InitialPath_PropertyBase_ArrayToken()
    {
        var a = new JArray
        {
            1, 2
        };

        var reader = new JTokenReader(a, "baseprop");

        Assert.Equal("baseprop", reader.Path);

        Assert.True(reader.Read());
        Assert.Equal("baseprop", reader.Path);

        Assert.True(reader.Read());
        Assert.Equal("baseprop[0]", reader.Path);

        Assert.True(reader.Read());
        Assert.Equal("baseprop[1]", reader.Path);

        Assert.True(reader.Read());
        Assert.Equal("baseprop", reader.Path);

        Assert.False(reader.Read());
        Assert.Equal("baseprop", reader.Path);
    }

    [Fact]
    public void InitialPath_ArrayBase_ArrayToken()
    {
        var a = new JArray
        {
            1, 2
        };

        var reader = new JTokenReader(a, "[0]");

        Assert.Equal("[0]", reader.Path);

        Assert.True(reader.Read());
        Assert.Equal("[0]", reader.Path);

        Assert.True(reader.Read());
        Assert.Equal("[0][0]", reader.Path);

        Assert.True(reader.Read());
        Assert.Equal("[0][1]", reader.Path);

        Assert.True(reader.Read());
        Assert.Equal("[0]", reader.Path);

        Assert.False(reader.Read());
        Assert.Equal("[0]", reader.Path);
    }

    [Fact]
    public void ReadAsDouble_InvalidToken()
    {
        var a = new JArray
        {
            1, 2
        };

        var reader = new JTokenReader(a);

        XUnitAssert.Throws<JsonReaderException>(
            () => { reader.ReadAsDouble(); },
            "Error reading double. Unexpected token: StartArray. Path ''.");
    }

    [Fact]
    public void ReadAsBoolean_InvalidToken()
    {
        var a = new JArray
        {
            1, 2
        };

        var reader = new JTokenReader(a);

        XUnitAssert.Throws<JsonReaderException>(
            () => { reader.ReadAsBoolean(); },
            "Error reading boolean. Unexpected token: StartArray. Path ''.");
    }

    [Fact]
    public void ReadAsDateTime_InvalidToken()
    {
        var a = new JArray
        {
            1, 2
        };

        var reader = new JTokenReader(a);

        XUnitAssert.Throws<JsonReaderException>(
            () => { reader.ReadAsDateTime(); },
            "Error reading date. Unexpected token: StartArray. Path ''.");
    }

    [Fact]
    public void ReadAsDateTimeOffset_InvalidToken()
    {
        var a = new JArray
        {
            1, 2
        };

        var reader = new JTokenReader(a);

        XUnitAssert.Throws<JsonReaderException>(
            () => { reader.ReadAsDateTimeOffset(); },
            "Error reading date. Unexpected token: StartArray. Path ''.");
    }

    [Fact]
    public void ReadAsDateTimeOffset_DateTime()
    {
        var v = new JValue(new DateTime(2001, 12, 12, 12, 12, 12, DateTimeKind.Utc));

        var reader = new JTokenReader(v);

        Assert.Equal(new DateTimeOffset(2001, 12, 12, 12, 12, 12, TimeSpan.Zero), reader.ReadAsDateTimeOffset());
    }

    [Fact]
    public void ReadAsDateTimeOffset_String()
    {
        var v = new JValue("2012-01-24T03:50Z");

        var reader = new JTokenReader(v);

        Assert.Equal(new DateTimeOffset(2012, 1, 24, 3, 50, 0, TimeSpan.Zero), reader.ReadAsDateTimeOffset());
    }

    [Fact]
    public void ReadAsDateTime_DateTimeOffset()
    {
        var v = new JValue(new DateTimeOffset(2012, 1, 24, 3, 50, 0, TimeSpan.Zero));

        var reader = new JTokenReader(v);

        Assert.Equal(new DateTime(2012, 1, 24, 3, 50, 0, DateTimeKind.Utc), reader.ReadAsDateTime());
    }

    [Fact]
    public void ReadAsDateTime_String()
    {
        var v = new JValue("2012-01-24T03:50Z");

        var reader = new JTokenReader(v);

        Assert.Equal(new DateTime(2012, 1, 24, 3, 50, 0, DateTimeKind.Utc), reader.ReadAsDateTime());
    }

    [Fact]
    public void ReadAsDouble_String_Success()
    {
        var s = JValue.CreateString("123.4");

        var reader = new JTokenReader(s);

        Assert.Equal(123.4d, reader.ReadAsDouble());
    }

    [Fact]
    public void ReadAsDouble_Null_Success()
    {
        var n = JValue.CreateNull();

        var reader = new JTokenReader(n);

        Assert.Equal(null, reader.ReadAsDouble());
    }

    [Fact]
    public void ReadAsDouble_Integer_Success()
    {
        var n = new JValue(1);

        var reader = new JTokenReader(n);

        Assert.Equal(1d, reader.ReadAsDouble());
    }

    [Fact]
    public void ReadAsBoolean_BigInteger_Success()
    {
        var s = new JValue(BigInteger.Parse("99999999999999999999999999999999999999999999999999999999999999999999999999"));

        var reader = new JTokenReader(s);

        XUnitAssert.True(reader.ReadAsBoolean());
    }

    [Fact]
    public void ReadAsBoolean_String_Success()
    {
        var s = JValue.CreateString("true");

        var reader = new JTokenReader(s);

        XUnitAssert.True(reader.ReadAsBoolean());
    }

    [Fact]
    public void ReadAsBoolean_Null_Success()
    {
        var n = JValue.CreateNull();

        var reader = new JTokenReader(n);

        Assert.Equal(null, reader.ReadAsBoolean());
    }

    [Fact]
    public void ReadAsBoolean_Integer_Success()
    {
        var n = new JValue(1);

        var reader = new JTokenReader(n);

        XUnitAssert.True(reader.ReadAsBoolean());
    }

    [Fact]
    public void ReadAsDateTime_Null_Success()
    {
        var n = JValue.CreateNull();

        var reader = new JTokenReader(n);

        Assert.Equal(null, reader.ReadAsDateTime());
    }

    [Fact]
    public void ReadAsDateTimeOffset_Null_Success()
    {
        var n = JValue.CreateNull();

        var reader = new JTokenReader(n);

        Assert.Equal(null, reader.ReadAsDateTimeOffset());
    }

    [Fact]
    public void ReadAsString_Integer_Success()
    {
        var n = new JValue(1);

        var reader = new JTokenReader(n);

        Assert.Equal("1", reader.ReadAsString());
    }

    [Fact]
    public void ReadAsString_Guid_Success()
    {
        var n = new JValue(new Uri("http://www.test.com"));

        var reader = new JTokenReader(n);

        Assert.Equal("http://www.test.com", reader.ReadAsString());
    }

    [Fact]
    public void ReadAsBytes_Integer_Success()
    {
        var n = JValue.CreateNull();

        var reader = new JTokenReader(n);

        Assert.Equal(null, reader.ReadAsBytes());
    }

    [Fact]
    public void ReadAsBytes_Array()
    {
        var a = new JArray
        {
            1, 2
        };

        var reader = new JTokenReader(a);

        var bytes = reader.ReadAsBytes();

        Assert.Equal(2, bytes.Length);
        Assert.Equal(1, bytes[0]);
        Assert.Equal(2, bytes[1]);
    }

    [Fact]
    public void ReadAsBytes_Null()
    {
        var n = JValue.CreateNull();

        var reader = new JTokenReader(n);

        Assert.Equal(null, reader.ReadAsBytes());
    }
}