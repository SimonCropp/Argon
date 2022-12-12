// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class ExceptionHandlingAsyncTests : TestFixtureBase
{
    [Fact]
    public async Task ReadAsBytes_MissingCommaAsync()
    {
        var data = "Hello world"u8.ToArray();

        var json = $@"['{Convert.ToBase64String(data)}' '{Convert.ToBase64String(data)}']";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(data, await reader.ReadAsBytesAsync());

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsBytesAsync(),
            "After parsing a value an unexpected character was encountered: '. Path '[0]', line 1, position 20.");
    }

    [Fact]
    public async Task ReadAsInt32_MissingCommaAsync()
    {
        var json = "[0 1 2]";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(0, (int) await reader.ReadAsInt32Async());

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsInt32Async(),
            "After parsing a value an unexpected character was encountered: 1. Path '[0]', line 1, position 3.");
    }

    [Fact]
    public async Task ReadAsBoolean_MissingCommaAsync()
    {
        var json = "[true false true]";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        XUnitAssert.True((bool) await reader.ReadAsBooleanAsync());

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsBooleanAsync(),
            "After parsing a value an unexpected character was encountered: f. Path '[0]', line 1, position 6.");
    }

    [Fact]
    public async Task ReadAsDateTime_MissingCommaAsync()
    {
        var json = "['2017-02-04T00:00:00Z' '2018-02-04T00:00:00Z' '2019-02-04T00:00:00Z']";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(new(2017, 2, 4, 0, 0, 0, DateTimeKind.Utc), (DateTime) await reader.ReadAsDateTimeAsync());

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsDateTimeAsync(),
            "After parsing a value an unexpected character was encountered: '. Path '[0]', line 1, position 24.");
    }

    [Fact]
    public async Task ReadAsDateTimeOffset_MissingCommaAsync()
    {
        var json = "['2017-02-04T00:00:00Z' '2018-02-04T00:00:00Z' '2019-02-04T00:00:00Z']";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(new(2017, 2, 4, 0, 0, 0, TimeSpan.Zero), (DateTimeOffset) await reader.ReadAsDateTimeOffsetAsync());

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsDateTimeOffsetAsync(),
            "After parsing a value an unexpected character was encountered: '. Path '[0]', line 1, position 24.");
    }

    [Fact]
    public async Task ReadAsString_MissingCommaAsync()
    {
        var json = "['2017-02-04T00:00:00Z' '2018-02-04T00:00:00Z' '2019-02-04T00:00:00Z']";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.Equal("2017-02-04T00:00:00Z", await reader.ReadAsStringAsync());

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsStringAsync(),
            "After parsing a value an unexpected character was encountered: '. Path '[0]', line 1, position 24.");
    }

    [Fact]
    public async Task Read_MissingCommaAsync()
    {
        var json = "[0 1 2]";
        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.True(await reader.ReadAsync());

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsync(),
            "After parsing a value an unexpected character was encountered: 1. Path '[0]', line 1, position 3.");
    }

    [Fact]
    public Task UnexpectedEndAfterReadingNAsync()
    {
        var reader = new JsonTextReader(new StringReader("n"));
        return XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsync(),
            "Unexpected end when reading JSON. Path '', line 1, position 1.");
    }

    [Fact]
    public Task UnexpectedEndAfterReadingNuAsync()
    {
        var reader = new JsonTextReader(new StringReader("nu"));
        return XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsync(),
            "Unexpected end when reading JSON. Path '', line 1, position 2.");
    }

    [Fact]
    public Task UnexpectedEndOfHexAsync()
    {
        JsonReader reader = new JsonTextReader(new StringReader(@"'h\u123"));

        return XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsync(),
            "Unexpected end while parsing Unicode escape sequence. Path '', line 1, position 4.");
    }

    [Fact]
    public async Task UnexpectedEndOfControlCharacterAsync()
    {
        JsonReader reader = new JsonTextReader(new StringReader(@"'h\"));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(() => reader.ReadAsync(), "Unterminated string. Expected delimiter: '. Path '', line 1, position 3.");
    }

    [Fact]
    public async Task ReadInvalidNonBase10NumberAsync()
    {
        var json = "0aq2dun13.hod";

        var reader = new JsonTextReader(new StringReader(json));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsync(),
            "Unexpected character encountered while parsing number: q. Path '', line 1, position 2.");

        reader = new(new StringReader(json));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsDecimalAsync(),
            "Unexpected character encountered while parsing number: q. Path '', line 1, position 2.");

        reader = new(new StringReader(json));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsInt32Async(),
            "Unexpected character encountered while parsing number: q. Path '', line 1, position 2.");
    }

    [Fact]
    public async Task ThrowErrorWhenParsingUnquoteStringThatStartsWithNEAsync()
    {
        const string json = @"{ ""ItemName"": ""value"", ""u"":netanelsalinger,""r"":9 }";

        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsync(),
            "Unexpected character encountered while parsing value: n. Path 'u', line 1, position 27.");
    }

    [Fact]
    public async Task UnexpectedEndOfStringAsync()
    {
        JsonReader reader = new JsonTextReader(new StringReader("'hi"));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsync(),
            "Unterminated string. Expected delimiter: '. Path '', line 1, position 3.");
    }

    [Fact]
    public async Task UnexpectedEndTokenWhenParsingOddEndTokenAsync()
    {
        JsonReader reader = new JsonTextReader(new StringReader(@"{}}"));
        Assert.True(await reader.ReadAsync());
        Assert.True(await reader.ReadAsync());

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsync(),
            "Additional text encountered after finished reading JSON content: }. Path '', line 1, position 2.");
    }

    [Fact]
    public async Task ResetJsonTextReaderErrorCountAsync()
    {
        var toggleReaderError = new ToggleReaderError(new StringReader("{'first':1,'second':2,'third':3}"));
        var reader = new JsonTextReader(toggleReaderError);

        Assert.True(await reader.ReadAsync());

        toggleReaderError.Error = true;

        await XUnitAssert.ThrowsAsync<Exception>(() => reader.ReadAsync(), "Read error");
        await XUnitAssert.ThrowsAsync<Exception>(() => reader.ReadAsync(), "Read error");

        toggleReaderError.Error = false;

        Assert.True(await reader.ReadAsync());
        Assert.Equal("first", reader.Value);

        toggleReaderError.Error = true;

        await XUnitAssert.ThrowsAsync<Exception>(() => reader.ReadAsync(), "Read error");

        toggleReaderError.Error = false;

        Assert.True(await reader.ReadAsync());
        Assert.Equal(1L, reader.Value);

        toggleReaderError.Error = true;

        await XUnitAssert.ThrowsAsync<Exception>(() => reader.ReadAsync(), "Read error");
        await XUnitAssert.ThrowsAsync<Exception>(() => reader.ReadAsync(), "Read error");
        await XUnitAssert.ThrowsAsync<Exception>(() => reader.ReadAsync(), "Read error");

        toggleReaderError.Error = false;
    }

    [Fact]
    public async Task MatchWithInsufficentCharactersAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"nul"));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsync(),
            "Unexpected end when reading JSON. Path '', line 1, position 3.");
    }

    [Fact]
    public async Task MatchWithWrongCharactersAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"nulz"));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(() => reader.ReadAsync(), "Error parsing null value. Path '', line 1, position 3.");
    }

    [Fact]
    public async Task MatchWithNoTrailingSeparatorAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"nullz"));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(() => reader.ReadAsync(), "Error parsing null value. Path '', line 1, position 4.");
    }

    [Fact]
    public async Task UnclosedCommentAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"/* sdf"));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(() => reader.ReadAsync(), "Unexpected end while parsing comment. Path '', line 1, position 6.");
    }

    [Fact]
    public async Task BadCommentStartAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"/sdf"));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(() => reader.ReadAsync(), "Error parsing comment. Expected: *, got s. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task MissingColonAsync()
    {
        var json = @"{
    ""A"" : true,
    ""B"" """;

        var reader = new JsonTextReader(new StringReader(json));

        await reader.ReadAsync();
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        await reader.ReadAsync();
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsync();
        Assert.Equal(JsonToken.Boolean, reader.TokenType);

        await XUnitAssert.ThrowsAsync<JsonReaderException>(() => reader.ReadAsync(), @"Invalid character after parsing property name. Expected ':' but got: "". Path 'A', line 3, position 8.");
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

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            async () =>
            {
                while (await reader.ReadAsync())
                {
                }
            },
            "Additional text encountered after finished reading JSON content: ,. Path '', line 5, position 1.");
    }

    [Fact]
    public async Task ParseAdditionalContent_TextAsync()
    {
        var json = @"[
""Small"",
""Medium"",
""Large""
]content";

        var reader = new JsonTextReader(new StringReader(json), 2);

        await reader.ReadAsync();
        Assert.Equal(1, reader.LineNumber);

        await reader.ReadAsync();
        Assert.Equal(2, reader.LineNumber);

        await reader.ReadAsync();
        Assert.Equal(3, reader.LineNumber);

        await reader.ReadAsync();
        Assert.Equal(4, reader.LineNumber);

        await reader.ReadAsync();
        Assert.Equal(5, reader.LineNumber);

        await XUnitAssert.ThrowsAsync<JsonReaderException>(() => reader.ReadAsync(), "Additional text encountered after finished reading JSON content: c. Path '', line 5, position 1.");
    }

    [Fact]
    public async Task ParseAdditionalContent_WhitespaceThenTextAsync()
    {
        var json = @"'hi' a";

        var reader = new JsonTextReader(new StringReader(json));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            async () =>
            {
                while (await reader.ReadAsync())
                {
                }
            },
            "Additional text encountered after finished reading JSON content: a. Path '', line 1, position 5.");
    }

    [Fact]
    public async Task ParseIncompleteCommentSeparatorAsync()
    {
        var reader = new JsonTextReader(new StringReader("true/"));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(() => reader.ReadAsync(), "Error parsing boolean value. Path '', line 1, position 4.");
    }

    [Fact]
    public async Task ReadBadCharInArrayAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"[}"));

        await reader.ReadAsync();

        await XUnitAssert.ThrowsAsync<JsonReaderException>(() => reader.ReadAsync(), "Unexpected character encountered while parsing value: }. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadAsBytesNoContentWrappedObjectAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"{"));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsBytesAsync(),
            "Unexpected end when reading JSON. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadBytesEmptyWrappedObjectAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"{}"));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsBytesAsync(),
            "Error reading bytes. Unexpected token: StartObject. Path '', line 1, position 2.");
    }

    [Fact]
    public async Task ReadIntegerWithErrorAsync()
    {
        var json = @"{
    ChildId: 333333333333333333333333333333333333333
}";

        var jsonTextReader = new JsonTextReader(new StringReader(json));

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, jsonTextReader.TokenType);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, jsonTextReader.TokenType);

        await XUnitAssert.ThrowsAsync<JsonReaderException>(() => jsonTextReader.ReadAsInt32Async(), "JSON integer 333333333333333333333333333333333333333 is too large or small for an Int32. Path 'ChildId', line 2, position 52.");

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, jsonTextReader.TokenType);

        Assert.False(await jsonTextReader.ReadAsync());
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

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.StartArray, jsonTextReader.TokenType);

        await XUnitAssert.ThrowsAsync<JsonReaderException>(() => jsonTextReader.ReadAsInt32Async(), "JSON integer 333333333333333333333333333333333333333 is too large or small for an Int32. Path '[0]', line 2, position 41.");

        await XUnitAssert.ThrowsAsync<JsonReaderException>(() => jsonTextReader.ReadAsInt32Async(), "Input string '3.3' is not a valid integer. Path '[1]', line 3, position 5.");

        await XUnitAssert.ThrowsAsync<JsonReaderException>(() => jsonTextReader.ReadAsInt32Async(), "Unexpected character encountered while parsing value: ,. Path '[2]', line 4, position 3.");

        await XUnitAssert.ThrowsAsync<JsonReaderException>(() => jsonTextReader.ReadAsInt32Async(), "Input string '0f' is not a valid integer. Path '[3]', line 5, position 4.");

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.EndArray, jsonTextReader.TokenType);

        Assert.False(await jsonTextReader.ReadAsync());
    }

    [Fact]
    public async Task ReadBytesWithErrorAsync()
    {
        var json = @"{
    ChildId: '123'
}";

        var jsonTextReader = new JsonTextReader(new StringReader(json));

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, jsonTextReader.TokenType);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, jsonTextReader.TokenType);

        try
        {
            await jsonTextReader.ReadAsBytesAsync();
        }
        catch (FormatException)
        {
        }

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, jsonTextReader.TokenType);

        Assert.False(await jsonTextReader.ReadAsync());
    }

    [Fact]
    public async Task ReadInt32OverflowAsync()
    {
        long i = int.MaxValue;

        var reader = new JsonTextReader(new StringReader(i.ToString(InvariantCulture)));
        await reader.ReadAsync();
        Assert.Equal(typeof(long), reader.ValueType);

        for (var j = 1; j < 1000; j++)
        {
            var total = j + i;
            await XUnitAssert.ThrowsAsync<JsonReaderException>(
                async () =>
                {
                    reader = new(new StringReader(total.ToString(InvariantCulture)));
                    await reader.ReadAsInt32Async();
                },
                $"JSON integer {total} is too large or small for an Int32. Path '', line 1, position 10.");
        }
    }

    [Fact]
    public async Task ReadInt32Overflow_NegativeAsync()
    {
        long i = int.MinValue;

        var reader = new JsonTextReader(new StringReader(i.ToString(InvariantCulture)));
        await reader.ReadAsync();
        Assert.Equal(typeof(long), reader.ValueType);
        Assert.Equal(i, reader.Value);

        for (var j = 1; j < 1000; j++)
        {
            var total = -j + i;
            await XUnitAssert.ThrowsAsync<JsonReaderException>(
                async () =>
                {
                    reader = new(new StringReader(total.ToString(InvariantCulture)));
                    await reader.ReadAsInt32Async();
                },
                $"JSON integer {total} is too large or small for an Int32. Path '', line 1, position 11.");
        }
    }

    [Fact]
    public async Task ReadInt64OverflowAsync()
    {
        var i = new BigInteger(long.MaxValue);

        var reader = new JsonTextReader(new StringReader(i.ToString(InvariantCulture)));
        await reader.ReadAsync();
        Assert.Equal(typeof(long), reader.ValueType);

        for (var j = 1; j < 1000; j++)
        {
            var total = i + j;

            reader = new(new StringReader(total.ToString(InvariantCulture)));
            await reader.ReadAsync();

            Assert.Equal(typeof(BigInteger), reader.ValueType);
        }
    }

    [Fact]
    public async Task ReadInt64Overflow_NegativeAsync()
    {
        var i = new BigInteger(long.MinValue);

        var reader = new JsonTextReader(new StringReader(i.ToString(InvariantCulture)));
        await reader.ReadAsync();
        Assert.Equal(typeof(long), reader.ValueType);

        for (var j = 1; j < 1000; j++)
        {
            var total = i + -j;

            reader = new(new StringReader(total.ToString(InvariantCulture)));
            await reader.ReadAsync();

            Assert.Equal(typeof(BigInteger), reader.ValueType);
        }
    }

    [Fact]
    public async Task ReadAsString_Null_AdditionalBadDataAsync()
    {
        var json = @"nullllll";

        var reader = new JsonTextReader(new StringReader(json));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsStringAsync(),
            "Error parsing null value. Path '', line 1, position 4.");
    }

    [Fact]
    public async Task ReadAsBoolean_AdditionalBadDataAsync()
    {
        var json = @"falseeeee";

        var reader = new JsonTextReader(new StringReader(json));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsBooleanAsync(),
            "Unexpected character encountered while parsing value: e. Path '', line 1, position 5.");
    }

    [Fact]
    public async Task ReadAsString_AdditionalBadDataAsync()
    {
        var json = @"falseeeee";

        var reader = new JsonTextReader(new StringReader(json));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsStringAsync(),
            "Unexpected character encountered while parsing value: e. Path '', line 1, position 5.");
    }

    [Fact]
    public async Task ReadAsBoolean_UnexpectedEndAsync()
    {
        var json = @"tru";

        var reader = new JsonTextReader(new StringReader(json));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsBooleanAsync(),
            "Unexpected end when reading JSON. Path '', line 1, position 3.");
    }

    [Fact]
    public async Task ReadAsBoolean_BadDataAsync()
    {
        var json = @"pie";

        var reader = new JsonTextReader(new StringReader(json));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsBooleanAsync(),
            "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadAsString_BadDataAsync()
    {
        var json = @"pie";

        var reader = new JsonTextReader(new StringReader(json));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsStringAsync(),
            "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadAsDouble_BadDataAsync()
    {
        var json = @"pie";

        var reader = new JsonTextReader(new StringReader(json));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsDoubleAsync(),
            "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadAsDouble_BooleanAsync()
    {
        var json = @"true";

        var reader = new JsonTextReader(new StringReader(json));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsDoubleAsync(),
            "Unexpected character encountered while parsing value: t. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadAsBytes_BadDataAsync()
    {
        var json = @"pie";

        var reader = new JsonTextReader(new StringReader(json));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsBytesAsync(),
            "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadAsBytesIntegerArrayWithNoEndAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"[1"));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsBytesAsync(),
            "Unexpected end when reading bytes. Path '[0]', line 1, position 2.");
    }

    [Fact]
    public async Task ReadAsBytesArrayWithBadContentAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"[1.0]"));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsBytesAsync(),
            "Unexpected token when reading bytes: Float. Path '[0]', line 1, position 4.");
    }

    [Fact]
    public async Task ReadAsBytes_CommaErrorsAsync()
    {
        var reader = new JsonTextReader(new StringReader("[,'']"));
        await reader.ReadAsync();

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsBytesAsync(),
            "Unexpected character encountered while parsing value: ,. Path '[0]', line 1, position 2.");

        Assert.Equal(Array.Empty<byte>(), await reader.ReadAsBytesAsync());
        Assert.True(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadAsBytes_InvalidEndArrayAsync()
    {
        var reader = new JsonTextReader(new StringReader("]"));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsBytesAsync(),
            "Unexpected character encountered while parsing value: ]. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadAsBytes_CommaErrors_MultipleAsync()
    {
        var reader = new JsonTextReader(new StringReader("['',,'']"));
        await reader.ReadAsync();
        Assert.Equal(Array.Empty<byte>(), await reader.ReadAsBytesAsync());

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsBytesAsync(),
            "Unexpected character encountered while parsing value: ,. Path '[1]', line 1, position 5.");

        Assert.Equal(Array.Empty<byte>(), await reader.ReadAsBytesAsync());
        Assert.True(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadBytesWithBadCharacterAsync()
    {
        JsonReader reader = new JsonTextReader(new StringReader(@"true"));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsBytesAsync(),
            "Unexpected character encountered while parsing value: t. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadBytesWithUnexpectedEndAsync()
    {
        var helloWorld = "Hello world!";
        var helloWorldData = Encoding.UTF8.GetBytes(helloWorld);

        JsonReader reader = new JsonTextReader(new StringReader($@"'{Convert.ToBase64String(helloWorldData)}"));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsBytesAsync(),
            "Unterminated string. Expected delimiter: '. Path '', line 1, position 17.");
    }

    [Fact]
    public async Task ReadAsDateTime_BadDataAsync()
    {
        var json = @"pie";

        var reader = new JsonTextReader(new StringReader(json));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsDateTimeAsync(),
            "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadAsDateTime_BooleanAsync()
    {
        var json = @"true";

        var reader = new JsonTextReader(new StringReader(json));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsDateTimeAsync(),
            "Unexpected character encountered while parsing value: t. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadAsDecimalBadContent_SecondLineAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"
new Date()"));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsDecimalAsync(),
            "Unexpected character encountered while parsing value: e. Path '', line 2, position 2.");
    }

    [Fact]
    public async Task ReadInt32WithBadCharacterAsync()
    {
        JsonReader reader = new JsonTextReader(new StringReader(@"true"));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsInt32Async(),
            "Unexpected character encountered while parsing value: t. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadNumberValue_CommaErrorsAsync()
    {
        var reader = new JsonTextReader(new StringReader("[,1]"));
        await reader.ReadAsync();

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsInt32Async(),
            "Unexpected character encountered while parsing value: ,. Path '[0]', line 1, position 2.");

        Assert.Equal(1, await reader.ReadAsInt32Async());
        Assert.True(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadNumberValue_InvalidEndArrayAsync()
    {
        var reader = new JsonTextReader(new StringReader("]"));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsInt32Async(),
            "Unexpected character encountered while parsing value: ]. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadNumberValue_CommaErrors_MultipleAsync()
    {
        var reader = new JsonTextReader(new StringReader("[1,,1]"));
        await reader.ReadAsync();
        await reader.ReadAsInt32Async();

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsInt32Async(),
            "Unexpected character encountered while parsing value: ,. Path '[1]', line 1, position 4.");

        Assert.Equal(1, await reader.ReadAsInt32Async());
        Assert.True(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadAsString_UnexpectedEndAsync()
    {
        var json = @"tru";

        var reader = new JsonTextReader(new StringReader(json));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsStringAsync(),
            "Unexpected end when reading JSON. Path '', line 1, position 3.");
    }

    [Fact]
    public async Task ReadAsString_Null_UnexpectedEndAsync()
    {
        var json = @"nul";

        var reader = new JsonTextReader(new StringReader(json));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsStringAsync(),
            "Unexpected end when reading JSON. Path '', line 1, position 3.");
    }

    [Fact]
    public async Task ReadStringValue_InvalidEndArrayAsync()
    {
        var reader = new JsonTextReader(new StringReader("]"));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsDateTimeAsync(),
            "Unexpected character encountered while parsing value: ]. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadStringValue_CommaErrorsAsync()
    {
        var reader = new JsonTextReader(new StringReader("[,'']"));
        await reader.ReadAsync();

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsStringAsync(),
            "Unexpected character encountered while parsing value: ,. Path '[0]', line 1, position 2.");

        Assert.Equal(string.Empty, await reader.ReadAsStringAsync());
        Assert.True(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadStringValue_CommaErrors_MultipleAsync()
    {
        var reader = new JsonTextReader(new StringReader("['',,'']"));
        await reader.ReadAsync();
        await reader.ReadAsInt32Async();

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsStringAsync(),
            "Unexpected character encountered while parsing value: ,. Path '[1]', line 1, position 5.");

        Assert.Equal(string.Empty, await reader.ReadAsStringAsync());
        Assert.True(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadStringValue_Numbers_NotStringAsync()
    {
        var reader = new JsonTextReader(new StringReader("[56,56]"));
        await reader.ReadAsync();

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsDateTimeAsync(),
            "Unexpected character encountered while parsing value: 5. Path '', line 1, position 2.");

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsDateTimeAsync(),
            "Unexpected character encountered while parsing value: 6. Path '', line 1, position 3.");

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsDateTimeAsync(),
            "Unexpected character encountered while parsing value: ,. Path '[0]', line 1, position 4.");

        Assert.Equal(56, await reader.ReadAsInt32Async());
        Assert.True(await reader.ReadAsync());
    }

    [Fact]
    public async Task ErrorReadingCommentAsync()
    {
        var json = @"/";

        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        await XUnitAssert.ThrowsAsync<JsonReaderException>(() => reader.ReadAsync(), "Unexpected end while parsing comment. Path '', line 1, position 1.");
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

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
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

        Assert.True(await reader.ReadAsync());

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            async () => Assert.True(await reader.ReadAsync()),
            "The reader's MaxDepth of 1 has been exceeded. Path '[0]', line 1, position 2.");
    }

    [Fact]
    public async Task MaxDepthDoesNotRecursivelyErrorAsync()
    {
        var json = "[[[[]]],[[]]]";

        var reader = new JsonTextReader(new StringReader(json))
        {
            MaxDepth = 1
        };

        Assert.True(await reader.ReadAsync());
        Assert.Equal(0, reader.Depth);

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            async () =>
            {
                Assert.True(await reader.ReadAsync());
            },
            "The reader's MaxDepth of 1 has been exceeded. Path '[0]', line 1, position 2.");
        Assert.Equal(1, reader.Depth);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(2, reader.Depth);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(3, reader.Depth);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(3, reader.Depth);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(2, reader.Depth);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(1, reader.Depth);

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            async () =>
            {
                Assert.True(await reader.ReadAsync());
            },
            "The reader's MaxDepth of 1 has been exceeded. Path '[1]', line 1, position 9.");
        Assert.Equal(1, reader.Depth);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(2, reader.Depth);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(2, reader.Depth);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(1, reader.Depth);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(0, reader.Depth);

        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task UnexpectedEndWhenParsingUnquotedPropertyAsync()
    {
        JsonReader reader = new JsonTextReader(new StringReader(@"{aww"));
        Assert.True(await reader.ReadAsync());

        await XUnitAssert.ThrowsAsync<JsonReaderException>(() => reader.ReadAsync(), "Unexpected end while parsing unquoted property name. Path '', line 1, position 4.");
    }
}