// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class JTokenReaderAsyncTests : TestFixtureBase
{
    [Fact]
    public async Task ConvertBigIntegerToDoubleAsync()
    {
        var jObject = JObject.Parse("{ maxValue:10000000000000000000}");

        var reader = jObject.CreateReader();
        Assert.True(await reader.ReadAsync());
        Assert.True(await reader.ReadAsync());
        Assert.Equal(10000000000000000000d, await reader.ReadAsDoubleAsync());
        Assert.True(await reader.ReadAsync());
    }

    [Fact]
    public async Task YahooFinanceAsync()
    {
        var o =
            new JObject(
                new JProperty("Test1", new DateTime(2000, 10, 15, 5, 5, 5, DateTimeKind.Utc)),
                new JProperty("Test2", new DateTimeOffset(2000, 10, 15, 5, 5, 5, new(11, 11, 0))),
                new JProperty("Test3", "Test3Value"),
                new JProperty("Test4", null)
            );

        using (var jsonReader = new JTokenReader(o))
        {
            IJsonLineInfo lineInfo = jsonReader;

            await jsonReader.ReadAsync();
            Assert.Equal(JsonToken.StartObject, jsonReader.TokenType);
            XUnitAssert.False(lineInfo.HasLineInfo());

            await jsonReader.ReadAsync();
            Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
            Assert.Equal("Test1", jsonReader.Value);
            XUnitAssert.False(lineInfo.HasLineInfo());

            await jsonReader.ReadAsync();
            Assert.Equal(JsonToken.Date, jsonReader.TokenType);
            Assert.Equal(new DateTime(2000, 10, 15, 5, 5, 5, DateTimeKind.Utc), jsonReader.Value);
            XUnitAssert.False(lineInfo.HasLineInfo());
            Assert.Equal(0, lineInfo.LinePosition);
            Assert.Equal(0, lineInfo.LineNumber);

            await jsonReader.ReadAsync();
            Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
            Assert.Equal("Test2", jsonReader.Value);

            await jsonReader.ReadAsync();
            Assert.Equal(JsonToken.Date, jsonReader.TokenType);
            Assert.Equal(new DateTimeOffset(2000, 10, 15, 5, 5, 5, new(11, 11, 0)), jsonReader.Value);

            await jsonReader.ReadAsync();
            Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
            Assert.Equal("Test3", jsonReader.Value);

            await jsonReader.ReadAsync();
            Assert.Equal(JsonToken.String, jsonReader.TokenType);
            Assert.Equal("Test3Value", jsonReader.Value);

            await jsonReader.ReadAsync();
            Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
            Assert.Equal("Test4", jsonReader.Value);

            await jsonReader.ReadAsync();
            Assert.Equal(JsonToken.Null, jsonReader.TokenType);
            Assert.Equal(null, jsonReader.Value);

            Assert.True(await jsonReader.ReadAsync());
            Assert.Equal(JsonToken.EndObject, jsonReader.TokenType);

            Assert.False(await jsonReader.ReadAsync());
            Assert.Equal(JsonToken.None, jsonReader.TokenType);
        }

        using (JsonReader jsonReader = new JTokenReader(o.Property("Test2")))
        {
            Assert.True(await jsonReader.ReadAsync());
            Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
            Assert.Equal("Test2", jsonReader.Value);

            Assert.True(await jsonReader.ReadAsync());
            Assert.Equal(JsonToken.Date, jsonReader.TokenType);
            Assert.Equal(new DateTimeOffset(2000, 10, 15, 5, 5, 5, new(11, 11, 0)), jsonReader.Value);

            Assert.False(await jsonReader.ReadAsync());
            Assert.Equal(JsonToken.None, jsonReader.TokenType);
        }
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetBadStringAsync()
    {
        var json = @"{""Offset"":""blablahbla""}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsDateTimeOffsetAsync(),
            "Could not convert string to DateTimeOffset: blablahbla. Path 'Offset', line 1, position 22.");
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetBooleanAsync()
    {
        var json = @"{""Offset"":true}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsDateTimeOffsetAsync(),
            "Error reading date. Unexpected token: Boolean. Path 'Offset', line 1, position 14.");
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetStringAsync()
    {
        var json = @"{""Offset"":""2012-01-24T03:50Z""}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsDateTimeOffsetAsync();
        Assert.Equal(JsonToken.Date, reader.TokenType);
        Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        Assert.Equal(new DateTimeOffset(2012, 1, 24, 3, 50, 0, TimeSpan.Zero), reader.Value);
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

        using var jsonReader = new JTokenReader(o);
        IJsonLineInfo lineInfo = jsonReader;

        Assert.Equal(jsonReader.TokenType, JsonToken.None);
        Assert.Equal(0, lineInfo.LineNumber);
        Assert.Equal(0, lineInfo.LinePosition);
        XUnitAssert.False(lineInfo.HasLineInfo());
        Assert.Equal(null, jsonReader.CurrentToken);

        await jsonReader.ReadAsync();
        Assert.Equal(jsonReader.TokenType, JsonToken.StartObject);
        Assert.Equal(1, lineInfo.LineNumber);
        Assert.Equal(1, lineInfo.LinePosition);
        XUnitAssert.True(lineInfo.HasLineInfo());
        Assert.Equal(o, jsonReader.CurrentToken);

        await jsonReader.ReadAsync();
        Assert.Equal(jsonReader.TokenType, JsonToken.PropertyName);
        Assert.Equal(jsonReader.Value, "CPU");
        Assert.Equal(2, lineInfo.LineNumber);
        Assert.Equal(6, lineInfo.LinePosition);
        XUnitAssert.True(lineInfo.HasLineInfo());
        Assert.Equal(o.Property("CPU"), jsonReader.CurrentToken);

        await jsonReader.ReadAsync();
        Assert.Equal(jsonReader.TokenType, JsonToken.String);
        Assert.Equal(jsonReader.Value, "Intel");
        Assert.Equal(2, lineInfo.LineNumber);
        Assert.Equal(14, lineInfo.LinePosition);
        XUnitAssert.True(lineInfo.HasLineInfo());
        Assert.Equal(o.Property("CPU").Value, jsonReader.CurrentToken);

        await jsonReader.ReadAsync();
        Assert.Equal(jsonReader.TokenType, JsonToken.PropertyName);
        Assert.Equal(jsonReader.Value, "Drives");
        Assert.Equal(3, lineInfo.LineNumber);
        Assert.Equal(9, lineInfo.LinePosition);
        XUnitAssert.True(lineInfo.HasLineInfo());
        Assert.Equal(o.Property("Drives"), jsonReader.CurrentToken);

        await jsonReader.ReadAsync();
        Assert.Equal(jsonReader.TokenType, JsonToken.StartArray);
        Assert.Equal(3, lineInfo.LineNumber);
        Assert.Equal(11, lineInfo.LinePosition);
        XUnitAssert.True(lineInfo.HasLineInfo());
        Assert.Equal(o.Property("Drives").Value, jsonReader.CurrentToken);

        await jsonReader.ReadAsync();
        Assert.Equal(jsonReader.TokenType, JsonToken.String);
        Assert.Equal(jsonReader.Value, "DVD read/writer");
        Assert.Equal(4, lineInfo.LineNumber);
        Assert.Equal(21, lineInfo.LinePosition);
        XUnitAssert.True(lineInfo.HasLineInfo());
        Assert.Equal(o["Drives"][0], jsonReader.CurrentToken);

        await jsonReader.ReadAsync();
        Assert.Equal(jsonReader.TokenType, JsonToken.String);
        Assert.Equal(jsonReader.Value, "500 gigabyte hard drive");
        Assert.Equal(5, lineInfo.LineNumber);
        Assert.Equal(29, lineInfo.LinePosition);
        XUnitAssert.True(lineInfo.HasLineInfo());
        Assert.Equal(o["Drives"][1], jsonReader.CurrentToken);

        await jsonReader.ReadAsync();
        Assert.Equal(jsonReader.TokenType, JsonToken.EndArray);
        Assert.Equal(3, lineInfo.LineNumber);
        Assert.Equal(11, lineInfo.LinePosition);
        XUnitAssert.True(lineInfo.HasLineInfo());
        Assert.Equal(o["Drives"], jsonReader.CurrentToken);

        await jsonReader.ReadAsync();
        Assert.Equal(jsonReader.TokenType, JsonToken.EndObject);
        Assert.Equal(1, lineInfo.LineNumber);
        Assert.Equal(1, lineInfo.LinePosition);
        XUnitAssert.True(lineInfo.HasLineInfo());
        Assert.Equal(o, jsonReader.CurrentToken);

        await jsonReader.ReadAsync();
        Assert.Equal(jsonReader.TokenType, JsonToken.None);
        Assert.Equal(null, jsonReader.CurrentToken);

        await jsonReader.ReadAsync();
        Assert.Equal(jsonReader.TokenType, JsonToken.None);
        Assert.Equal(null, jsonReader.CurrentToken);
    }

    [Fact]
    public async Task ReadBytesAsync()
    {
        var data = Encoding.UTF8.GetBytes("Hello world!");

        var o =
            new JObject(
                new JProperty("Test1", data)
            );

        using var jsonReader = new JTokenReader(o);
        await jsonReader.ReadAsync();
        Assert.Equal(JsonToken.StartObject, jsonReader.TokenType);

        await jsonReader.ReadAsync();
        Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
        Assert.Equal("Test1", jsonReader.Value);

        var readBytes = await jsonReader.ReadAsBytesAsync();
        Assert.Equal(data, readBytes);

        Assert.True(await jsonReader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, jsonReader.TokenType);

        Assert.False(await jsonReader.ReadAsync());
        Assert.Equal(JsonToken.None, jsonReader.TokenType);
    }

    [Fact]
    public async Task ReadBytesFailureAsync()
    {
            var o =
                new JObject(
                    new JProperty("Test1", 1)
                );

            using var jsonReader = new JTokenReader(o);
            await jsonReader.ReadAsync();
            Assert.Equal(JsonToken.StartObject, jsonReader.TokenType);

            await jsonReader.ReadAsync();
            Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
            Assert.Equal("Test1", jsonReader.Value);
        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => jsonReader.ReadAsBytesAsync(),
            "Error reading bytes. Unexpected token: Integer. Path 'Test1'.");
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

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsDecimalAsync();
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(typeof(decimal), reader.ValueType);
        Assert.Equal(1m, reader.Value);
    }

    [Fact]
    public async Task ReadAsInt32IntAsync()
    {
        var json = @"{""Name"":1}";

        var o = JObject.Parse(json);

        var reader = (JTokenReader) o.CreateReader();

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);
        Assert.Equal(o, reader.CurrentToken);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal(o.Property("Name"), reader.CurrentToken);

        await reader.ReadAsInt32Async();
        Assert.Equal(o["Name"], reader.CurrentToken);
        Assert.Equal(JsonToken.Integer, reader.TokenType);
        Assert.Equal(typeof(int), reader.ValueType);
        Assert.Equal(1, reader.Value);
    }

    [Fact]
    public async Task ReadAsInt32BadStringAsync()
    {
        var json = @"{""Name"":""hi""}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsInt32Async(),
            "Could not convert string to integer: hi. Path 'Name', line 1, position 12.");
    }

    [Fact]
    public async Task ReadAsInt32BooleanAsync()
    {
        var json = @"{""Name"":true}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsInt32Async(),
            "Error reading integer. Unexpected token: Boolean. Path 'Name', line 1, position 12.");
    }

    [Fact]
    public async Task ReadAsDecimalStringAsync()
    {
        var json = @"{""Name"":""1.1""}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsDecimalAsync();
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(typeof(decimal), reader.ValueType);
        Assert.Equal(1.1m, reader.Value);
    }

    [Fact]
    public async Task ReadAsDecimalBadStringAsync()
    {
        var json = @"{""Name"":""blah""}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsDecimalAsync(),
            "Could not convert string to decimal: blah. Path 'Name', line 1, position 14.");
    }

    [Fact]
    public async Task ReadAsDecimalBooleanAsync()
    {
        var json = @"{""Name"":true}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsDecimalAsync(),
            "Error reading decimal. Unexpected token: Boolean. Path 'Name', line 1, position 12.");
    }

    [Fact]
    public async Task ReadAsDecimalNullAsync()
    {
        var json = @"{""Name"":null}";

        var o = JObject.Parse(json);

        var reader = o.CreateReader();

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsDecimalAsync();
        Assert.Equal(JsonToken.Null, reader.TokenType);
        Assert.Equal(null, reader.ValueType);
        Assert.Equal(null, reader.Value);
    }

    [Fact]
    public async Task InitialPath_PropertyBase_PropertyTokenAsync()
    {
        var o = new JObject
        {
            {"prop1", true}
        };

        var reader = new JTokenReader(o, "baseprop");

        Assert.Equal("baseprop", reader.Path);

        Assert.True(await reader.ReadAsync());
        Assert.Equal("baseprop", reader.Path);

        Assert.True(await reader.ReadAsync());
        Assert.Equal("baseprop.prop1", reader.Path);

        Assert.True(await reader.ReadAsync());
        Assert.Equal("baseprop.prop1", reader.Path);

        Assert.True(await reader.ReadAsync());
        Assert.Equal("baseprop", reader.Path);

        Assert.False(await reader.ReadAsync());
        Assert.Equal("baseprop", reader.Path);
    }

    [Fact]
    public async Task InitialPath_ArrayBase_PropertyTokenAsync()
    {
        var o = new JObject
        {
            {"prop1", true}
        };

        var reader = new JTokenReader(o, "[0]");

        Assert.Equal("[0]", reader.Path);

        Assert.True(await reader.ReadAsync());
        Assert.Equal("[0]", reader.Path);

        Assert.True(await reader.ReadAsync());
        Assert.Equal("[0].prop1", reader.Path);

        Assert.True(await reader.ReadAsync());
        Assert.Equal("[0].prop1", reader.Path);

        Assert.True(await reader.ReadAsync());
        Assert.Equal("[0]", reader.Path);

        Assert.False(await reader.ReadAsync());
        Assert.Equal("[0]", reader.Path);
    }

    [Fact]
    public async Task InitialPath_PropertyBase_ArrayTokenAsync()
    {
        var a = new JArray
        {
            1, 2
        };

        var reader = new JTokenReader(a, "baseprop");

        Assert.Equal("baseprop", reader.Path);

        Assert.True(await reader.ReadAsync());
        Assert.Equal("baseprop", reader.Path);

        Assert.True(await reader.ReadAsync());
        Assert.Equal("baseprop[0]", reader.Path);

        Assert.True(await reader.ReadAsync());
        Assert.Equal("baseprop[1]", reader.Path);

        Assert.True(await reader.ReadAsync());
        Assert.Equal("baseprop", reader.Path);

        Assert.False(await reader.ReadAsync());
        Assert.Equal("baseprop", reader.Path);
    }

    [Fact]
    public async Task InitialPath_ArrayBase_ArrayTokenAsync()
    {
        var a = new JArray
        {
            1, 2
        };

        var reader = new JTokenReader(a, "[0]");

        Assert.Equal("[0]", reader.Path);

        Assert.True(await reader.ReadAsync());
        Assert.Equal("[0]", reader.Path);

        Assert.True(await reader.ReadAsync());
        Assert.Equal("[0][0]", reader.Path);

        Assert.True(await reader.ReadAsync());
        Assert.Equal("[0][1]", reader.Path);

        Assert.True(await reader.ReadAsync());
        Assert.Equal("[0]", reader.Path);

        Assert.False(await reader.ReadAsync());
        Assert.Equal("[0]", reader.Path);
    }

    [Fact]
    public async Task ReadAsDouble_InvalidTokenAsync()
    {
        var a = new JArray
        {
            1, 2
        };

        var reader = new JTokenReader(a);

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            async () =>
            {
                await reader.ReadAsDoubleAsync();
            },
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

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            async () =>
            {
                await reader.ReadAsBooleanAsync();
            },
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

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            async () =>
            {
                await reader.ReadAsDateTimeAsync();
            },
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

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            async () =>
            {
                await reader.ReadAsDateTimeOffsetAsync();
            },
            "Error reading date. Unexpected token: StartArray. Path ''.");
    }

    [Fact]
    public async Task ReadAsDateTimeOffset_DateTimeAsync()
    {
        var v = new JValue(new DateTime(2001, 12, 12, 12, 12, 12, DateTimeKind.Utc));

        var reader = new JTokenReader(v);

        Assert.Equal(new DateTimeOffset(2001, 12, 12, 12, 12, 12, TimeSpan.Zero), await reader.ReadAsDateTimeOffsetAsync());
    }

    [Fact]
    public async Task ReadAsDateTimeOffset_StringAsync()
    {
        var v = new JValue("2012-01-24T03:50Z");

        var reader = new JTokenReader(v);

        Assert.Equal(new DateTimeOffset(2012, 1, 24, 3, 50, 0, TimeSpan.Zero), await reader.ReadAsDateTimeOffsetAsync());
    }

    [Fact]
    public async Task ReadAsDateTime_DateTimeOffsetAsync()
    {
        var v = new JValue(new DateTimeOffset(2012, 1, 24, 3, 50, 0, TimeSpan.Zero));

        var reader = new JTokenReader(v);

        Assert.Equal(new DateTime(2012, 1, 24, 3, 50, 0, DateTimeKind.Utc), await reader.ReadAsDateTimeAsync());
    }

    [Fact]
    public async Task ReadAsDateTime_StringAsync()
    {
        var v = new JValue("2012-01-24T03:50Z");

        var reader = new JTokenReader(v);

        Assert.Equal(new DateTime(2012, 1, 24, 3, 50, 0, DateTimeKind.Utc), await reader.ReadAsDateTimeAsync());
    }

    [Fact]
    public async Task ReadAsDouble_String_SuccessAsync()
    {
        var s = JValue.CreateString("123.4");

        var reader = new JTokenReader(s);

        Assert.Equal(123.4d, await reader.ReadAsDoubleAsync());
    }

    [Fact]
    public async Task ReadAsDouble_Null_SuccessAsync()
    {
        var n = JValue.CreateNull();

        var reader = new JTokenReader(n);

        Assert.Equal(null, await reader.ReadAsDoubleAsync());
    }

    [Fact]
    public async Task ReadAsDouble_Integer_SuccessAsync()
    {
        var n = new JValue(1);

        var reader = new JTokenReader(n);

        Assert.Equal(1d, await reader.ReadAsDoubleAsync());
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

        Assert.Equal(null, await reader.ReadAsBooleanAsync());
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

        Assert.Equal(null, await reader.ReadAsDateTimeAsync());
    }

    [Fact]
    public async Task ReadAsDateTimeOffset_Null_SuccessAsync()
    {
        var n = JValue.CreateNull();

        var reader = new JTokenReader(n);

        Assert.Equal(null, await reader.ReadAsDateTimeOffsetAsync());
    }

    [Fact]
    public async Task ReadAsString_Integer_SuccessAsync()
    {
        var n = new JValue(1);

        var reader = new JTokenReader(n);

        Assert.Equal("1", await reader.ReadAsStringAsync());
    }

    [Fact]
    public async Task ReadAsString_Guid_SuccessAsync()
    {
        var n = new JValue(new Uri("http://www.test.com"));

        var reader = new JTokenReader(n);

        Assert.Equal("http://www.test.com", await reader.ReadAsStringAsync());
    }

    [Fact]
    public async Task ReadAsBytes_Integer_SuccessAsync()
    {
        var n = JValue.CreateNull();

        var reader = new JTokenReader(n);

        Assert.Equal(null, await reader.ReadAsBytesAsync());
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

        Assert.Equal(2, bytes.Length);
        Assert.Equal(1, bytes[0]);
        Assert.Equal(2, bytes[1]);
    }

    [Fact]
    public async Task ReadAsBytes_NullAsync()
    {
        var n = JValue.CreateNull();

        var reader = new JTokenReader(n);

        Assert.Equal(null, await reader.ReadAsBytesAsync());
    }
}