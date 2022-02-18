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
using Assert = Argon.Tests.XUnitAssert;
using Argon.Tests.TestObjects.JsonTextReaderTests;

namespace Argon.Tests.JsonTextReaderTests;

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
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("hi\r\nbye", reader.Value);
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);
        Xunit.Assert.Equal(1L, reader.Value);
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);
        Xunit.Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ParseIntegersAsync()
    {
        var reader = new JsonTextReader(new StringReader("1"));
        Xunit.Assert.Equal(1, await reader.ReadAsInt32Async());

        reader = new JsonTextReader(new StringReader("-1"));
        Xunit.Assert.Equal(-1, await reader.ReadAsInt32Async());

        reader = new JsonTextReader(new StringReader("0"));
        Xunit.Assert.Equal(0, await reader.ReadAsInt32Async());

        reader = new JsonTextReader(new StringReader("-0"));
        Xunit.Assert.Equal(0, await reader.ReadAsInt32Async());

        reader = new JsonTextReader(new StringReader(int.MaxValue.ToString()));
        Xunit.Assert.Equal(int.MaxValue, await reader.ReadAsInt32Async());

        reader = new JsonTextReader(new StringReader(int.MinValue.ToString()));
        Xunit.Assert.Equal(int.MinValue, await reader.ReadAsInt32Async());

        reader = new JsonTextReader(new StringReader(long.MaxValue.ToString()));
        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsInt32Async(), "JSON integer 9223372036854775807 is too large or small for an Int32. Path '', line 1, position 19.");

        reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsInt32Async(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

        reader = new JsonTextReader(new StringReader("1E-06"));
        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsInt32Async(), "Input string '1E-06' is not a valid integer. Path '', line 1, position 5.");

        reader = new JsonTextReader(new StringReader("1.1"));
        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsInt32Async(), "Input string '1.1' is not a valid integer. Path '', line 1, position 3.");

        reader = new JsonTextReader(new StringReader(""));
        Xunit.Assert.Equal(null, await reader.ReadAsInt32Async());

        reader = new JsonTextReader(new StringReader("-"));
        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsInt32Async(), "Input string '-' is not a valid integer. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ParseDecimalsAsync()
    {
        var reader = new JsonTextReader(new StringReader("1.1"));
        Xunit.Assert.Equal(1.1m, await reader.ReadAsDecimalAsync());

        reader = new JsonTextReader(new StringReader("-1.1"));
        Xunit.Assert.Equal(-1.1m, await reader.ReadAsDecimalAsync());

        reader = new JsonTextReader(new StringReader("0.0"));
        Xunit.Assert.Equal(0.0m, await reader.ReadAsDecimalAsync());

        reader = new JsonTextReader(new StringReader("-0.0"));
        Xunit.Assert.Equal(0, await reader.ReadAsDecimalAsync());

        reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsDecimalAsync(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

        reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
        reader.FloatParseHandling = FloatParseHandling.Decimal;
        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

        reader = new JsonTextReader(new StringReader("1E-06"));
        Xunit.Assert.Equal(0.000001m, await reader.ReadAsDecimalAsync());

        reader = new JsonTextReader(new StringReader(""));
        Xunit.Assert.Equal(null, await reader.ReadAsDecimalAsync());

        reader = new JsonTextReader(new StringReader("-"));
        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsDecimalAsync(), "Input string '-' is not a valid decimal. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ParseDoublesAsync()
    {
        var reader = new JsonTextReader(new StringReader("1.1"));
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(typeof(double), reader.ValueType);
        Xunit.Assert.Equal(1.1d, reader.Value);

        reader = new JsonTextReader(new StringReader("-1.1"));
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(typeof(double), reader.ValueType);
        Xunit.Assert.Equal(-1.1d, reader.Value);

        reader = new JsonTextReader(new StringReader("0.0"));
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(typeof(double), reader.ValueType);
        Xunit.Assert.Equal(0.0d, reader.Value);

        reader = new JsonTextReader(new StringReader("-0.0"));
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(typeof(double), reader.ValueType);
        Xunit.Assert.Equal(-0.0d, reader.Value);

        reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

        reader = new JsonTextReader(new StringReader("1E-06"));
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(typeof(double), reader.ValueType);
        Xunit.Assert.Equal(0.000001d, reader.Value);

        reader = new JsonTextReader(new StringReader(""));
        Xunit.Assert.False(await reader.ReadAsync());

        reader = new JsonTextReader(new StringReader("-"));
        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Input string '-' is not a valid number. Path '', line 1, position 1.");

        reader = new JsonTextReader(new StringReader("1.7976931348623157E+308"));
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(typeof(double), reader.ValueType);
        Xunit.Assert.Equal(Double.MaxValue, reader.Value);

        reader = new JsonTextReader(new StringReader("-1.7976931348623157E+308"));
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(typeof(double), reader.ValueType);
        Xunit.Assert.Equal(Double.MinValue, reader.Value);

        reader = new JsonTextReader(new StringReader("1E+309"));
#if !(NETSTANDARD2_0)
        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Input string '1E+309' is not a valid number. Path '', line 1, position 6.");
#else
            Xunit.Assert.True(await reader.ReadAsync());
            Xunit.Assert.Equal(typeof(double), reader.ValueType);
            Xunit.Assert.Equal(Double.PositiveInfinity, reader.Value);
#endif

        reader = new JsonTextReader(new StringReader("-1E+5000"));
#if !(NETSTANDARD2_0)
        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Input string '-1E+5000' is not a valid number. Path '', line 1, position 8.");
#else
            Xunit.Assert.True(await reader.ReadAsync());
            Xunit.Assert.Equal(typeof(double), reader.ValueType);
            Xunit.Assert.Equal(Double.NegativeInfinity, reader.Value);
#endif

        reader = new JsonTextReader(new StringReader("5.1231231E"));
        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Input string '5.1231231E' is not a valid number. Path '', line 1, position 10.");

        reader = new JsonTextReader(new StringReader("1E-23"));
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(typeof(double), reader.ValueType);
        Xunit.Assert.Equal(1e-23, reader.Value);
    }

    [Fact]
    public async Task ParseArrayWithMissingValuesAsync()
    {
        var json = "[,,, \n\r\n \0   \r  , ,    ]";
        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Undefined, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Undefined, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Undefined, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Undefined, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Undefined, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);
    }

    [Fact]
    public async Task ParseBooleanWithNoExtraContentAsync()
    {
        var json = "[true ";
        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ParseContentDelimitedByNonStandardWhitespaceAsync()
    {
        var json = "\x00a0{\x00a0'h\x00a0i\x00a0'\x00a0:\x00a0[\x00a0true\x00a0,\x00a0new\x00a0Date\x00a0(\x00a0)\x00a0]\x00a0/*\x00a0comment\x00a0*/\x00a0}\x00a0";
        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Boolean, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartConstructor, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndConstructor, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Xunit.Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ParseObjectWithNoEndAsync()
    {
        var json = "{hi:1, ";
        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ParseEmptyArrayAsync()
    {
        var json = "[]";
        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);
    }

    [Fact]
    public async Task ParseEmptyObjectAsync()
    {
        var json = "{}";
        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public async Task ParseEmptyConstructorAsync()
    {
        var json = "new Date()";
        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartConstructor, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndConstructor, reader.TokenType);
    }

    [Fact]
    public async Task ParseHexNumberAsync()
    {
        var json = @"0x20";

        var reader = new JsonTextReader(new StringReader(json));

        await reader.ReadAsDecimalAsync();
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);
        Xunit.Assert.Equal(32m, reader.Value);
    }

    [Fact]
    public async Task ParseNumbersAsync()
    {
        var json = @"[0,1,2 , 3]";

        var reader = new JsonTextReader(new StringReader(json));

        await reader.ReadAsync();
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        await reader.ReadAsync();
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);

        await reader.ReadAsync();
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);

        await reader.ReadAsync();
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);

        await reader.ReadAsync();
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);

        await reader.ReadAsync();
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);
    }

    [Fact]
    public async Task ParseLineFeedDelimitedConstructorAsync()
    {
        var json = "new Date\n()";
        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal("Date", reader.Value);
        Xunit.Assert.Equal(JsonToken.StartConstructor, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndConstructor, reader.TokenType);
    }

    [Fact]
    public async Task ParseNullStringConstructorAsync()
    {
        var json = "new Date\0()";
        var reader = new JsonTextReader(new StringReader(json));
#if DEBUG
        reader.CharBuffer = new char[7];
#endif

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal("Date", reader.Value);
        Xunit.Assert.Equal(JsonToken.StartConstructor, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndConstructor, reader.TokenType);
    }

    [Fact]
    public async Task ParseOctalNumberAsync()
    {
        var json = @"010";

        var reader = new JsonTextReader(new StringReader(json));

        await reader.ReadAsDecimalAsync();
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);
        Xunit.Assert.Equal(8m, reader.Value);
    }

    [Fact]
    public async Task DateParseHandlingAsync()
    {
        var json = @"[""1970-01-01T00:00:00Z"",""\/Date(0)\/""]";

        var reader = new JsonTextReader(new StringReader(json));
        reader.DateParseHandling = DateParseHandling.DateTime;

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
        Xunit.Assert.Equal(typeof(DateTime), reader.ValueType);
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
        Xunit.Assert.Equal(typeof(DateTime), reader.ValueType);
        Xunit.Assert.True(await reader.ReadAsync());

        reader = new JsonTextReader(new StringReader(json));
        reader.DateParseHandling = DateParseHandling.DateTimeOffset;

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
        Xunit.Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
        Xunit.Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        Xunit.Assert.True(await reader.ReadAsync());

        reader = new JsonTextReader(new StringReader(json));
        reader.DateParseHandling = DateParseHandling.None;

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(@"1970-01-01T00:00:00Z", reader.Value);
        Xunit.Assert.Equal(typeof(string), reader.ValueType);
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(@"/Date(0)/", reader.Value);
        Xunit.Assert.Equal(typeof(string), reader.ValueType);
        Xunit.Assert.True(await reader.ReadAsync());

        reader = new JsonTextReader(new StringReader(json));
        reader.DateParseHandling = DateParseHandling.DateTime;

        Xunit.Assert.True(await reader.ReadAsync());
        await reader.ReadAsDateTimeOffsetAsync();
        Xunit.Assert.Equal(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
        Xunit.Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        await reader.ReadAsDateTimeOffsetAsync();
        Xunit.Assert.Equal(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
        Xunit.Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        Xunit.Assert.True(await reader.ReadAsync());

        reader = new JsonTextReader(new StringReader(json));
        reader.DateParseHandling = DateParseHandling.DateTimeOffset;

        Xunit.Assert.True(await reader.ReadAsync());
        await reader.ReadAsDateTimeAsync();
        Xunit.Assert.Equal(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
        Xunit.Assert.Equal(typeof(DateTime), reader.ValueType);
        await reader.ReadAsDateTimeAsync();
        Xunit.Assert.Equal(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
        Xunit.Assert.Equal(typeof(DateTime), reader.ValueType);
        Xunit.Assert.True(await reader.ReadAsync());
    }
}