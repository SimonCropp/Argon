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

[TestFixture]
public class ExceptionHandlingAsyncTests : TestFixtureBase
{
    [Fact]
    public async Task ReadAsBytes_MissingCommaAsync()
    {
        var data = Encoding.UTF8.GetBytes("Hello world");

        var json = @"['" + Convert.ToBase64String(data) + "' '" + Convert.ToBase64String(data) + @"']";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.IsTrue(await reader.ReadAsync());
        Xunit.Assert.Equal(data, await reader.ReadAsBytesAsync());

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsBytesAsync(),
            "After parsing a value an unexpected character was encountered: '. Path '[0]', line 1, position 20.");
    }

    [Fact]
    public async Task ReadAsInt32_MissingCommaAsync()
    {
        var json = "[0 1 2]";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual(0, (int)await reader.ReadAsInt32Async());

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsInt32Async(),
            "After parsing a value an unexpected character was encountered: 1. Path '[0]', line 1, position 3.");
    }

    [Fact]
    public async Task ReadAsBoolean_MissingCommaAsync()
    {
        var json = "[true false true]";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual(true, (bool)await reader.ReadAsBooleanAsync());

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsBooleanAsync(),
            "After parsing a value an unexpected character was encountered: f. Path '[0]', line 1, position 6.");
    }

    [Fact]
    public async Task ReadAsDateTime_MissingCommaAsync()
    {
        var json = "['2017-02-04T00:00:00Z' '2018-02-04T00:00:00Z' '2019-02-04T00:00:00Z']";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual(new DateTime(2017, 2, 4, 0, 0, 0, DateTimeKind.Utc), (DateTime)await reader.ReadAsDateTimeAsync());

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsDateTimeAsync(),
            "After parsing a value an unexpected character was encountered: '. Path '[0]', line 1, position 24.");
    }

    [Fact]
    public async Task ReadAsDateTimeOffset_MissingCommaAsync()
    {
        var json = "['2017-02-04T00:00:00Z' '2018-02-04T00:00:00Z' '2019-02-04T00:00:00Z']";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual(new DateTimeOffset(2017, 2, 4, 0, 0, 0, TimeSpan.Zero), (DateTimeOffset)await reader.ReadAsDateTimeOffsetAsync());

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsDateTimeOffsetAsync(),
            "After parsing a value an unexpected character was encountered: '. Path '[0]', line 1, position 24.");
    }

    [Fact]
    public async Task ReadAsString_MissingCommaAsync()
    {
        var json = "['2017-02-04T00:00:00Z' '2018-02-04T00:00:00Z' '2019-02-04T00:00:00Z']";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual("2017-02-04T00:00:00Z", await reader.ReadAsStringAsync());

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsStringAsync(),
            "After parsing a value an unexpected character was encountered: '. Path '[0]', line 1, position 24.");
    }

    [Fact]
    public async Task Read_MissingCommaAsync()
    {
        var json = "[0 1 2]";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.IsTrue(await reader.ReadAsync());
        Assert.IsTrue(await reader.ReadAsync());

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsync(),
            "After parsing a value an unexpected character was encountered: 1. Path '[0]', line 1, position 3.");
    }

    [Fact]
    public async Task UnexpectedEndAfterReadingNAsync()
    {
        var reader = new JsonTextReader(new StringReader("n"));
        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Unexpected end when reading JSON. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task UnexpectedEndAfterReadingNuAsync()
    {
        var reader = new JsonTextReader(new StringReader("nu"));
        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Unexpected end when reading JSON. Path '', line 1, position 2.");
    }

    [Fact]
    public async Task UnexpectedEndAfterReadingNeAsync()
    {
        var reader = new JsonTextReader(new StringReader("ne"));
        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await reader.ReadAsync(), "Unexpected end when reading JSON. Path '', line 1, position 2.");
    }

    [Fact]
    public async Task UnexpectedEndOfHexAsync()
    {
        JsonReader reader = new JsonTextReader(new StringReader(@"'h\u123"));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unexpected end while parsing Unicode escape sequence. Path '', line 1, position 4.");
    }

    [Fact]
    public async Task UnexpectedEndOfControlCharacterAsync()
    {
        JsonReader reader = new JsonTextReader(new StringReader(@"'h\"));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unterminated string. Expected delimiter: '. Path '', line 1, position 3.");
    }

    [Fact]
    public async Task ReadInvalidNonBase10NumberAsync()
    {
        var json = "0aq2dun13.hod";

        var reader = new JsonTextReader(new StringReader(json));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unexpected character encountered while parsing number: q. Path '', line 1, position 2.");

        reader = new JsonTextReader(new StringReader(json));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDecimalAsync(); }, "Unexpected character encountered while parsing number: q. Path '', line 1, position 2.");

        reader = new JsonTextReader(new StringReader(json));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsInt32Async(); }, "Unexpected character encountered while parsing number: q. Path '', line 1, position 2.");
    }

    [Fact]
    public async Task ThrowErrorWhenParsingUnquoteStringThatStartsWithNEAsync()
    {
        const string json = @"{ ""ItemName"": ""value"", ""u"":netanelsalinger,""r"":9 }";

        var reader = new JsonTextReader(new StringReader(json));

        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.String, reader.TokenType);

        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unexpected content while parsing JSON. Path 'u', line 1, position 29.");
    }

    [Fact]
    public async Task UnexpectedEndOfStringAsync()
    {
        JsonReader reader = new JsonTextReader(new StringReader("'hi"));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unterminated string. Expected delimiter: '. Path '', line 1, position 3.");
    }

    [Fact]
    public async Task UnexpectedEndTokenWhenParsingOddEndTokenAsync()
    {
        JsonReader reader = new JsonTextReader(new StringReader(@"{}}"));
        Assert.IsTrue(await reader.ReadAsync());
        Assert.IsTrue(await reader.ReadAsync());

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Additional text encountered after finished reading JSON content: }. Path '', line 1, position 2.");
    }

    [Fact]
    public async Task ResetJsonTextReaderErrorCountAsync()
    {
        var toggleReaderError = new ToggleReaderError(new StringReader("{'first':1,'second':2,'third':3}"));
        var jsonTextReader = new JsonTextReader(toggleReaderError);

        Assert.IsTrue(await jsonTextReader.ReadAsync());

        toggleReaderError.Error = true;

        await ExceptionAssert.ThrowsAsync<Exception>(async () => await jsonTextReader.ReadAsync(), "Read error");
        await ExceptionAssert.ThrowsAsync<Exception>(async () => await jsonTextReader.ReadAsync(), "Read error");

        toggleReaderError.Error = false;

        Assert.IsTrue(await jsonTextReader.ReadAsync());
        Assert.AreEqual("first", jsonTextReader.Value);

        toggleReaderError.Error = true;

        await ExceptionAssert.ThrowsAsync<Exception>(async () => await jsonTextReader.ReadAsync(), "Read error");

        toggleReaderError.Error = false;

        Assert.IsTrue(await jsonTextReader.ReadAsync());
        Assert.AreEqual(1L, jsonTextReader.Value);

        toggleReaderError.Error = true;

        await ExceptionAssert.ThrowsAsync<Exception>(async () => await jsonTextReader.ReadAsync(), "Read error");
        await ExceptionAssert.ThrowsAsync<Exception>(async () => await jsonTextReader.ReadAsync(), "Read error");
        await ExceptionAssert.ThrowsAsync<Exception>(async () => await jsonTextReader.ReadAsync(), "Read error");

        toggleReaderError.Error = false;
    }

    [Fact]
    public async Task MatchWithInsufficentCharactersAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"nul"));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unexpected end when reading JSON. Path '', line 1, position 3.");
    }

    [Fact]
    public async Task MatchWithWrongCharactersAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"nulz"));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Error parsing null value. Path '', line 1, position 3.");
    }

    [Fact]
    public async Task MatchWithNoTrailingSeparatorAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"nullz"));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Error parsing null value. Path '', line 1, position 4.");
    }

    [Fact]
    public async Task UnclosedCommentAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"/* sdf"));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unexpected end while parsing comment. Path '', line 1, position 6.");
    }

    [Fact]
    public async Task BadCommentStartAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"/sdf"));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Error parsing comment. Expected: *, got s. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task MissingColonAsync()
    {
        var json = @"{
    ""A"" : true,
    ""B"" """;

        var reader = new JsonTextReader(new StringReader(json));

        await reader.ReadAsync();
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        await reader.ReadAsync();
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsync();
        Assert.AreEqual(JsonToken.Boolean, reader.TokenType);

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, @"Invalid character after parsing property name. Expected ':' but got: "". Path 'A', line 3, position 8.");
    }

    [Fact]
    public async Task ParseConstructorWithBadCharacterAsync()
    {
        var json = "new Date,()";
        var reader = new JsonTextReader(new StringReader(json));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { Assert.IsTrue(await reader.ReadAsync()); }, "Unexpected character while parsing constructor: ,. Path '', line 1, position 8.");
    }

    [Fact]
    public async Task ParseConstructorWithUnexpectedEndAsync()
    {
        var json = "new Dat";
        var reader = new JsonTextReader(new StringReader(json));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unexpected end while parsing constructor. Path '', line 1, position 7.");
    }

    [Fact]
    public async Task ParseConstructorWithUnexpectedCharacterAsync()
    {
        var json = "new Date !";
        var reader = new JsonTextReader(new StringReader(json));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unexpected character while parsing constructor: !. Path '', line 1, position 9.");
    }

    [Fact]
    public async Task ParseAdditionalContent_CommaAsync()
    {
        var json = @"[
""Small"",
""Medium"",
""Large""
],";

        var reader = new JsonTextReader(new StringReader(json));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
        {
            while (await reader.ReadAsync())
            {
            }
        }, "Additional text encountered after finished reading JSON content: ,. Path '', line 5, position 1.");
    }

    [Fact]
    public async Task ParseAdditionalContent_TextAsync()
    {
        var json = @"[
""Small"",
""Medium"",
""Large""
]content";

        var reader = new JsonTextReader(new StringReader(json));
#if DEBUG
        reader.CharBuffer = new char[2];
#endif

        await reader.ReadAsync();
        Assert.AreEqual(1, reader.LineNumber);

        await reader.ReadAsync();
        Assert.AreEqual(2, reader.LineNumber);

        await reader.ReadAsync();
        Assert.AreEqual(3, reader.LineNumber);

        await reader.ReadAsync();
        Assert.AreEqual(4, reader.LineNumber);

        await reader.ReadAsync();
        Assert.AreEqual(5, reader.LineNumber);

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Additional text encountered after finished reading JSON content: c. Path '', line 5, position 1.");
    }

    [Fact]
    public async Task ParseAdditionalContent_WhitespaceThenTextAsync()
    {
        var json = @"'hi' a";

        var reader = new JsonTextReader(new StringReader(json));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
        {
            while (await reader.ReadAsync())
            {
            }
        }, "Additional text encountered after finished reading JSON content: a. Path '', line 1, position 5.");
    }

    [Fact]
    public async Task ParseIncompleteCommentSeparatorAsync()
    {
        var reader = new JsonTextReader(new StringReader("true/"));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Error parsing boolean value. Path '', line 1, position 4.");
    }

    [Fact]
    public async Task ReadBadCharInArrayAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"[}"));

        await reader.ReadAsync();

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unexpected character encountered while parsing value: }. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadAsBytesNoContentWrappedObjectAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"{"));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsBytesAsync(); }, "Unexpected end when reading JSON. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadBytesEmptyWrappedObjectAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"{}"));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsBytesAsync(); }, "Error reading bytes. Unexpected token: StartObject. Path '', line 1, position 2." );
    }

    [Fact]
    public async Task ReadIntegerWithErrorAsync()
    {
        var json = @"{
    ChildId: 333333333333333333333333333333333333333
}";

        var jsonTextReader = new JsonTextReader(new StringReader(json));

        Assert.IsTrue(await jsonTextReader.ReadAsync());
        Assert.AreEqual(JsonToken.StartObject, jsonTextReader.TokenType);

        Assert.IsTrue(await jsonTextReader.ReadAsync());
        Assert.AreEqual(JsonToken.PropertyName, jsonTextReader.TokenType);

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await jsonTextReader.ReadAsInt32Async(), "JSON integer 333333333333333333333333333333333333333 is too large or small for an Int32. Path 'ChildId', line 2, position 52.");

        Assert.IsTrue(await jsonTextReader.ReadAsync());
        Assert.AreEqual(JsonToken.EndObject, jsonTextReader.TokenType);

        Assert.IsFalse(await jsonTextReader.ReadAsync());
    }

    [Fact]
    public async Task ReadIntegerWithErrorInArrayAsync()
    {
        var json = @"[
  333333333333333333333333333333333333333,
  3.3,
  ,
  0f
]";

        var jsonTextReader = new JsonTextReader(new StringReader(json));

        Assert.IsTrue(await jsonTextReader.ReadAsync());
        Assert.AreEqual(JsonToken.StartArray, jsonTextReader.TokenType);

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await jsonTextReader.ReadAsInt32Async(), "JSON integer 333333333333333333333333333333333333333 is too large or small for an Int32. Path '[0]', line 2, position 41.");

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await jsonTextReader.ReadAsInt32Async(), "Input string '3.3' is not a valid integer. Path '[1]', line 3, position 5.");

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await jsonTextReader.ReadAsInt32Async(), "Unexpected character encountered while parsing value: ,. Path '[2]', line 4, position 3.");

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await jsonTextReader.ReadAsInt32Async(), "Input string '0f' is not a valid integer. Path '[3]', line 5, position 4.");

        Assert.IsTrue(await jsonTextReader.ReadAsync());
        Assert.AreEqual(JsonToken.EndArray, jsonTextReader.TokenType);

        Assert.IsFalse(await jsonTextReader.ReadAsync());
    }

    [Fact]
    public async Task ReadBytesWithErrorAsync()
    {
        var json = @"{
    ChildId: '123'
}";

        var jsonTextReader = new JsonTextReader(new StringReader(json));

        Assert.IsTrue(await jsonTextReader.ReadAsync());
        Assert.AreEqual(JsonToken.StartObject, jsonTextReader.TokenType);

        Assert.IsTrue(await jsonTextReader.ReadAsync());
        Assert.AreEqual(JsonToken.PropertyName, jsonTextReader.TokenType);

        try
        {
            await jsonTextReader.ReadAsBytesAsync();
        }
        catch (FormatException)
        {
        }

        Assert.IsTrue(await jsonTextReader.ReadAsync());
        Assert.AreEqual(JsonToken.EndObject, jsonTextReader.TokenType);

        Assert.IsFalse(await jsonTextReader.ReadAsync());
    }

    [Fact]
    public async Task ReadInt32OverflowAsync()
    {
        long i = int.MaxValue;

        var reader = new JsonTextReader(new StringReader(i.ToString(CultureInfo.InvariantCulture)));
        await reader.ReadAsync();
        Assert.AreEqual(typeof(long), reader.ValueType);

        for (var j = 1; j < 1000; j++)
        {
            var total = j + i;
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                reader = new JsonTextReader(new StringReader(total.ToString(CultureInfo.InvariantCulture)));
                await reader.ReadAsInt32Async();
            }, "JSON integer " + total + " is too large or small for an Int32. Path '', line 1, position 10.");
        }
    }

    [Fact]
    public async Task ReadInt32Overflow_NegativeAsync()
    {
        long i = int.MinValue;

        var reader = new JsonTextReader(new StringReader(i.ToString(CultureInfo.InvariantCulture)));
        await reader.ReadAsync();
        Assert.AreEqual(typeof(long), reader.ValueType);
        Assert.AreEqual(i, reader.Value);

        for (var j = 1; j < 1000; j++)
        {
            var total = -j + i;
            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                reader = new JsonTextReader(new StringReader(total.ToString(CultureInfo.InvariantCulture)));
                await reader.ReadAsInt32Async();
            }, "JSON integer " + total + " is too large or small for an Int32. Path '', line 1, position 11.");
        }
    }

    [Fact]
    public async Task ReadInt64OverflowAsync()
    {
        var i = new BigInteger(long.MaxValue);

        var reader = new JsonTextReader(new StringReader(i.ToString(CultureInfo.InvariantCulture)));
        await reader.ReadAsync();
        Assert.AreEqual(typeof(long), reader.ValueType);

        for (var j = 1; j < 1000; j++)
        {
            var total = i + j;

            reader = new JsonTextReader(new StringReader(total.ToString(CultureInfo.InvariantCulture)));
            await reader.ReadAsync();

            Assert.AreEqual(typeof(BigInteger), reader.ValueType);
        }
    }

    [Fact]
    public async Task ReadInt64Overflow_NegativeAsync()
    {
        var i = new BigInteger(long.MinValue);

        var reader = new JsonTextReader(new StringReader(i.ToString(CultureInfo.InvariantCulture)));
        await reader.ReadAsync();
        Assert.AreEqual(typeof(long), reader.ValueType);

        for (var j = 1; j < 1000; j++)
        {
            var total = i + -j;

            reader = new JsonTextReader(new StringReader(total.ToString(CultureInfo.InvariantCulture)));
            await reader.ReadAsync();

            Assert.AreEqual(typeof(BigInteger), reader.ValueType);
        }
    }

    [Fact]
    public async Task ReadAsString_Null_AdditionalBadDataAsync()
    {
        var json = @"nullllll";

        var reader = new JsonTextReader(new StringReader(json));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsStringAsync(); }, "Error parsing null value. Path '', line 1, position 4.");
    }

    [Fact]
    public async Task ReadAsBoolean_AdditionalBadDataAsync()
    {
        var json = @"falseeeee";

        var reader = new JsonTextReader(new StringReader(json));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsBooleanAsync(); }, "Unexpected character encountered while parsing value: e. Path '', line 1, position 5.");
    }

    [Fact]
    public async Task ReadAsString_AdditionalBadDataAsync()
    {
        var json = @"falseeeee";

        var reader = new JsonTextReader(new StringReader(json));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsStringAsync(); }, "Unexpected character encountered while parsing value: e. Path '', line 1, position 5.");
    }

    [Fact]
    public async Task ReadAsBoolean_UnexpectedEndAsync()
    {
        var json = @"tru";

        var reader = new JsonTextReader(new StringReader(json));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsBooleanAsync(); }, "Unexpected end when reading JSON. Path '', line 1, position 3.");
    }

    [Fact]
    public async Task ReadAsBoolean_BadDataAsync()
    {
        var json = @"pie";

        var reader = new JsonTextReader(new StringReader(json));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsBooleanAsync(); }, "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadAsString_BadDataAsync()
    {
        var json = @"pie";

        var reader = new JsonTextReader(new StringReader(json));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsStringAsync(); }, "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadAsDouble_BadDataAsync()
    {
        var json = @"pie";

        var reader = new JsonTextReader(new StringReader(json));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDoubleAsync(); }, "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadAsDouble_BooleanAsync()
    {
        var json = @"true";

        var reader = new JsonTextReader(new StringReader(json));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDoubleAsync(); }, "Unexpected character encountered while parsing value: t. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadAsBytes_BadDataAsync()
    {
        var json = @"pie";

        var reader = new JsonTextReader(new StringReader(json));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsBytesAsync(); }, "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadAsBytesIntegerArrayWithNoEndAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"[1"));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsBytesAsync(); }, "Unexpected end when reading bytes. Path '[0]', line 1, position 2.");
    }

    [Fact]
    public async Task ReadAsBytesArrayWithBadContentAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"[1.0]"));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsBytesAsync(); }, "Unexpected token when reading bytes: Float. Path '[0]', line 1, position 4.");
    }

    [Fact]
    public async Task ReadAsBytesBadContentAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"new Date()"));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsBytesAsync(); }, "Unexpected character encountered while parsing value: e. Path '', line 1, position 2.");
    }

    [Fact]
    public async Task ReadAsBytes_CommaErrorsAsync()
    {
        var reader = new JsonTextReader(new StringReader("[,'']"));
        await reader.ReadAsync();

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
        {
            await reader.ReadAsBytesAsync();
        }, "Unexpected character encountered while parsing value: ,. Path '[0]', line 1, position 2.");

        Xunit.Assert.Equal(new byte[0], await reader.ReadAsBytesAsync());
        Assert.IsTrue(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadAsBytes_InvalidEndArrayAsync()
    {
        var reader = new JsonTextReader(new StringReader("]"));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
        {
            await reader.ReadAsBytesAsync();
        }, "Unexpected character encountered while parsing value: ]. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadAsBytes_CommaErrors_MultipleAsync()
    {
        var reader = new JsonTextReader(new StringReader("['',,'']"));
        await reader.ReadAsync();
        Xunit.Assert.Equal(new byte[0], await reader.ReadAsBytesAsync());

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
        {
            await reader.ReadAsBytesAsync();
        }, "Unexpected character encountered while parsing value: ,. Path '[1]', line 1, position 5.");

        Xunit.Assert.Equal(new byte[0], await reader.ReadAsBytesAsync());
        Assert.IsTrue(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadBytesWithBadCharacterAsync()
    {
        JsonReader reader = new JsonTextReader(new StringReader(@"true"));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsBytesAsync(); }, "Unexpected character encountered while parsing value: t. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadBytesWithUnexpectedEndAsync()
    {
        var helloWorld = "Hello world!";
        var helloWorldData = Encoding.UTF8.GetBytes(helloWorld);

        JsonReader reader = new JsonTextReader(new StringReader(@"'" + Convert.ToBase64String(helloWorldData)));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsBytesAsync(); }, "Unterminated string. Expected delimiter: '. Path '', line 1, position 17.");
    }

    [Fact]
    public async Task ReadAsDateTime_BadDataAsync()
    {
        var json = @"pie";

        var reader = new JsonTextReader(new StringReader(json));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDateTimeAsync(); }, "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadAsDateTime_BooleanAsync()
    {
        var json = @"true";

        var reader = new JsonTextReader(new StringReader(json));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDateTimeAsync(); }, "Unexpected character encountered while parsing value: t. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetBadContentAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"new Date()"));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDateTimeOffsetAsync(); }, "Unexpected character encountered while parsing value: e. Path '', line 1, position 2.");
    }

    [Fact]
    public async Task ReadAsDecimalBadContentAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"new Date()"));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDecimalAsync(); }, "Unexpected character encountered while parsing value: e. Path '', line 1, position 2.");
    }

    [Fact]
    public async Task ReadAsDecimalBadContent_SecondLineAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"
new Date()"));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsDecimalAsync(); }, "Unexpected character encountered while parsing value: e. Path '', line 2, position 2.");
    }

    [Fact]
    public async Task ReadInt32WithBadCharacterAsync()
    {
        JsonReader reader = new JsonTextReader(new StringReader(@"true"));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsInt32Async(); }, "Unexpected character encountered while parsing value: t. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadNumberValue_CommaErrorsAsync()
    {
        var reader = new JsonTextReader(new StringReader("[,1]"));
        await reader.ReadAsync();

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
        {
            await reader.ReadAsInt32Async();
        }, "Unexpected character encountered while parsing value: ,. Path '[0]', line 1, position 2.");

        Assert.AreEqual(1, await reader.ReadAsInt32Async());
        Assert.IsTrue(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadNumberValue_InvalidEndArrayAsync()
    {
        var reader = new JsonTextReader(new StringReader("]"));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
        {
            await reader.ReadAsInt32Async();
        }, "Unexpected character encountered while parsing value: ]. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadNumberValue_CommaErrors_MultipleAsync()
    {
        var reader = new JsonTextReader(new StringReader("[1,,1]"));
        await reader.ReadAsync();
        await reader.ReadAsInt32Async();

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
        {
            await reader.ReadAsInt32Async();
        }, "Unexpected character encountered while parsing value: ,. Path '[1]', line 1, position 4.");

        Assert.AreEqual(1, await reader.ReadAsInt32Async());
        Assert.IsTrue(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadAsString_UnexpectedEndAsync()
    {
        var json = @"tru";

        var reader = new JsonTextReader(new StringReader(json));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsStringAsync(); }, "Unexpected end when reading JSON. Path '', line 1, position 3.");
    }

    [Fact]
    public async Task ReadAsString_Null_UnexpectedEndAsync()
    {
        var json = @"nul";

        var reader = new JsonTextReader(new StringReader(json));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsStringAsync(); }, "Unexpected end when reading JSON. Path '', line 1, position 3.");
    }

    [Fact]
    public async Task ReadStringValue_InvalidEndArrayAsync()
    {
        var reader = new JsonTextReader(new StringReader("]"));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
        {
            await reader.ReadAsDateTimeAsync();
        }, "Unexpected character encountered while parsing value: ]. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadStringValue_CommaErrorsAsync()
    {
        var reader = new JsonTextReader(new StringReader("[,'']"));
        await reader.ReadAsync();

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
        {
            await reader.ReadAsStringAsync();
        }, "Unexpected character encountered while parsing value: ,. Path '[0]', line 1, position 2.");

        Assert.AreEqual(string.Empty, await reader.ReadAsStringAsync());
        Assert.IsTrue(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadStringValue_CommaErrors_MultipleAsync()
    {
        var reader = new JsonTextReader(new StringReader("['',,'']"));
        await reader.ReadAsync();
        await reader.ReadAsInt32Async();

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
        {
            await reader.ReadAsStringAsync();
        }, "Unexpected character encountered while parsing value: ,. Path '[1]', line 1, position 5.");

        Assert.AreEqual(string.Empty, await reader.ReadAsStringAsync());
        Assert.IsTrue(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadStringValue_Numbers_NotStringAsync()
    {
        var reader = new JsonTextReader(new StringReader("[56,56]"));
        await reader.ReadAsync();

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
        {
            await reader.ReadAsDateTimeAsync();
        }, "Unexpected character encountered while parsing value: 5. Path '', line 1, position 2.");

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
        {
            await reader.ReadAsDateTimeAsync();
        }, "Unexpected character encountered while parsing value: 6. Path '', line 1, position 3.");

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
        {
            await reader.ReadAsDateTimeAsync();
        }, "Unexpected character encountered while parsing value: ,. Path '[0]', line 1, position 4.");

        Assert.AreEqual(56, await reader.ReadAsInt32Async());
        Assert.IsTrue(await reader.ReadAsync());
    }

    [Fact]
    public async Task ErrorReadingCommentAsync()
    {
        var json = @"/";

        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unexpected end while parsing comment. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task EscapedPathInExceptionMessageAsync()
    {
        var json = @"{
  ""frameworks"": {
    ""NET5_0_OR_GREATER"": {
      ""dependencies"": {
        ""System.Xml.ReaderWriter"": {
          ""source"": !!! !!!
        }
      }
    }
  }
}";

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(
            async () =>
            {
                var reader = new JsonTextReader(new StringReader(json));
                while (await reader.ReadAsync())
                {
                }
            },
            "Unexpected character encountered while parsing value: !. Path 'frameworks.NET5_0_OR_GREATER.dependencies['System.Xml.ReaderWriter'].source', line 6, position 20.");
    }

    [Fact]
    public async Task MaxDepthAsync()
    {
        var json = "[[]]";

        var reader = new JsonTextReader(new StringReader(json))
        {
            MaxDepth = 1
        };

        Assert.IsTrue(await reader.ReadAsync());

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { Assert.IsTrue(await reader.ReadAsync()); }, "The reader's MaxDepth of 1 has been exceeded. Path '[0]', line 1, position 2.");
    }

    [Fact]
    public async Task MaxDepthDoesNotRecursivelyErrorAsync()
    {
        var json = "[[[[]]],[[]]]";

        var reader = new JsonTextReader(new StringReader(json))
        {
            MaxDepth = 1
        };

        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual(0, reader.Depth);

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { Assert.IsTrue(await reader.ReadAsync()); }, "The reader's MaxDepth of 1 has been exceeded. Path '[0]', line 1, position 2.");
        Assert.AreEqual(1, reader.Depth);

        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual(2, reader.Depth);

        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual(3, reader.Depth);

        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual(3, reader.Depth);

        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual(2, reader.Depth);

        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual(1, reader.Depth);

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { Assert.IsTrue(await reader.ReadAsync()); }, "The reader's MaxDepth of 1 has been exceeded. Path '[1]', line 1, position 9.");
        Assert.AreEqual(1, reader.Depth);

        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual(2, reader.Depth);

        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual(2, reader.Depth);

        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual(1, reader.Depth);

        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual(0, reader.Depth);

        Assert.IsFalse(await reader.ReadAsync());
    }

    [Fact]
    public async Task UnexpectedEndWhenParsingUnquotedPropertyAsync()
    {
        JsonReader reader = new JsonTextReader(new StringReader(@"{aww"));
        Assert.IsTrue(await reader.ReadAsync());

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await reader.ReadAsync(); }, "Unexpected end while parsing unquoted property name. Path '', line 1, position 4.");
    }
}