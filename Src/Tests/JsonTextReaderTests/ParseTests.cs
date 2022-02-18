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
using Argon.Tests.TestObjects.JsonTextReaderTests;

namespace Argon.Tests.JsonTextReaderTests;

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
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("hi\r\nbye", reader.Value);
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);
        Xunit.Assert.Equal(1L, reader.Value);
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);
        Xunit.Assert.False(reader.Read());
    }

    [Fact]
    public void ParseIntegers()
    {
        var reader = new JsonTextReader(new StringReader("1"));
        Xunit.Assert.Equal(1, reader.ReadAsInt32());

        reader = new JsonTextReader(new StringReader("-1"));
        Xunit.Assert.Equal(-1, reader.ReadAsInt32());

        reader = new JsonTextReader(new StringReader("0"));
        Xunit.Assert.Equal(0, reader.ReadAsInt32());

        reader = new JsonTextReader(new StringReader("-0"));
        Xunit.Assert.Equal(0, reader.ReadAsInt32());

        reader = new JsonTextReader(new StringReader(int.MaxValue.ToString()));
        Xunit.Assert.Equal(int.MaxValue, reader.ReadAsInt32());

        reader = new JsonTextReader(new StringReader(int.MinValue.ToString()));
        Xunit.Assert.Equal(int.MinValue, reader.ReadAsInt32());

        reader = new JsonTextReader(new StringReader(long.MaxValue.ToString()));
        ExceptionAssert.Throws<JsonReaderException>(() => reader.ReadAsInt32(), "JSON integer 9223372036854775807 is too large or small for an Int32. Path '', line 1, position 19.");

        reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
        ExceptionAssert.Throws<JsonReaderException>(() => reader.ReadAsInt32(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

        reader = new JsonTextReader(new StringReader("1E-06"));
        ExceptionAssert.Throws<JsonReaderException>(() => reader.ReadAsInt32(), "Input string '1E-06' is not a valid integer. Path '', line 1, position 5.");

        reader = new JsonTextReader(new StringReader("1.1"));
        ExceptionAssert.Throws<JsonReaderException>(() => reader.ReadAsInt32(), "Input string '1.1' is not a valid integer. Path '', line 1, position 3.");

        reader = new JsonTextReader(new StringReader(""));
        Xunit.Assert.Equal(null, reader.ReadAsInt32());

        reader = new JsonTextReader(new StringReader("-"));
        ExceptionAssert.Throws<JsonReaderException>(() => reader.ReadAsInt32(), "Input string '-' is not a valid integer. Path '', line 1, position 1.");
    }

    [Fact]
    public void ParseDecimals()
    {
        var reader = new JsonTextReader(new StringReader("1.1"));
        Xunit.Assert.Equal(1.1m, reader.ReadAsDecimal());

        reader = new JsonTextReader(new StringReader("-1.1"));
        Xunit.Assert.Equal(-1.1m, reader.ReadAsDecimal());

        reader = new JsonTextReader(new StringReader("0.0"));
        Xunit.Assert.Equal(0.0m, reader.ReadAsDecimal());

        reader = new JsonTextReader(new StringReader("-0.0"));
        Xunit.Assert.Equal(0, reader.ReadAsDecimal());

        reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
        ExceptionAssert.Throws<JsonReaderException>(() => reader.ReadAsDecimal(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

        reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
        reader.FloatParseHandling = FloatParseHandling.Decimal;
        ExceptionAssert.Throws<JsonReaderException>(() => reader.Read(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

        reader = new JsonTextReader(new StringReader("1E-06"));
        Xunit.Assert.Equal(0.000001m, reader.ReadAsDecimal());

        reader = new JsonTextReader(new StringReader(""));
        Xunit.Assert.Equal(null, reader.ReadAsDecimal());

        reader = new JsonTextReader(new StringReader("-"));
        ExceptionAssert.Throws<JsonReaderException>(() => reader.ReadAsDecimal(), "Input string '-' is not a valid decimal. Path '', line 1, position 1.");
    }

    [Fact]
    public void ParseDoubles()
    {
        var reader = new JsonTextReader(new StringReader("1.1"));
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(typeof(double), reader.ValueType);
        Xunit.Assert.Equal(1.1d, reader.Value);

        reader = new JsonTextReader(new StringReader("-1.1"));
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(typeof(double), reader.ValueType);
        Xunit.Assert.Equal(-1.1d, reader.Value);

        reader = new JsonTextReader(new StringReader("0.0"));
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(typeof(double), reader.ValueType);
        Xunit.Assert.Equal(0.0d, reader.Value);

        reader = new JsonTextReader(new StringReader("-0.0"));
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(typeof(double), reader.ValueType);
        Xunit.Assert.Equal(-0.0d, reader.Value);

        reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
        ExceptionAssert.Throws<JsonReaderException>(() => reader.Read(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

        reader = new JsonTextReader(new StringReader("1E-06"));
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(typeof(double), reader.ValueType);
        Xunit.Assert.Equal(0.000001d, reader.Value);

        reader = new JsonTextReader(new StringReader(""));
        Xunit.Assert.False(reader.Read());

        reader = new JsonTextReader(new StringReader("-"));
        ExceptionAssert.Throws<JsonReaderException>(() => reader.Read(), "Input string '-' is not a valid number. Path '', line 1, position 1.");

        reader = new JsonTextReader(new StringReader("1.7976931348623157E+308"));
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(typeof(double), reader.ValueType);
        Xunit.Assert.Equal(Double.MaxValue, reader.Value);

        reader = new JsonTextReader(new StringReader("-1.7976931348623157E+308"));
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(typeof(double), reader.ValueType);
        Xunit.Assert.Equal(Double.MinValue, reader.Value);

        reader = new JsonTextReader(new StringReader("1E+309"));
#if !(NETSTANDARD2_0)
        ExceptionAssert.Throws<JsonReaderException>(() => reader.Read(), "Input string '1E+309' is not a valid number. Path '', line 1, position 6.");
#else
            Xunit.Assert.True(reader.Read());
            Xunit.Assert.Equal(typeof(double), reader.ValueType);
            Xunit.Assert.Equal(Double.PositiveInfinity, reader.Value);
#endif

        reader = new JsonTextReader(new StringReader("-1E+5000"));
#if !(NETSTANDARD2_0)
        ExceptionAssert.Throws<JsonReaderException>(() => reader.Read(), "Input string '-1E+5000' is not a valid number. Path '', line 1, position 8.");
#else
            Xunit.Assert.True(reader.Read());
            Xunit.Assert.Equal(typeof(double), reader.ValueType);
            Xunit.Assert.Equal(Double.NegativeInfinity, reader.Value);
#endif

        reader = new JsonTextReader(new StringReader("5.1231231E"));
        ExceptionAssert.Throws<JsonReaderException>(() => reader.Read(), "Input string '5.1231231E' is not a valid number. Path '', line 1, position 10.");

        reader = new JsonTextReader(new StringReader("1E-23"));
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(typeof(double), reader.ValueType);
        Xunit.Assert.Equal(1e-23, reader.Value);
    }

    [Fact]
    public void ParseArrayWithMissingValues()
    {
        var json = "[,,, \n\r\n \0   \r  , ,    ]";
        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Undefined, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Undefined, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Undefined, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Undefined, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Undefined, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);
    }

    [Fact]
    public void ParseBooleanWithNoExtraContent()
    {
        var json = "[true ";
        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.False(reader.Read());
    }

    [Fact]
    public void ParseContentDelimitedByNonStandardWhitespace()
    {
        var json = "\x00a0{\x00a0'h\x00a0i\x00a0'\x00a0:\x00a0[\x00a0true\x00a0,\x00a0new\x00a0Date\x00a0(\x00a0)\x00a0]\x00a0/*\x00a0comment\x00a0*/\x00a0}\x00a0";
        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Boolean, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartConstructor, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndConstructor, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Xunit.Assert.False(reader.Read());
    }

    [Fact]
    public void ParseObjectWithNoEnd()
    {
        var json = "{hi:1, ";
        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.False(reader.Read());
    }

    [Fact]
    public void ParseEmptyArray()
    {
        var json = "[]";
        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);
    }

    [Fact]
    public void ParseEmptyObject()
    {
        var json = "{}";
        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public void ParseEmptyConstructor()
    {
        var json = "new Date()";
        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartConstructor, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndConstructor, reader.TokenType);
    }

    [Fact]
    public void ParseHexNumber()
    {
        var json = @"0x20";

        var reader = new JsonTextReader(new StringReader(json));

        reader.ReadAsDecimal();
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);
        Xunit.Assert.Equal(32m, reader.Value);
    }

    [Fact]
    public void ParseNumbers()
    {
        var json = @"[0,1,2 , 3]";

        var reader = new JsonTextReader(new StringReader(json));

        reader.Read();
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        reader.Read();
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);

        reader.Read();
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);

        reader.Read();
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);

        reader.Read();
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);

        reader.Read();
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);
    }

    [Fact]
    public void ParseLineFeedDelimitedConstructor()
    {
        var json = "new Date\n()";
        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal("Date", reader.Value);
        Xunit.Assert.Equal(JsonToken.StartConstructor, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndConstructor, reader.TokenType);
    }

    [Fact]
    public void ParseNullStringConstructor()
    {
        var json = "new Date\0()";
        var reader = new JsonTextReader(new StringReader(json));
#if DEBUG
        reader.CharBuffer = new char[7];
#endif

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal("Date", reader.Value);
        Xunit.Assert.Equal(JsonToken.StartConstructor, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndConstructor, reader.TokenType);
    }

    [Fact]
    public void ParseOctalNumber()
    {
        var json = @"010";

        var reader = new JsonTextReader(new StringReader(json));

        reader.ReadAsDecimal();
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);
        Xunit.Assert.Equal(8m, reader.Value);
    }

    [Fact]
    public void DateParseHandling()
    {
        var json = @"[""1970-01-01T00:00:00Z"",""\/Date(0)\/""]";

        var reader = new JsonTextReader(new StringReader(json));
        reader.DateParseHandling = Argon.DateParseHandling.DateTime;

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
        Xunit.Assert.Equal(typeof(DateTime), reader.ValueType);
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
        Xunit.Assert.Equal(typeof(DateTime), reader.ValueType);
        Xunit.Assert.True(reader.Read());

        reader = new JsonTextReader(new StringReader(json));
        reader.DateParseHandling = Argon.DateParseHandling.DateTimeOffset;

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
        Xunit.Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
        Xunit.Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        Xunit.Assert.True(reader.Read());

        reader = new JsonTextReader(new StringReader(json));
        reader.DateParseHandling = Argon.DateParseHandling.None;

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(@"1970-01-01T00:00:00Z", reader.Value);
        Xunit.Assert.Equal(typeof(string), reader.ValueType);
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(@"/Date(0)/", reader.Value);
        Xunit.Assert.Equal(typeof(string), reader.ValueType);
        Xunit.Assert.True(reader.Read());

        reader = new JsonTextReader(new StringReader(json));
        reader.DateParseHandling = Argon.DateParseHandling.DateTime;

        Xunit.Assert.True(reader.Read());
        reader.ReadAsDateTimeOffset();
        Xunit.Assert.Equal(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
        Xunit.Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        reader.ReadAsDateTimeOffset();
        Xunit.Assert.Equal(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
        Xunit.Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        Xunit.Assert.True(reader.Read());

        reader = new JsonTextReader(new StringReader(json));
        reader.DateParseHandling = Argon.DateParseHandling.DateTimeOffset;

        Xunit.Assert.True(reader.Read());
        reader.ReadAsDateTime();
        Xunit.Assert.Equal(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
        Xunit.Assert.Equal(typeof(DateTime), reader.ValueType);
        reader.ReadAsDateTime();
        Xunit.Assert.Equal(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
        Xunit.Assert.Equal(typeof(DateTime), reader.ValueType);
        Xunit.Assert.True(reader.Read());
    }
}