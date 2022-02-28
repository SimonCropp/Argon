// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class ParseAsyncTests : TestFixtureBase
{
    [Fact]
    public async Task ParseAdditionalContent_WhitespaceAsync()
    {
        var json = @"[
""Small"",
""Medium"",
""Large""
]   

";

        var reader = new JsonTextReader(new StringReader(json));
        while (await reader.ReadAsync())
        {
        }
    }

    [Fact]
    public async Task ParsingQuotedPropertyWithControlCharactersAsync()
    {
        JsonReader reader = new JsonTextReader(new StringReader(@"{'hi\r\nbye':1}"));
        Assert.True(await reader.ReadAsync());
        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("hi\r\nbye", reader.Value);
        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Integer, reader.TokenType);
        Assert.Equal(1L, reader.Value);
        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);
        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ParseIntegersAsync()
    {
        var reader = new JsonTextReader(new StringReader("1"));
        Assert.Equal(1, await reader.ReadAsInt32Async());

        reader = new JsonTextReader(new StringReader("-1"));
        Assert.Equal(-1, await reader.ReadAsInt32Async());

        reader = new JsonTextReader(new StringReader("0"));
        Assert.Equal(0, await reader.ReadAsInt32Async());

        reader = new JsonTextReader(new StringReader("-0"));
        Assert.Equal(0, await reader.ReadAsInt32Async());

        reader = new JsonTextReader(new StringReader(int.MaxValue.ToString()));
        Assert.Equal(int.MaxValue, await reader.ReadAsInt32Async());

        reader = new JsonTextReader(new StringReader(int.MinValue.ToString()));
        Assert.Equal(int.MinValue, await reader.ReadAsInt32Async());

        reader = new JsonTextReader(new StringReader(long.MaxValue.ToString()));
        await XUnitAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsInt32Async(), "JSON integer 9223372036854775807 is too large or small for an Int32. Path '', line 1, position 19.");

        reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
        await XUnitAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsInt32Async(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

        reader = new JsonTextReader(new StringReader("1E-06"));
        await XUnitAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsInt32Async(), "Input string '1E-06' is not a valid integer. Path '', line 1, position 5.");

        reader = new JsonTextReader(new StringReader("1.1"));
        await XUnitAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsInt32Async(), "Input string '1.1' is not a valid integer. Path '', line 1, position 3.");

        reader = new JsonTextReader(new StringReader(""));
        Assert.Equal(null, await reader.ReadAsInt32Async());

        reader = new JsonTextReader(new StringReader("-"));
        await XUnitAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsInt32Async(), "Input string '-' is not a valid integer. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ParseDecimalsAsync()
    {
        var reader = new JsonTextReader(new StringReader("1.1"));
        Assert.Equal(1.1m, await reader.ReadAsDecimalAsync());

        reader = new JsonTextReader(new StringReader("-1.1"));
        Assert.Equal(-1.1m, await reader.ReadAsDecimalAsync());

        reader = new JsonTextReader(new StringReader("0.0"));
        Assert.Equal(0.0m, await reader.ReadAsDecimalAsync());

        reader = new JsonTextReader(new StringReader("-0.0"));
        Assert.Equal(0, await reader.ReadAsDecimalAsync());

        reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
        await XUnitAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsDecimalAsync(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

        reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
        reader.FloatParseHandling = FloatParseHandling.Decimal;
        await XUnitAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

        reader = new JsonTextReader(new StringReader("1E-06"));
        Assert.Equal(0.000001m, await reader.ReadAsDecimalAsync());

        reader = new JsonTextReader(new StringReader(""));
        Assert.Equal(null, await reader.ReadAsDecimalAsync());

        reader = new JsonTextReader(new StringReader("-"));
        await XUnitAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsDecimalAsync(), "Input string '-' is not a valid decimal. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ParseDoublesAsync()
    {
        var reader = new JsonTextReader(new StringReader("1.1"));
        Assert.True(await reader.ReadAsync());
        Assert.Equal(typeof(double), reader.ValueType);
        Assert.Equal(1.1d, reader.Value);

        reader = new JsonTextReader(new StringReader("-1.1"));
        Assert.True(await reader.ReadAsync());
        Assert.Equal(typeof(double), reader.ValueType);
        Assert.Equal(-1.1d, reader.Value);

        reader = new JsonTextReader(new StringReader("0.0"));
        Assert.True(await reader.ReadAsync());
        Assert.Equal(typeof(double), reader.ValueType);
        Assert.Equal(0.0d, reader.Value);

        reader = new JsonTextReader(new StringReader("-0.0"));
        Assert.True(await reader.ReadAsync());
        Assert.Equal(typeof(double), reader.ValueType);
        Assert.Equal(-0.0d, reader.Value);

        reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
        await XUnitAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

        reader = new JsonTextReader(new StringReader("1E-06"));
        Assert.True(await reader.ReadAsync());
        Assert.Equal(typeof(double), reader.ValueType);
        Assert.Equal(0.000001d, reader.Value);

        reader = new JsonTextReader(new StringReader(""));
        Assert.False(await reader.ReadAsync());

        reader = new JsonTextReader(new StringReader("-"));
        await XUnitAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Input string '-' is not a valid number. Path '', line 1, position 1.");

        reader = new JsonTextReader(new StringReader("1.7976931348623157E+308"));
        Assert.True(await reader.ReadAsync());
        Assert.Equal(typeof(double), reader.ValueType);
        Assert.Equal(Double.MaxValue, reader.Value);

        reader = new JsonTextReader(new StringReader("-1.7976931348623157E+308"));
        Assert.True(await reader.ReadAsync());
        Assert.Equal(typeof(double), reader.ValueType);
        Assert.Equal(Double.MinValue, reader.Value);

        reader = new JsonTextReader(new StringReader("1E+309"));
#if !(NET5_0_OR_GREATER)
        await XUnitAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Input string '1E+309' is not a valid number. Path '', line 1, position 6.");
#else
            Assert.True(await reader.ReadAsync());
            Assert.Equal(typeof(double), reader.ValueType);
            Assert.Equal(Double.PositiveInfinity, reader.Value);
#endif

        reader = new JsonTextReader(new StringReader("-1E+5000"));
#if !(NET5_0_OR_GREATER)
        await XUnitAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Input string '-1E+5000' is not a valid number. Path '', line 1, position 8.");
#else
            Assert.True(await reader.ReadAsync());
            Assert.Equal(typeof(double), reader.ValueType);
            Assert.Equal(Double.NegativeInfinity, reader.Value);
#endif

        reader = new JsonTextReader(new StringReader("5.1231231E"));
        await XUnitAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Input string '5.1231231E' is not a valid number. Path '', line 1, position 10.");

        reader = new JsonTextReader(new StringReader("1E-23"));
        Assert.True(await reader.ReadAsync());
        Assert.Equal(typeof(double), reader.ValueType);
        Assert.Equal(1e-23, reader.Value);
    }

    [Fact]
    public async Task ParseArrayWithMissingValuesAsync()
    {
        var json = "[,,, \n\r\n \0   \r  , ,    ]";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Undefined, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Undefined, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Undefined, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Undefined, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Undefined, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);
    }

    [Fact]
    public async Task ParseBooleanWithNoExtraContentAsync()
    {
        var json = "[true ";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.True(await reader.ReadAsync());
        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ParseContentDelimitedByNonStandardWhitespaceAsync()
    {
        var json = "\x00a0{\x00a0'h\x00a0i\x00a0'\x00a0:\x00a0[\x00a0true\x00a0,\x00a0new\x00a0Date\x00a0(\x00a0)\x00a0]\x00a0/*\x00a0comment\x00a0*/\x00a0}\x00a0";
        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        await reader.VerifyReaderState();
    }

    [Fact]
    public async Task ParseObjectWithNoEndAsync()
    {
        var json = "{hi:1, ";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.True(await reader.ReadAsync());
        Assert.True(await reader.ReadAsync());
        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ParseEmptyArrayAsync()
    {
        var json = "[]";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);
    }

    [Fact]
    public async Task ParseEmptyObjectAsync()
    {
        var json = "{}";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public async Task ParseEmptyConstructorAsync()
    {
        var json = "new Date()";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartConstructor, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndConstructor, reader.TokenType);
    }

    [Fact]
    public async Task ParseHexNumberAsync()
    {
        var json = @"0x20";

        var reader = new JsonTextReader(new StringReader(json));

        await reader.ReadAsDecimalAsync();
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(32m, reader.Value);
    }

    [Fact]
    public async Task ParseNumbersAsync()
    {
        var json = @"[0,1,2 , 3]";

        var reader = new JsonTextReader(new StringReader(json));

        await reader.ReadAsync();
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        await reader.ReadAsync();
        Assert.Equal(JsonToken.Integer, reader.TokenType);

        await reader.ReadAsync();
        Assert.Equal(JsonToken.Integer, reader.TokenType);

        await reader.ReadAsync();
        Assert.Equal(JsonToken.Integer, reader.TokenType);

        await reader.ReadAsync();
        Assert.Equal(JsonToken.Integer, reader.TokenType);

        await reader.ReadAsync();
        Assert.Equal(JsonToken.EndArray, reader.TokenType);
    }

 

    [Fact]
    public async Task ParseOctalNumberAsync()
    {
        var json = @"010";

        var reader = new JsonTextReader(new StringReader(json));

        await reader.ReadAsDecimalAsync();
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(8m, reader.Value);
    }

    [Fact]
    public async Task DateParseHandlingAsync()
    {
        var json = @"['1970-01-01T00:00:00Z']";

        var reader = new JsonTextReader(new StringReader(json));
        reader.DateParseHandling = DateParseHandling.DateTime;

        Assert.True(await reader.ReadAsync());
        Assert.True(await reader.ReadAsync());
        Assert.Equal(new DateTime(ParseTests.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
        Assert.Equal(typeof(DateTime), reader.ValueType);

        reader = new JsonTextReader(new StringReader(json));
        reader.DateParseHandling = DateParseHandling.DateTimeOffset;

        Assert.True(await reader.ReadAsync());
        Assert.True(await reader.ReadAsync());
        Assert.Equal(new DateTimeOffset(ParseTests.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
        Assert.Equal(typeof(DateTimeOffset), reader.ValueType);

        reader = new JsonTextReader(new StringReader(json));
        reader.DateParseHandling = DateParseHandling.None;

        Assert.True(await reader.ReadAsync());
        Assert.True(await reader.ReadAsync());
        Assert.Equal(@"1970-01-01T00:00:00Z", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);

        reader = new JsonTextReader(new StringReader(json));
        reader.DateParseHandling = DateParseHandling.DateTime;

        Assert.True(await reader.ReadAsync());
        await reader.ReadAsDateTimeOffsetAsync();
        Assert.Equal(new DateTimeOffset(ParseTests.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
        Assert.Equal(typeof(DateTimeOffset), reader.ValueType);

        reader = new JsonTextReader(new StringReader(json));
        reader.DateParseHandling = DateParseHandling.DateTimeOffset;

        Assert.True(await reader.ReadAsync());
        await reader.ReadAsDateTimeAsync();
        Assert.Equal(new DateTime(ParseTests.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
        Assert.Equal(typeof(DateTime), reader.ValueType);
    }
}