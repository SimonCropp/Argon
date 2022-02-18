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

namespace Argon.Tests.Linq;

public class JTokenReaderAsyncTests : TestFixtureBase
{
    [Fact]
    public async Task ConvertBigIntegerToDoubleAsync()
    {
        var jObject = JObject.Parse("{ maxValue:10000000000000000000}");

        var reader = jObject.CreateReader();
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(10000000000000000000d, await reader.ReadAsDoubleAsync());
        Xunit.Assert.True(await reader.ReadAsync());
    }

    [Fact]
    public async Task YahooFinanceAsync()
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

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(JsonToken.StartObject, jsonReader.TokenType);
            XUnitAssert.False(lineInfo.HasLineInfo());

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
            Xunit.Assert.Equal("Test1", jsonReader.Value);
            XUnitAssert.False(lineInfo.HasLineInfo());

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(JsonToken.Date, jsonReader.TokenType);
            Xunit.Assert.Equal(new DateTime(2000, 10, 15, 5, 5, 5, DateTimeKind.Utc), jsonReader.Value);
            XUnitAssert.False(lineInfo.HasLineInfo());
            Xunit.Assert.Equal(0, lineInfo.LinePosition);
            Xunit.Assert.Equal(0, lineInfo.LineNumber);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
            Xunit.Assert.Equal("Test2", jsonReader.Value);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(JsonToken.Date, jsonReader.TokenType);
            Xunit.Assert.Equal(new DateTimeOffset(2000, 10, 15, 5, 5, 5, new TimeSpan(11, 11, 0)), jsonReader.Value);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
            Xunit.Assert.Equal("Test3", jsonReader.Value);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(JsonToken.String, jsonReader.TokenType);
            Xunit.Assert.Equal("Test3Value", jsonReader.Value);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
            Xunit.Assert.Equal("Test4", jsonReader.Value);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(JsonToken.Null, jsonReader.TokenType);
            Xunit.Assert.Equal(null, jsonReader.Value);

            Xunit.Assert.True(await jsonReader.ReadAsync());
            Xunit.Assert.Equal(JsonToken.EndObject, jsonReader.TokenType);

