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
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;
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
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(10000000000000000000d, reader.ReadAsDouble());
        Xunit.Assert.True(reader.Read());
    }

    [Fact]
    public void ConvertBigIntegerToDecimal()
    {
        var jObject = JObject.Parse("{ maxValue:10000000000000000000}");

        var reader = jObject.CreateReader();
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(10000000000000000000m, reader.ReadAsDecimal());
        Xunit.Assert.True(reader.Read());
    }

    [Fact]
    public void ErrorTokenIndex()
    {
        var json = JObject.Parse(@"{""IntList"":[1, ""two""]}");

        ExceptionAssert.Throws<Exception>(() =>
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
            Assert.AreEqual(JsonToken.StartObject, jsonReader.TokenType);
            Assert.False( lineInfo.HasLineInfo());

            jsonReader.Read();
            Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
            Assert.AreEqual("Test1", jsonReader.Value);
            Assert.False( lineInfo.HasLineInfo());

            jsonReader.Read();
            Assert.AreEqual(JsonToken.Date, jsonReader.TokenType);
            Assert.AreEqual(new DateTime(2000, 10, 15, 5, 5, 5, DateTimeKind.Utc), jsonReader.Value);
            Assert.False( lineInfo.HasLineInfo());
            Assert.AreEqual(0, lineInfo.LinePosition);
            Assert.AreEqual(0, lineInfo.LineNumber);

            jsonReader.Read();
            Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
            Assert.AreEqual("Test2", jsonReader.Value);

            jsonReader.Read();
            Assert.AreEqual(JsonToken.Date, jsonReader.TokenType);
            Assert.AreEqual(new DateTimeOffset(2000, 10, 15, 5, 5, 5, new TimeSpan(11, 11, 0)), jsonReader.Value);

            jsonReader.Read();
            Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
            Assert.AreEqual("Test3", jsonReader.Value);

            jsonReader.Read();
            Assert.AreEqual(JsonToken.String, jsonReader.TokenType);
            Assert.AreEqual("Test3Value", jsonReader.Value);

            jsonReader.Read();
            Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
            Assert.AreEqual("Test4", jsonReader.Value);

            jsonReader.Read();
            Assert.AreEqual(JsonToken.Null, jsonReader.TokenType);
            Assert.AreEqual(null, jsonReader.Value);

            Xunit.Assert.True(jsonReader.Read());
            Assert.AreEqual(JsonToken.EndObject, jsonReader.TokenType);

            Xunit.Assert.False(jsonReader.Read());
            Assert.AreEqual(JsonToken.None, jsonReader.TokenType);
        }

        using (JsonReader jsonReader = new JTokenReader(o.Property("Test2")))
        {
            Xunit.Assert.True(jsonReader.Read());
            Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
            Assert.AreEqual("Test2", jsonReader.Value);

            Xunit.Assert.True(jsonReader.Read());
            Assert.AreEqual(JsonToken.Date, jsonReader.TokenType);
            Assert.AreEqual(new DateTimeOffset(2000, 10, 15, 5, 5, 5, new TimeSpan(11, 11, 0)), jsonReader.Value);

            Xunit.Assert.False(jsonReader.Read());
            Assert.AreEqual(JsonToken.None, jsonReader.TokenType);
        }
    }

    [Fact]
    public void ReadAsDateTimeOffsetBadString()
    {
        var json = @"{""Offset"":""blablahbla""}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDateTimeOffset(); }, "Could not convert string to DateTimeOffset: blablahbla. Path 'Offset', line 1, position 22.");
    }

    [Fact]
    public void ReadAsDateTimeOffsetBoolean()
    {
        var json = @"{""Offset"":true}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDateTimeOffset(); }, "Error reading date. Unexpected token: Boolean. Path 'Offset', line 1, position 14.");
    }

    [Fact]
    public void ReadAsDateTimeOffsetString()
    {
        var json = @"{""Offset"":""2012-01-24T03:50Z""}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        reader.ReadAsDateTimeOffset();
        Assert.AreEqual(JsonToken.Date, reader.TokenType);
        Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
        Assert.AreEqual(new DateTimeOffset(2012, 1, 24, 3, 50, 0, TimeSpan.Zero), reader.Value);
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

            Assert.AreEqual(jsonReader.TokenType, JsonToken.None);
            Assert.AreEqual(0, lineInfo.LineNumber);
            Assert.AreEqual(0, lineInfo.LinePosition);
            Assert.False( lineInfo.HasLineInfo());
            Assert.AreEqual(null, jsonReader.CurrentToken);

            jsonReader.Read();
            Assert.AreEqual(jsonReader.TokenType, JsonToken.StartObject);
            Assert.AreEqual(1, lineInfo.LineNumber);
            Assert.AreEqual(1, lineInfo.LinePosition);
            Assert.True( lineInfo.HasLineInfo());
            Assert.AreEqual(o, jsonReader.CurrentToken);

            jsonReader.Read();
            Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);
            Assert.AreEqual(jsonReader.Value, "CPU");
            Assert.AreEqual(2, lineInfo.LineNumber);
            Assert.AreEqual(6, lineInfo.LinePosition);
            Assert.True( lineInfo.HasLineInfo());
            Assert.AreEqual(o.Property("CPU"), jsonReader.CurrentToken);

            jsonReader.Read();
            Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
            Assert.AreEqual(jsonReader.Value, "Intel");
            Assert.AreEqual(2, lineInfo.LineNumber);
            Assert.AreEqual(14, lineInfo.LinePosition);
            Assert.True( lineInfo.HasLineInfo());
            Assert.AreEqual(o.Property("CPU").Value, jsonReader.CurrentToken);

            jsonReader.Read();
            Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);
            Assert.AreEqual(jsonReader.Value, "Drives");
            Assert.AreEqual(3, lineInfo.LineNumber);
            Assert.AreEqual(9, lineInfo.LinePosition);
            Assert.True( lineInfo.HasLineInfo());
            Assert.AreEqual(o.Property("Drives"), jsonReader.CurrentToken);

            jsonReader.Read();
            Assert.AreEqual(jsonReader.TokenType, JsonToken.StartArray);
            Assert.AreEqual(3, lineInfo.LineNumber);
            Assert.AreEqual(11, lineInfo.LinePosition);
            Assert.True( lineInfo.HasLineInfo());
            Assert.AreEqual(o.Property("Drives").Value, jsonReader.CurrentToken);

            jsonReader.Read();
            Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
            Assert.AreEqual(jsonReader.Value, "DVD read/writer");
            Assert.AreEqual(4, lineInfo.LineNumber);
            Assert.AreEqual(21, lineInfo.LinePosition);
            Assert.True( lineInfo.HasLineInfo());
            Assert.AreEqual(o["Drives"][0], jsonReader.CurrentToken);

            jsonReader.Read();
            Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
            Assert.AreEqual(jsonReader.Value, "500 gigabyte hard drive");
            Assert.AreEqual(5, lineInfo.LineNumber);
            Assert.AreEqual(29, lineInfo.LinePosition);
            Assert.True( lineInfo.HasLineInfo());
            Assert.AreEqual(o["Drives"][1], jsonReader.CurrentToken);

            jsonReader.Read();
            Assert.AreEqual(jsonReader.TokenType, JsonToken.EndArray);
            Assert.AreEqual(3, lineInfo.LineNumber);
            Assert.AreEqual(11, lineInfo.LinePosition);
            Assert.True( lineInfo.HasLineInfo());
            Assert.AreEqual(o["Drives"], jsonReader.CurrentToken);

            jsonReader.Read();
            Assert.AreEqual(jsonReader.TokenType, JsonToken.EndObject);
            Assert.AreEqual(1, lineInfo.LineNumber);
            Assert.AreEqual(1, lineInfo.LinePosition);
            Assert.True( lineInfo.HasLineInfo());
            Assert.AreEqual(o, jsonReader.CurrentToken);

            jsonReader.Read();
            Assert.AreEqual(jsonReader.TokenType, JsonToken.None);
            Assert.AreEqual(null, jsonReader.CurrentToken);

            jsonReader.Read();
            Assert.AreEqual(jsonReader.TokenType, JsonToken.None);
            Assert.AreEqual(null, jsonReader.CurrentToken);
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
            Assert.AreEqual(JsonToken.StartObject, jsonReader.TokenType);

            jsonReader.Read();
            Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
            Assert.AreEqual("Test1", jsonReader.Value);

            var readBytes = jsonReader.ReadAsBytes();
            Assert.AreEqual(data, readBytes);

            Xunit.Assert.True(jsonReader.Read());
            Assert.AreEqual(JsonToken.EndObject, jsonReader.TokenType);

            Xunit.Assert.False(jsonReader.Read());
            Assert.AreEqual(JsonToken.None, jsonReader.TokenType);
        }
    }

    [Fact]
    public void ReadBytesFailure()
    {
        ExceptionAssert.Throws<JsonReaderException>(() =>
        {
            var o =
                new JObject(
                    new JProperty("Test1", 1)
                );

            using (var jsonReader = new JTokenReader(o))
            {
                jsonReader.Read();
                Assert.AreEqual(JsonToken.StartObject, jsonReader.TokenType);

                jsonReader.Read();
                Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
                Assert.AreEqual("Test1", jsonReader.Value);

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

        Xunit.Assert.Equal(new byte[] { 1, 2, 3, 4 }, result2.Bytes);
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

        Xunit.Assert.Equal(new byte[0], result2.Bytes);
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
        Xunit.Assert.Null(x.Data);
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

            Assert.AreEqual("Test", newObject.Name);
            Xunit.Assert.Equal(new byte[] { 72, 63, 62, 71, 92, 55 }, newObject.Data);
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

            Assert.AreEqual(99, c.PreProperty);
            Assert.AreEqual(-1, c.PostProperty);
        }
    }

    [Fact]
    public void ReadAsDecimalInt()
    {
        var json = @"{""Name"":1}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        reader.ReadAsDecimal();
        Assert.AreEqual(JsonToken.Float, reader.TokenType);
        Assert.AreEqual(typeof(decimal), reader.ValueType);
        Assert.AreEqual(1m, reader.Value);
    }

    [Fact]
    public void ReadAsInt32Int()
    {
        var json = @"{""Name"":1}";

        var o = JObject.Parse(json);

        var reader = (JTokenReader)o.CreateReader();

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);
        Assert.AreEqual(o, reader.CurrentToken);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
        Assert.AreEqual(o.Property("Name"), reader.CurrentToken);

        reader.ReadAsInt32();
        Assert.AreEqual(o["Name"], reader.CurrentToken);
        Assert.AreEqual(JsonToken.Integer, reader.TokenType);
        Assert.AreEqual(typeof(int), reader.ValueType);
        Assert.AreEqual(1, reader.Value);
    }

    [Fact]
    public void ReadAsInt32BadString()
    {
        var json = @"{""Name"":""hi""}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsInt32(); }, "Could not convert string to integer: hi. Path 'Name', line 1, position 12.");
    }

    [Fact]
    public void ReadAsInt32Boolean()
    {
        var json = @"{""Name"":true}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsInt32(); }, "Error reading integer. Unexpected token: Boolean. Path 'Name', line 1, position 12.");
    }

    [Fact]
    public void ReadAsDecimalString()
    {
        var json = @"{""Name"":""1.1""}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        reader.ReadAsDecimal();
        Assert.AreEqual(JsonToken.Float, reader.TokenType);
        Assert.AreEqual(typeof(decimal), reader.ValueType);
        Assert.AreEqual(1.1m, reader.Value);
    }

    [Fact]
    public void ReadAsDecimalBadString()
    {
        var json = @"{""Name"":""blah""}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDecimal(); }, "Could not convert string to decimal: blah. Path 'Name', line 1, position 14.");
    }

    [Fact]
    public void ReadAsDecimalBoolean()
    {
        var json = @"{""Name"":true}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDecimal(); }, "Error reading decimal. Unexpected token: Boolean. Path 'Name', line 1, position 12.");
    }

    [Fact]
    public void ReadAsDecimalNull()
    {
        var json = @"{""Name"":null}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        reader.ReadAsDecimal();
        Assert.AreEqual(JsonToken.Null, reader.TokenType);
        Assert.AreEqual(null, reader.ValueType);
        Assert.AreEqual(null, reader.Value);
    }

    [Fact]
    public void InitialPath_PropertyBase_PropertyToken()
    {
        var o = new JObject
        {
            { "prop1", true }
        };

        var reader = new JTokenReader(o, "baseprop");

        Assert.AreEqual("baseprop", reader.Path);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual("baseprop", reader.Path);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual("baseprop.prop1", reader.Path);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual("baseprop.prop1", reader.Path);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual("baseprop", reader.Path);

        Xunit.Assert.False(reader.Read());
        Assert.AreEqual("baseprop", reader.Path);
    }

    [Fact]
    public void InitialPath_ArrayBase_PropertyToken()
    {
        var o = new JObject
        {
            { "prop1", true }
        };

        var reader = new JTokenReader(o, "[0]");

        Assert.AreEqual("[0]", reader.Path);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual("[0]", reader.Path);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual("[0].prop1", reader.Path);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual("[0].prop1", reader.Path);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual("[0]", reader.Path);

        Xunit.Assert.False(reader.Read());
        Assert.AreEqual("[0]", reader.Path);
    }

    [Fact]
    public void InitialPath_PropertyBase_ArrayToken()
    {
        var a = new JArray
        {
            1, 2
        };

        var reader = new JTokenReader(a, "baseprop");

        Assert.AreEqual("baseprop", reader.Path);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual("baseprop", reader.Path);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual("baseprop[0]", reader.Path);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual("baseprop[1]", reader.Path);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual("baseprop", reader.Path);

        Xunit.Assert.False(reader.Read());
        Assert.AreEqual("baseprop", reader.Path);
    }

    [Fact]
    public void InitialPath_ArrayBase_ArrayToken()
    {
        var a = new JArray
        {
            1, 2
        };

        var reader = new JTokenReader(a, "[0]");

        Assert.AreEqual("[0]", reader.Path);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual("[0]", reader.Path);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual("[0][0]", reader.Path);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual("[0][1]", reader.Path);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual("[0]", reader.Path);

        Xunit.Assert.False(reader.Read());
        Assert.AreEqual("[0]", reader.Path);
    }

    [Fact]
    public void ReadAsDouble_InvalidToken()
    {
        var a = new JArray
        {
            1, 2
        };

        var reader = new JTokenReader(a);

        ExceptionAssert.Throws<JsonReaderException>(
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

        ExceptionAssert.Throws<JsonReaderException>(
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

        ExceptionAssert.Throws<JsonReaderException>(
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

        ExceptionAssert.Throws<JsonReaderException>(
            () => { reader.ReadAsDateTimeOffset(); },
            "Error reading date. Unexpected token: StartArray. Path ''.");
    }

    [Fact]
    public void ReadAsDateTimeOffset_DateTime()
    {
        var v = new JValue(new DateTime(2001, 12, 12, 12, 12, 12, DateTimeKind.Utc));

        var reader = new JTokenReader(v);

        Assert.AreEqual(new DateTimeOffset(2001, 12, 12, 12, 12, 12, TimeSpan.Zero), reader.ReadAsDateTimeOffset());
    }

    [Fact]
    public void ReadAsDateTimeOffset_String()
    {
        var v = new JValue("2012-01-24T03:50Z");

        var reader = new JTokenReader(v);

        Assert.AreEqual(new DateTimeOffset(2012, 1, 24, 3, 50, 0, TimeSpan.Zero), reader.ReadAsDateTimeOffset());
    }

    [Fact]
    public void ReadAsDateTime_DateTimeOffset()
    {
        var v = new JValue(new DateTimeOffset(2012, 1, 24, 3, 50, 0, TimeSpan.Zero));

        var reader = new JTokenReader(v);

        Assert.AreEqual(new DateTime(2012, 1, 24, 3, 50, 0, DateTimeKind.Utc), reader.ReadAsDateTime());
    }

    [Fact]
    public void ReadAsDateTime_String()
    {
        var v = new JValue("2012-01-24T03:50Z");

        var reader = new JTokenReader(v);

        Assert.AreEqual(new DateTime(2012, 1, 24, 3, 50, 0, DateTimeKind.Utc), reader.ReadAsDateTime());
    }

    [Fact]
    public void ReadAsDouble_String_Success()
    {
        var s = JValue.CreateString("123.4");

        var reader = new JTokenReader(s);

        Assert.AreEqual(123.4d, reader.ReadAsDouble());
    }

    [Fact]
    public void ReadAsDouble_Null_Success()
    {
        var n = JValue.CreateNull();

        var reader = new JTokenReader(n);

        Assert.AreEqual(null, reader.ReadAsDouble());
    }

    [Fact]
    public void ReadAsDouble_Integer_Success()
    {
        var n = new JValue(1);

        var reader = new JTokenReader(n);

        Assert.AreEqual(1d, reader.ReadAsDouble());
    }

    [Fact]
    public void ReadAsBoolean_BigInteger_Success()
    {
        var s = new JValue(BigInteger.Parse("99999999999999999999999999999999999999999999999999999999999999999999999999"));

        var reader = new JTokenReader(s);

        Assert.True( reader.ReadAsBoolean());
    }

    [Fact]
    public void ReadAsBoolean_String_Success()
    {
        var s = JValue.CreateString("true");

        var reader = new JTokenReader(s);

        Assert.True( reader.ReadAsBoolean());
    }

    [Fact]
    public void ReadAsBoolean_Null_Success()
    {
        var n = JValue.CreateNull();

        var reader = new JTokenReader(n);

        Assert.AreEqual(null, reader.ReadAsBoolean());
    }

    [Fact]
    public void ReadAsBoolean_Integer_Success()
    {
        var n = new JValue(1);

        var reader = new JTokenReader(n);

        Assert.True( reader.ReadAsBoolean());
    }

    [Fact]
    public void ReadAsDateTime_Null_Success()
    {
        var n = JValue.CreateNull();

        var reader = new JTokenReader(n);

        Assert.AreEqual(null, reader.ReadAsDateTime());
    }

    [Fact]
    public void ReadAsDateTimeOffset_Null_Success()
    {
        var n = JValue.CreateNull();

        var reader = new JTokenReader(n);

        Assert.AreEqual(null, reader.ReadAsDateTimeOffset());
    }

    [Fact]
    public void ReadAsString_Integer_Success()
    {
        var n = new JValue(1);

        var reader = new JTokenReader(n);

        Assert.AreEqual("1", reader.ReadAsString());
    }

    [Fact]
    public void ReadAsString_Guid_Success()
    {
        var n = new JValue(new Uri("http://www.test.com"));

        var reader = new JTokenReader(n);

        Assert.AreEqual("http://www.test.com", reader.ReadAsString());
    }

    [Fact]
    public void ReadAsBytes_Integer_Success()
    {
        var n = JValue.CreateNull();

        var reader = new JTokenReader(n);

        Assert.AreEqual(null, reader.ReadAsBytes());
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

        Assert.AreEqual(2, bytes.Length);
        Assert.AreEqual(1, bytes[0]);
        Assert.AreEqual(2, bytes[1]);
    }

    [Fact]
    public void ReadAsBytes_Null()
    {
        var n = JValue.CreateNull();

        var reader = new JTokenReader(n);

        Assert.AreEqual(null, reader.ReadAsBytes());
    }
}