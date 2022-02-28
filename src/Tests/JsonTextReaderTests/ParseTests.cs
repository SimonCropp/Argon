// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class ParseTests : TestFixtureBase
{
    [Fact]
    public void ParseAdditionalContent_Whitespace()
    {
        var json = @"[
""Small"",
""Medium"",
""Large""
]   

";

        var reader = new JsonTextReader(new StringReader(json));
        while (reader.Read())
        {
        }
    }

    [Fact]
    public void ParsingQuotedPropertyWithControlCharacters()
    {
        JsonReader reader = new JsonTextReader(new StringReader(@"{'hi\r\nbye':1}"));
        Assert.True(reader.Read());
        Assert.True(reader.Read());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("hi\r\nbye", reader.Value);
        Assert.True(reader.Read());
        Assert.Equal(JsonToken.Integer, reader.TokenType);
        Assert.Equal(1L, reader.Value);
        Assert.True(reader.Read());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);
        Assert.False(reader.Read());
    }

    [Fact]
    public void ParseIntegers()
    {
        var reader = new JsonTextReader(new StringReader("1"));
        Assert.Equal(1, reader.ReadAsInt32());

        reader = new JsonTextReader(new StringReader("-1"));
        Assert.Equal(-1, reader.ReadAsInt32());

        reader = new JsonTextReader(new StringReader("0"));
        Assert.Equal(0, reader.ReadAsInt32());

        reader = new JsonTextReader(new StringReader("-0"));
        Assert.Equal(0, reader.ReadAsInt32());

        reader = new JsonTextReader(new StringReader(int.MaxValue.ToString()));
        Assert.Equal(int.MaxValue, reader.ReadAsInt32());

        reader = new JsonTextReader(new StringReader(int.MinValue.ToString()));
        Assert.Equal(int.MinValue, reader.ReadAsInt32());

        reader = new JsonTextReader(new StringReader(long.MaxValue.ToString()));
        XUnitAssert.Throws<JsonReaderException>(
            () => reader.ReadAsInt32(),
            "JSON integer 9223372036854775807 is too large or small for an Int32. Path '', line 1, position 19.");

        reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
        XUnitAssert.Throws<JsonReaderException>(
            () => reader.ReadAsInt32(),
            "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

        reader = new JsonTextReader(new StringReader("1E-06"));
        XUnitAssert.Throws<JsonReaderException>(
            () => reader.ReadAsInt32(),
            "Input string '1E-06' is not a valid integer. Path '', line 1, position 5.");

        reader = new JsonTextReader(new StringReader("1.1"));
        XUnitAssert.Throws<JsonReaderException>(
            () => reader.ReadAsInt32(),
            "Input string '1.1' is not a valid integer. Path '', line 1, position 3.");

        reader = new JsonTextReader(new StringReader(""));
        Assert.Equal(null, reader.ReadAsInt32());

        reader = new JsonTextReader(new StringReader("-"));
        XUnitAssert.Throws<JsonReaderException>(
            () => reader.ReadAsInt32(),
            "Input string '-' is not a valid integer. Path '', line 1, position 1.");
    }

    [Fact]
    public void ParseDecimals()
    {
        var reader = new JsonTextReader(new StringReader("1.1"));
        Assert.Equal(1.1m, reader.ReadAsDecimal());

        reader = new JsonTextReader(new StringReader("-1.1"));
        Assert.Equal(-1.1m, reader.ReadAsDecimal());

        reader = new JsonTextReader(new StringReader("0.0"));
        Assert.Equal(0.0m, reader.ReadAsDecimal());

        reader = new JsonTextReader(new StringReader("-0.0"));
        Assert.Equal(0, reader.ReadAsDecimal());

        reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
        XUnitAssert.Throws<JsonReaderException>(
            () => reader.ReadAsDecimal(),
            "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

        reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
        reader.FloatParseHandling = FloatParseHandling.Decimal;
        XUnitAssert.Throws<JsonReaderException>(
            () => reader.Read(),
            "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

        reader = new JsonTextReader(new StringReader("1E-06"));
        Assert.Equal(0.000001m, reader.ReadAsDecimal());

        reader = new JsonTextReader(new StringReader(""));
        Assert.Equal(null, reader.ReadAsDecimal());

        reader = new JsonTextReader(new StringReader("-"));
        XUnitAssert.Throws<JsonReaderException>(
            () => reader.ReadAsDecimal(),
            "Input string '-' is not a valid decimal. Path '', line 1, position 1.");
    }

    [Fact]
    public void ParseDoubles()
    {
        var reader = new JsonTextReader(new StringReader("1.1"));
        Assert.True(reader.Read());
        Assert.Equal(typeof(double), reader.ValueType);
        Assert.Equal(1.1d, reader.Value);

        reader = new JsonTextReader(new StringReader("-1.1"));
        Assert.True(reader.Read());
        Assert.Equal(typeof(double), reader.ValueType);
        Assert.Equal(-1.1d, reader.Value);

        reader = new JsonTextReader(new StringReader("0.0"));
        Assert.True(reader.Read());
        Assert.Equal(typeof(double), reader.ValueType);
        Assert.Equal(0.0d, reader.Value);

        reader = new JsonTextReader(new StringReader("-0.0"));
        Assert.True(reader.Read());
        Assert.Equal(typeof(double), reader.ValueType);
        Assert.Equal(-0.0d, reader.Value);

        reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
        XUnitAssert.Throws<JsonReaderException>(
            () => reader.Read(),
            "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

        reader = new JsonTextReader(new StringReader("1E-06"));
        Assert.True(reader.Read());
        Assert.Equal(typeof(double), reader.ValueType);
        Assert.Equal(0.000001d, reader.Value);

        reader = new JsonTextReader(new StringReader(""));
        Assert.False(reader.Read());

        reader = new JsonTextReader(new StringReader("-"));
        XUnitAssert.Throws<JsonReaderException>(
            () => reader.Read(),
            "Input string '-' is not a valid number. Path '', line 1, position 1.");

        reader = new JsonTextReader(new StringReader("1.7976931348623157E+308"));
        Assert.True(reader.Read());
        Assert.Equal(typeof(double), reader.ValueType);
        Assert.Equal(Double.MaxValue, reader.Value);

        reader = new JsonTextReader(new StringReader("-1.7976931348623157E+308"));
        Assert.True(reader.Read());
        Assert.Equal(typeof(double), reader.ValueType);
        Assert.Equal(Double.MinValue, reader.Value);

        reader = new JsonTextReader(new StringReader("1E+309"));
#if !(NET5_0_OR_GREATER)
        XUnitAssert.Throws<JsonReaderException>(
            () => reader.Read(),
            "Input string '1E+309' is not a valid number. Path '', line 1, position 6.");
#else
            Assert.True(reader.Read());
            Assert.Equal(typeof(double), reader.ValueType);
            Assert.Equal(Double.PositiveInfinity, reader.Value);
#endif

        reader = new JsonTextReader(new StringReader("-1E+5000"));
#if !(NET5_0_OR_GREATER)
        XUnitAssert.Throws<JsonReaderException>(
            () => reader.Read(),
            "Input string '-1E+5000' is not a valid number. Path '', line 1, position 8.");
#else
            Assert.True(reader.Read());
            Assert.Equal(typeof(double), reader.ValueType);
            Assert.Equal(Double.NegativeInfinity, reader.Value);
#endif

        reader = new JsonTextReader(new StringReader("5.1231231E"));
        XUnitAssert.Throws<JsonReaderException>(
            () => reader.Read(),
            "Input string '5.1231231E' is not a valid number. Path '', line 1, position 10.");

        reader = new JsonTextReader(new StringReader("1E-23"));
        Assert.True(reader.Read());
        Assert.Equal(typeof(double), reader.ValueType);
        Assert.Equal(1e-23, reader.Value);
    }

    [Fact]
    public void ParseArrayWithMissingValues()
    {
        var json = "[,,, \n\r\n \0   \r  , ,    ]";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.Undefined, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.Undefined, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.Undefined, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.Undefined, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.Undefined, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);
    }

    [Fact]
    public void ParseBooleanWithNoExtraContent()
    {
        var json = "[true ";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(reader.Read());
        Assert.True(reader.Read());
        Assert.False(reader.Read());
    }

    [Fact]
    public void ParseContentDelimitedByNonStandardWhitespace()
    {
        var json = "\x00a0{\x00a0'h\x00a0i\x00a0'\x00a0:\x00a0[\x00a0true\x00a0,\x00a0new\x00a0Date\x00a0(\x00a0)\x00a0]\x00a0/*\x00a0comment\x00a0*/\x00a0}\x00a0";
        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.Boolean, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.StartConstructor, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.EndConstructor, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.Comment, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.False(reader.Read());
    }

    [Fact]
    public void ParseObjectWithNoEnd()
    {
        var json = "{hi:1, ";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(reader.Read());
        Assert.True(reader.Read());
        Assert.True(reader.Read());
        Assert.False(reader.Read());
    }

    [Fact]
    public void ParseEmptyArray()
    {
        var json = "[]";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);
    }

    [Fact]
    public void ParseEmptyObject()
    {
        var json = "{}";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public void ParseEmptyConstructor()
    {
        var json = "new Date()";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.StartConstructor, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.EndConstructor, reader.TokenType);
    }

    [Fact]
    public void ParseHexNumber()
    {
        var json = @"0x20";

        var reader = new JsonTextReader(new StringReader(json));

        reader.ReadAsDecimal();
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(32m, reader.Value);
    }

    [Fact]
    public void ParseNumbers()
    {
        var json = @"[0,1,2 , 3]";

        var reader = new JsonTextReader(new StringReader(json));

        reader.Read();
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        reader.Read();
        Assert.Equal(JsonToken.Integer, reader.TokenType);

        reader.Read();
        Assert.Equal(JsonToken.Integer, reader.TokenType);

        reader.Read();
        Assert.Equal(JsonToken.Integer, reader.TokenType);

        reader.Read();
        Assert.Equal(JsonToken.Integer, reader.TokenType);

        reader.Read();
        Assert.Equal(JsonToken.EndArray, reader.TokenType);
    }

    [Fact]
    public void ParseLineFeedDelimitedConstructor()
    {
        var json = "new Date\n()";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(reader.Read());
        Assert.Equal("Date", reader.Value);
        Assert.Equal(JsonToken.StartConstructor, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.EndConstructor, reader.TokenType);
    }

    [Fact]
    public void ParseNullStringConstructor()
    {
        var json = "new Date\0()";
        var reader = new JsonTextReader(new StringReader(json));
#if !RELEASE
        reader.CharBuffer = new char[7];
#endif

        Assert.True(reader.Read());
        Assert.Equal("Date", reader.Value);
        Assert.Equal(JsonToken.StartConstructor, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.EndConstructor, reader.TokenType);
    }

    [Fact]
    public void ParseOctalNumber()
    {
        var json = @"010";

        var reader = new JsonTextReader(new StringReader(json));

        reader.ReadAsDecimal();
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(8m, reader.Value);
    }

    [Fact]
    public void DateParseHandling()
    {
        var json = @"[""1970-01-01T00:00:00Z""]";

        var reader = new JsonTextReader(new StringReader(json));
        reader.DateParseHandling = Argon.DateParseHandling.DateTime;

        Assert.True(reader.Read());
        Assert.True(reader.Read());
        Assert.Equal(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
        Assert.Equal(typeof(DateTime), reader.ValueType);
        Assert.True(reader.Read());

        reader = new JsonTextReader(new StringReader(json));
        reader.DateParseHandling = Argon.DateParseHandling.DateTimeOffset;

        Assert.True(reader.Read());
        Assert.True(reader.Read());
        Assert.Equal(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
        Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        Assert.True(reader.Read());
        Assert.Equal(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
        Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        Assert.True(reader.Read());

        reader = new JsonTextReader(new StringReader(json));
        reader.DateParseHandling = Argon.DateParseHandling.None;

        Assert.True(reader.Read());
        Assert.True(reader.Read());
        Assert.Equal(@"1970-01-01T00:00:00Z", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);
        Assert.True(reader.Read());
        Assert.Equal(@"/Date(0)/", reader.Value);
        Assert.Equal(typeof(string), reader.ValueType);
        Assert.True(reader.Read());

        reader = new JsonTextReader(new StringReader(json));
        reader.DateParseHandling = Argon.DateParseHandling.DateTime;

        Assert.True(reader.Read());
        reader.ReadAsDateTimeOffset();
        Assert.Equal(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
        Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        reader.ReadAsDateTimeOffset();
        Assert.Equal(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
        Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        Assert.True(reader.Read());

        reader = new JsonTextReader(new StringReader(json));
        reader.DateParseHandling = Argon.DateParseHandling.DateTimeOffset;

        Assert.True(reader.Read());
        reader.ReadAsDateTime();
        Assert.Equal(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
        Assert.Equal(typeof(DateTime), reader.ValueType);
        reader.ReadAsDateTime();
        Assert.Equal(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
        Assert.Equal(typeof(DateTime), reader.ValueType);
        Assert.True(reader.Read());
    }
}