            Xunit.Assert.False(await jsonReader.ReadAsync());
            Xunit.Assert.Equal(JsonToken.None, jsonReader.TokenType);
        }

        using (JsonReader jsonReader = new JTokenReader(o.Property("Test2")))
        {
            Xunit.Assert.True(await jsonReader.ReadAsync());
            Xunit.Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
            Xunit.Assert.Equal("Test2", jsonReader.Value);

            Xunit.Assert.True(await jsonReader.ReadAsync());
            Xunit.Assert.Equal(JsonToken.Date, jsonReader.TokenType);
            Xunit.Assert.Equal(new DateTimeOffset(2000, 10, 15, 5, 5, 5, new TimeSpan(11, 11, 0)), jsonReader.Value);

            Xunit.Assert.False(await jsonReader.ReadAsync());
            Xunit.Assert.Equal(JsonToken.None, jsonReader.TokenType);
        }
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetBadStringAsync()
    {
        var json = @"{""Offset"":""blablahbla""}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDateTimeOffsetAsync(); }, "Could not convert string to DateTimeOffset: blablahbla. Path 'Offset', line 1, position 22.");
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetBooleanAsync()
    {
        var json = @"{""Offset"":true}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDateTimeOffsetAsync(); }, "Error reading date. Unexpected token: Boolean. Path 'Offset', line 1, position 14.");
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetStringAsync()
    {
        var json = @"{""Offset"":""2012-01-24T03:50Z""}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsDateTimeOffsetAsync();
        Xunit.Assert.Equal(JsonToken.Date, reader.TokenType);
        Xunit.Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        Xunit.Assert.Equal(new DateTimeOffset(2012, 1, 24, 3, 50, 0, TimeSpan.Zero), reader.Value);
    }

    [Fact]
    public async Task ReadLineInfoAsync()
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

            Xunit.Assert.Equal(jsonReader.TokenType, JsonToken.None);
            Xunit.Assert.Equal(0, lineInfo.LineNumber);
            Xunit.Assert.Equal(0, lineInfo.LinePosition);
            XUnitAssert.False(lineInfo.HasLineInfo());
            Xunit.Assert.Equal(null, jsonReader.CurrentToken);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(jsonReader.TokenType, JsonToken.StartObject);
            Xunit.Assert.Equal(1, lineInfo.LineNumber);
            Xunit.Assert.Equal(1, lineInfo.LinePosition);
            XUnitAssert.True(lineInfo.HasLineInfo());
            Xunit.Assert.Equal(o, jsonReader.CurrentToken);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(jsonReader.TokenType, JsonToken.PropertyName);
            Xunit.Assert.Equal(jsonReader.Value, "CPU");
            Xunit.Assert.Equal(2, lineInfo.LineNumber);
            Xunit.Assert.Equal(6, lineInfo.LinePosition);
            XUnitAssert.True(lineInfo.HasLineInfo());
            Xunit.Assert.Equal(o.Property("CPU"), jsonReader.CurrentToken);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(jsonReader.TokenType, JsonToken.String);
            Xunit.Assert.Equal(jsonReader.Value, "Intel");
            Xunit.Assert.Equal(2, lineInfo.LineNumber);
            Xunit.Assert.Equal(14, lineInfo.LinePosition);
            XUnitAssert.True(lineInfo.HasLineInfo());
            Xunit.Assert.Equal(o.Property("CPU").Value, jsonReader.CurrentToken);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(jsonReader.TokenType, JsonToken.PropertyName);
            Xunit.Assert.Equal(jsonReader.Value, "Drives");
            Xunit.Assert.Equal(3, lineInfo.LineNumber);
            Xunit.Assert.Equal(9, lineInfo.LinePosition);
            XUnitAssert.True(lineInfo.HasLineInfo());
            Xunit.Assert.Equal(o.Property("Drives"), jsonReader.CurrentToken);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(jsonReader.TokenType, JsonToken.StartArray);
            Xunit.Assert.Equal(3, lineInfo.LineNumber);
            Xunit.Assert.Equal(11, lineInfo.LinePosition);
            XUnitAssert.True(lineInfo.HasLineInfo());
            Xunit.Assert.Equal(o.Property("Drives").Value, jsonReader.CurrentToken);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(jsonReader.TokenType, JsonToken.String);
            Xunit.Assert.Equal(jsonReader.Value, "DVD read/writer");
            Xunit.Assert.Equal(4, lineInfo.LineNumber);
            Xunit.Assert.Equal(21, lineInfo.LinePosition);
            XUnitAssert.True(lineInfo.HasLineInfo());
            Xunit.Assert.Equal(o["Drives"][0], jsonReader.CurrentToken);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(jsonReader.TokenType, JsonToken.String);
            Xunit.Assert.Equal(jsonReader.Value, "500 gigabyte hard drive");
            Xunit.Assert.Equal(5, lineInfo.LineNumber);
            Xunit.Assert.Equal(29, lineInfo.LinePosition);
            XUnitAssert.True(lineInfo.HasLineInfo());
            Xunit.Assert.Equal(o["Drives"][1], jsonReader.CurrentToken);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(jsonReader.TokenType, JsonToken.EndArray);
            Xunit.Assert.Equal(3, lineInfo.LineNumber);
            Xunit.Assert.Equal(11, lineInfo.LinePosition);
            XUnitAssert.True(lineInfo.HasLineInfo());
            Xunit.Assert.Equal(o["Drives"], jsonReader.CurrentToken);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(jsonReader.TokenType, JsonToken.EndObject);
            Xunit.Assert.Equal(1, lineInfo.LineNumber);
            Xunit.Assert.Equal(1, lineInfo.LinePosition);
            XUnitAssert.True(lineInfo.HasLineInfo());
            Xunit.Assert.Equal(o, jsonReader.CurrentToken);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(jsonReader.TokenType, JsonToken.None);
            Xunit.Assert.Equal(null, jsonReader.CurrentToken);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(jsonReader.TokenType, JsonToken.None);
            Xunit.Assert.Equal(null, jsonReader.CurrentToken);
        }
    }

    [Fact]
    public async Task ReadBytesAsync()
    {
        var data = Encoding.UTF8.GetBytes("Hello world!");

        var o =
            new JObject(
                new JProperty("Test1", data)
            );

        using (var jsonReader = new JTokenReader(o))
        {
            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(JsonToken.StartObject, jsonReader.TokenType);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
            Xunit.Assert.Equal("Test1", jsonReader.Value);

            var readBytes = await jsonReader.ReadAsBytesAsync();
            Xunit.Assert.Equal(data, readBytes);

            Xunit.Assert.True(await jsonReader.ReadAsync());
            Xunit.Assert.Equal(JsonToken.EndObject, jsonReader.TokenType);

            Xunit.Assert.False(await jsonReader.ReadAsync());
            Xunit.Assert.Equal(JsonToken.None, jsonReader.TokenType);
        }
    }

    [Fact]
    public async Task ReadBytesFailureAsync()
    {
        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
        {
            var o =
                new JObject(
                    new JProperty("Test1", 1)
                );

            using (var jsonReader = new JTokenReader(o))
            {
                await jsonReader.ReadAsync();
                Xunit.Assert.Equal(JsonToken.StartObject, jsonReader.TokenType);

                await jsonReader.ReadAsync();
                Xunit.Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
                Xunit.Assert.Equal("Test1", jsonReader.Value);

                await jsonReader.ReadAsBytesAsync();
            }
        }, "Error reading bytes. Unexpected token: Integer. Path 'Test1'.");
    }

    public class HasBytes
    {
        public byte[] Bytes { get; set; }
    }

    [Fact]
    public async Task ReadAsDecimalIntAsync()
    {
        var json = @"{""Name"":1}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsDecimalAsync();
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);
        Xunit.Assert.Equal(typeof(decimal), reader.ValueType);
        Xunit.Assert.Equal(1m, reader.Value);
    }

    [Fact]
    public async Task ReadAsInt32IntAsync()
    {
        var json = @"{""Name"":1}";

        var o = JObject.Parse(json);

        var reader = (JTokenReader)o.CreateReader();

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);
        Xunit.Assert.Equal(o, reader.CurrentToken);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal(o.Property("Name"), reader.CurrentToken);

        await reader.ReadAsInt32Async();
        Xunit.Assert.Equal(o["Name"], reader.CurrentToken);
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);
        Xunit.Assert.Equal(typeof(int), reader.ValueType);
        Xunit.Assert.Equal(1, reader.Value);
    }

    [Fact]
    public async Task ReadAsInt32BadStringAsync()
    {
        var json = @"{""Name"":""hi""}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsInt32Async(); }, "Could not convert string to integer: hi. Path 'Name', line 1, position 12.");
    }

    [Fact]
    public async Task ReadAsInt32BooleanAsync()
    {
        var json = @"{""Name"":true}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsInt32Async(); }, "Error reading integer. Unexpected token: Boolean. Path 'Name', line 1, position 12.");
    }

    [Fact]
    public async Task ReadAsDecimalStringAsync()
    {
        var json = @"{""Name"":""1.1""}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsDecimalAsync();
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);
        Xunit.Assert.Equal(typeof(decimal), reader.ValueType);
        Xunit.Assert.Equal(1.1m, reader.Value);
    }

    [Fact]
    public async Task ReadAsDecimalBadStringAsync()
    {
        var json = @"{""Name"":""blah""}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDecimalAsync(); }, "Could not convert string to decimal: blah. Path 'Name', line 1, position 14.");
    }

    [Fact]
    public async Task ReadAsDecimalBooleanAsync()
    {
        var json = @"{""Name"":true}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDecimalAsync(); }, "Error reading decimal. Unexpected token: Boolean. Path 'Name', line 1, position 12.");
    }

    [Fact]
    public async Task ReadAsDecimalNullAsync()
    {
        var json = @"{""Name"":null}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsDecimalAsync();
        Xunit.Assert.Equal(JsonToken.Null, reader.TokenType);
        Xunit.Assert.Equal(null, reader.ValueType);
        Xunit.Assert.Equal(null, reader.Value);
    }

    [Fact]
    public async Task InitialPath_PropertyBase_PropertyTokenAsync()
    {
        var o = new JObject
        {
            { "prop1", true }
        };

        var reader = new JTokenReader(o, "baseprop");

        Xunit.Assert.Equal("baseprop", reader.Path);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal("baseprop", reader.Path);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal("baseprop.prop1", reader.Path);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal("baseprop.prop1", reader.Path);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal("baseprop", reader.Path);

        Xunit.Assert.False(await reader.ReadAsync());
        Xunit.Assert.Equal("baseprop", reader.Path);
    }

    [Fact]
    public async Task InitialPath_ArrayBase_PropertyTokenAsync()
    {
        var o = new JObject
        {
            { "prop1", true }
        };

        var reader = new JTokenReader(o, "[0]");

        Xunit.Assert.Equal("[0]", reader.Path);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal("[0]", reader.Path);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal("[0].prop1", reader.Path);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal("[0].prop1", reader.Path);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal("[0]", reader.Path);

        Xunit.Assert.False(await reader.ReadAsync());
        Xunit.Assert.Equal("[0]", reader.Path);
    }

    [Fact]
    public async Task InitialPath_PropertyBase_ArrayTokenAsync()
    {
        var a = new JArray
        {
            1, 2
        };

        var reader = new JTokenReader(a, "baseprop");

        Xunit.Assert.Equal("baseprop", reader.Path);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal("baseprop", reader.Path);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal("baseprop[0]", reader.Path);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal("baseprop[1]", reader.Path);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal("baseprop", reader.Path);

        Xunit.Assert.False(await reader.ReadAsync());
        Xunit.Assert.Equal("baseprop", reader.Path);
    }

    [Fact]
    public async Task InitialPath_ArrayBase_ArrayTokenAsync()
    {
        var a = new JArray
        {
            1, 2
        };

        var reader = new JTokenReader(a, "[0]");

        Xunit.Assert.Equal("[0]", reader.Path);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal("[0]", reader.Path);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal("[0][0]", reader.Path);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal("[0][1]", reader.Path);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal("[0]", reader.Path);

        Xunit.Assert.False(await reader.ReadAsync());
        Xunit.Assert.Equal("[0]", reader.Path);
    }

    [Fact]
    public async Task ReadAsDouble_InvalidTokenAsync()
    {
        var a = new JArray
        {
            1, 2
        };

        var reader = new JTokenReader(a);

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(
            async () => { await reader.ReadAsDoubleAsync(); },
            "Error reading double. Unexpected token: StartArray. Path ''.");
    }

    [Fact]
    public async Task ReadAsBoolean_InvalidTokenAsync()
    {
        var a = new JArray
        {
            1, 2
        };

        var reader = new JTokenReader(a);

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(
            async () => { await reader.ReadAsBooleanAsync(); },
            "Error reading boolean. Unexpected token: StartArray. Path ''.");
    }

    [Fact]
    public async Task ReadAsDateTime_InvalidTokenAsync()
    {
        var a = new JArray
        {
            1, 2
        };

        var reader = new JTokenReader(a);

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(
            async () => { await reader.ReadAsDateTimeAsync(); },
            "Error reading date. Unexpected token: StartArray. Path ''.");
    }

    [Fact]
    public async Task ReadAsDateTimeOffset_InvalidTokenAsync()
    {
        var a = new JArray
        {
            1, 2
        };

        var reader = new JTokenReader(a);

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(
            async () => { await reader.ReadAsDateTimeOffsetAsync(); },
            "Error reading date. Unexpected token: StartArray. Path ''.");
    }

    [Fact]
    public async Task ReadAsDateTimeOffset_DateTimeAsync()
    {
        var v = new JValue(new DateTime(2001, 12, 12, 12, 12, 12, DateTimeKind.Utc));

        var reader = new JTokenReader(v);

        Xunit.Assert.Equal(new DateTimeOffset(2001, 12, 12, 12, 12, 12, TimeSpan.Zero), await reader.ReadAsDateTimeOffsetAsync());
    }

    [Fact]
    public async Task ReadAsDateTimeOffset_StringAsync()
    {
        var v = new JValue("2012-01-24T03:50Z");

        var reader = new JTokenReader(v);

        Xunit.Assert.Equal(new DateTimeOffset(2012, 1, 24, 3, 50, 0, TimeSpan.Zero), await reader.ReadAsDateTimeOffsetAsync());
    }

    [Fact]
    public async Task ReadAsDateTime_DateTimeOffsetAsync()
    {
        var v = new JValue(new DateTimeOffset(2012, 1, 24, 3, 50, 0, TimeSpan.Zero));

        var reader = new JTokenReader(v);

        Xunit.Assert.Equal(new DateTime(2012, 1, 24, 3, 50, 0, DateTimeKind.Utc), await reader.ReadAsDateTimeAsync());
    }

    [Fact]
    public async Task ReadAsDateTime_StringAsync()
    {
        var v = new JValue("2012-01-24T03:50Z");

        var reader = new JTokenReader(v);

        Xunit.Assert.Equal(new DateTime(2012, 1, 24, 3, 50, 0, DateTimeKind.Utc), await reader.ReadAsDateTimeAsync());
    }

    [Fact]
    public async Task ReadAsDouble_String_SuccessAsync()
    {
        var s = JValue.CreateString("123.4");

        var reader = new JTokenReader(s);

        Xunit.Assert.Equal(123.4d, await reader.ReadAsDoubleAsync());
    }

    [Fact]
    public async Task ReadAsDouble_Null_SuccessAsync()
    {
        var n = JValue.CreateNull();

        var reader = new JTokenReader(n);

        Xunit.Assert.Equal(null, await reader.ReadAsDoubleAsync());
    }

    [Fact]
    public async Task ReadAsDouble_Integer_SuccessAsync()
    {
        var n = new JValue(1);

        var reader = new JTokenReader(n);

        Xunit.Assert.Equal(1d, await reader.ReadAsDoubleAsync());
    }

    [Fact]
    public async Task ReadAsBoolean_BigInteger_SuccessAsync()
    {
        var s = new JValue(BigInteger.Parse("99999999999999999999999999999999999999999999999999999999999999999999999999"));

        var reader = new JTokenReader(s);

        XUnitAssert.True(await reader.ReadAsBooleanAsync());
    }

    [Fact]
    public async Task ReadAsBoolean_String_SuccessAsync()
    {
        var s = JValue.CreateString("true");

        var reader = new JTokenReader(s);

        XUnitAssert.True(await reader.ReadAsBooleanAsync());
    }

    [Fact]
    public async Task ReadAsBoolean_Null_SuccessAsync()
    {
        var n = JValue.CreateNull();

        var reader = new JTokenReader(n);

        Xunit.Assert.Equal(null, await reader.ReadAsBooleanAsync());
    }

    [Fact]
    public async Task ReadAsBoolean_Integer_SuccessAsync()
    {
        var n = new JValue(1);

        var reader = new JTokenReader(n);

        XUnitAssert.True(await reader.ReadAsBooleanAsync());
    }

    [Fact]
    public async Task ReadAsDateTime_Null_SuccessAsync()
    {
        var n = JValue.CreateNull();

        var reader = new JTokenReader(n);

        Xunit.Assert.Equal(null, await reader.ReadAsDateTimeAsync());
    }

    [Fact]
    public async Task ReadAsDateTimeOffset_Null_SuccessAsync()
    {
        var n = JValue.CreateNull();

        var reader = new JTokenReader(n);

        Xunit.Assert.Equal(null, await reader.ReadAsDateTimeOffsetAsync());
    }

    [Fact]
    public async Task ReadAsString_Integer_SuccessAsync()
    {
        var n = new JValue(1);

        var reader = new JTokenReader(n);

        Xunit.Assert.Equal("1", await reader.ReadAsStringAsync());
    }

    [Fact]
    public async Task ReadAsString_Guid_SuccessAsync()
    {
        var n = new JValue(new Uri("http://www.test.com"));

        var reader = new JTokenReader(n);

        Xunit.Assert.Equal("http://www.test.com", await reader.ReadAsStringAsync());
    }

    [Fact]
    public async Task ReadAsBytes_Integer_SuccessAsync()
    {
        var n = JValue.CreateNull();

        var reader = new JTokenReader(n);

        Xunit.Assert.Equal(null, await reader.ReadAsBytesAsync());
    }

    [Fact]
    public async Task ReadAsBytes_ArrayAsync()
    {
        var a = new JArray
        {
            1, 2
        };

        var reader = new JTokenReader(a);

        var bytes = await reader.ReadAsBytesAsync();

        Xunit.Assert.Equal(2, bytes.Length);
        Xunit.Assert.Equal(1, bytes[0]);
        Xunit.Assert.Equal(2, bytes[1]);
    }

    [Fact]
    public async Task ReadAsBytes_NullAsync()
    {
        var n = JValue.CreateNull();

        var reader = new JTokenReader(n);

        Xunit.Assert.Equal(null, await reader.ReadAsBytesAsync());
    }
}