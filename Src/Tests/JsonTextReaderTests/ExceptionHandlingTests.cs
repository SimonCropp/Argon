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

public class ExceptionHandlingTests : TestFixtureBase
{
    [Fact]
    public void ReadAsBytes_MissingComma()
    {
        var data = Encoding.UTF8.GetBytes("Hello world");

        var json = @"['" + Convert.ToBase64String(data) + "' '" + Convert.ToBase64String(data) + @"']";
        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(data, reader.ReadAsBytes());

        ExceptionAssert.Throws<JsonReaderException>(
            () => reader.ReadAsBytes(),
            "After parsing a value an unexpected character was encountered: '. Path '[0]', line 1, position 20.");
    }

    [Fact]
    public void ReadAsInt32_MissingComma()
    {
        var json = "[0 1 2]";
        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(0, (int)reader.ReadAsInt32());

        ExceptionAssert.Throws<JsonReaderException>(
            () => reader.ReadAsInt32(),
            "After parsing a value an unexpected character was encountered: 1. Path '[0]', line 1, position 3.");
    }

    [Fact]
    public void ReadAsBoolean_MissingComma()
    {
        var json = "[true false true]";
        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        XUnitAssert.True((bool)reader.ReadAsBoolean());

        ExceptionAssert.Throws<JsonReaderException>(
            () => reader.ReadAsBoolean(),
            "After parsing a value an unexpected character was encountered: f. Path '[0]', line 1, position 6.");
    }

    [Fact]
    public void ReadAsDateTime_MissingComma()
    {
        var json = "['2017-02-04T00:00:00Z' '2018-02-04T00:00:00Z' '2019-02-04T00:00:00Z']";
        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(new DateTime(2017, 2, 4, 0, 0, 0, DateTimeKind.Utc), (DateTime)reader.ReadAsDateTime());

        ExceptionAssert.Throws<JsonReaderException>(
            () => reader.ReadAsDateTime(),
            "After parsing a value an unexpected character was encountered: '. Path '[0]', line 1, position 24.");
    }

    [Fact]
    public void ReadAsDateTimeOffset_MissingComma()
    {
        var json = "['2017-02-04T00:00:00Z' '2018-02-04T00:00:00Z' '2019-02-04T00:00:00Z']";
        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(new DateTimeOffset(2017, 2, 4, 0, 0, 0, TimeSpan.Zero), (DateTimeOffset)reader.ReadAsDateTimeOffset());

        ExceptionAssert.Throws<JsonReaderException>(
            () => reader.ReadAsDateTimeOffset(),
            "After parsing a value an unexpected character was encountered: '. Path '[0]', line 1, position 24.");
    }

    [Fact]
    public void ReadAsString_MissingComma()
    {
        var json = "['2017-02-04T00:00:00Z' '2018-02-04T00:00:00Z' '2019-02-04T00:00:00Z']";
        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal("2017-02-04T00:00:00Z", reader.ReadAsString());

        ExceptionAssert.Throws<JsonReaderException>(
            () => reader.ReadAsString(),
            "After parsing a value an unexpected character was encountered: '. Path '[0]', line 1, position 24.");
    }

    [Fact]
    public void Read_MissingComma()
    {
        var json = "[0 1 2]";
        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.True(reader.Read());

        ExceptionAssert.Throws<JsonReaderException>(
            () => reader.Read(),
            "After parsing a value an unexpected character was encountered: 1. Path '[0]', line 1, position 3.");
    }

    [Fact]
    public void UnexpectedEndAfterReadingN()
    {
        var reader = new JsonTextReader(new StringReader("n"));
        ExceptionAssert.Throws<JsonReaderException>(() => reader.Read(), "Unexpected end when reading JSON. Path '', line 1, position 1.");
    }

    [Fact]
    public void UnexpectedEndAfterReadingNu()
    {
        var reader = new JsonTextReader(new StringReader("nu"));
        ExceptionAssert.Throws<JsonReaderException>(() => reader.Read(), "Unexpected end when reading JSON. Path '', line 1, position 2.");
    }

    [Fact]
    public void UnexpectedEndAfterReadingNe()
    {
        var reader = new JsonTextReader(new StringReader("ne"));
        ExceptionAssert.Throws<JsonReaderException>(() => reader.Read(), "Unexpected end when reading JSON. Path '', line 1, position 2.");
    }

    [Fact]
    public void UnexpectedEndOfHex()
    {
        JsonReader reader = new JsonTextReader(new StringReader(@"'h\u123"));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected end while parsing Unicode escape sequence. Path '', line 1, position 4.");
    }

    [Fact]
    public void UnexpectedEndOfControlCharacter()
    {
        JsonReader reader = new JsonTextReader(new StringReader(@"'h\"));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unterminated string. Expected delimiter: '. Path '', line 1, position 3.");
    }

    [Fact]
    public void ReadInvalidNonBase10Number()
    {
        var json = "0aq2dun13.hod";

        var reader = new JsonTextReader(new StringReader(json));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected character encountered while parsing number: q. Path '', line 1, position 2.");

        reader = new JsonTextReader(new StringReader(json));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDecimal(); }, "Unexpected character encountered while parsing number: q. Path '', line 1, position 2.");

        reader = new JsonTextReader(new StringReader(json));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsInt32(); }, "Unexpected character encountered while parsing number: q. Path '', line 1, position 2.");
    }

    [Fact]
    public void ThrowErrorWhenParsingUnquoteStringThatStartsWithNE()
    {
        const string json = @"{ ""ItemName"": ""value"", ""u"":netanelsalinger,""r"":9 }";

        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected content while parsing JSON. Path 'u', line 1, position 29.");
    }

    [Fact]
    public void UnexpectedEndOfString()
    {
        JsonReader reader = new JsonTextReader(new StringReader("'hi"));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unterminated string. Expected delimiter: '. Path '', line 1, position 3.");
    }

    [Fact]
    public void UnexpectedEndTokenWhenParsingOddEndToken()
    {
        JsonReader reader = new JsonTextReader(new StringReader(@"{}}"));
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.True(reader.Read());

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Additional text encountered after finished reading JSON content: }. Path '', line 1, position 2.");
    }

    [Fact]
    public void ResetJsonTextReaderErrorCount()
    {
        var toggleReaderError = new ToggleReaderError(new StringReader("{'first':1,'second':2,'third':3}"));
        var jsonTextReader = new JsonTextReader(toggleReaderError);

        Xunit.Assert.True(jsonTextReader.Read());

        toggleReaderError.Error = true;

        ExceptionAssert.Throws<Exception>(() => jsonTextReader.Read(), "Read error");
        ExceptionAssert.Throws<Exception>(() => jsonTextReader.Read(), "Read error");

        toggleReaderError.Error = false;

        Xunit.Assert.True(jsonTextReader.Read());
        Xunit.Assert.Equal("first", jsonTextReader.Value);

        toggleReaderError.Error = true;

        ExceptionAssert.Throws<Exception>(() => jsonTextReader.Read(), "Read error");

        toggleReaderError.Error = false;

        Xunit.Assert.True(jsonTextReader.Read());
        Xunit.Assert.Equal(1L, jsonTextReader.Value);

        toggleReaderError.Error = true;

        ExceptionAssert.Throws<Exception>(() => jsonTextReader.Read(), "Read error");
        ExceptionAssert.Throws<Exception>(() => jsonTextReader.Read(), "Read error");
        ExceptionAssert.Throws<Exception>(() => jsonTextReader.Read(), "Read error");

        toggleReaderError.Error = false;
    }

    [Fact]
    public void MatchWithInsufficentCharacters()
    {
        var reader = new JsonTextReader(new StringReader(@"nul"));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected end when reading JSON. Path '', line 1, position 3.");
    }

    [Fact]
    public void MatchWithWrongCharacters()
    {
        var reader = new JsonTextReader(new StringReader(@"nulz"));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Error parsing null value. Path '', line 1, position 3.");
    }

    [Fact]
    public void MatchWithNoTrailingSeparator()
    {
        var reader = new JsonTextReader(new StringReader(@"nullz"));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Error parsing null value. Path '', line 1, position 4.");
    }

    [Fact]
    public void UnclosedComment()
    {
        var reader = new JsonTextReader(new StringReader(@"/* sdf"));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected end while parsing comment. Path '', line 1, position 6.");
    }

    [Fact]
    public void BadCommentStart()
    {
        var reader = new JsonTextReader(new StringReader(@"/sdf"));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Error parsing comment. Expected: *, got s. Path '', line 1, position 1.");
    }

    [Fact]
    public void MissingColon()
    {
        var json = @"{
    ""A"" : true,
    ""B"" """;

        var reader = new JsonTextReader(new StringReader(json));

        reader.Read();
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        reader.Read();
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        reader.Read();
        Xunit.Assert.Equal(JsonToken.Boolean, reader.TokenType);

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, @"Invalid character after parsing property name. Expected ':' but got: "". Path 'A', line 3, position 8.");
    }

    [Fact]
    public void NullTextReader()
    {
        ExceptionAssert.Throws<ArgumentNullException>(
            () => { new JsonTextReader(null); },
            new[]
            {
                "Value cannot be null." + Environment.NewLine + "Parameter name: reader",
                "Argument cannot be null." + Environment.NewLine + "Parameter name: reader", // Mono
                "Value cannot be null. (Parameter 'reader')"
            });
    }

    [Fact]
    public void ParseConstructorWithBadCharacter()
    {
        var json = "new Date,()";
        var reader = new JsonTextReader(new StringReader(json));

        ExceptionAssert.Throws<JsonReaderException>(() => { Xunit.Assert.True(reader.Read()); }, "Unexpected character while parsing constructor: ,. Path '', line 1, position 8.");
    }

    [Fact]
    public void ParseConstructorWithUnexpectedEnd()
    {
        var json = "new Dat";
        var reader = new JsonTextReader(new StringReader(json));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected end while parsing constructor. Path '', line 1, position 7.");
    }

    [Fact]
    public void ParseConstructorWithUnexpectedCharacter()
    {
        var json = "new Date !";
        var reader = new JsonTextReader(new StringReader(json));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected character while parsing constructor: !. Path '', line 1, position 9.");
    }

    [Fact]
    public void ParseAdditionalContent_Comma()
    {
        var json = @"[
""Small"",
""Medium"",
""Large""
],";

        var reader = new JsonTextReader(new StringReader(json));

        ExceptionAssert.Throws<JsonReaderException>(() =>
        {
            while (reader.Read())
            {
            }
        }, "Additional text encountered after finished reading JSON content: ,. Path '', line 5, position 1.");
    }

    [Fact]
    public void ParseAdditionalContent_Text()
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

        reader.Read();
        Xunit.Assert.Equal(1, reader.LineNumber);

        reader.Read();
        Xunit.Assert.Equal(2, reader.LineNumber);

        reader.Read();
        Xunit.Assert.Equal(3, reader.LineNumber);

        reader.Read();
        Xunit.Assert.Equal(4, reader.LineNumber);

        reader.Read();
        Xunit.Assert.Equal(5, reader.LineNumber);

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Additional text encountered after finished reading JSON content: c. Path '', line 5, position 1.");
    }

    [Fact]
    public void ParseAdditionalContent_WhitespaceThenText()
    {
        var json = @"'hi' a";

        var reader = new JsonTextReader(new StringReader(json));

        ExceptionAssert.Throws<JsonReaderException>(() =>
        {
            while (reader.Read())
            {
            }
        }, "Additional text encountered after finished reading JSON content: a. Path '', line 1, position 5.");
    }

    [Fact]
    public void ParseIncompleteCommentSeparator()
    {
        var reader = new JsonTextReader(new StringReader("true/"));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Error parsing boolean value. Path '', line 1, position 4.");
    }

    [Fact]
    public void ReadBadCharInArray()
    {
        var reader = new JsonTextReader(new StringReader(@"[}"));

        reader.Read();

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected character encountered while parsing value: }. Path '', line 1, position 1.");
    }

    [Fact]
    public void ReadAsBytesNoContentWrappedObject()
    {
        var reader = new JsonTextReader(new StringReader(@"{"));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsBytes(); }, "Unexpected end when reading JSON. Path '', line 1, position 1.");
    }

    [Fact]
    public void ReadBytesEmptyWrappedObject()
    {
        var reader = new JsonTextReader(new StringReader(@"{}"));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsBytes(); }, "Error reading bytes. Unexpected token: StartObject. Path '', line 1, position 2." );
    }

    [Fact]
    public void ReadIntegerWithError()
    {
        var json = @"{
    ChildId: 333333333333333333333333333333333333333
}";

        var jsonTextReader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(jsonTextReader.Read());
        Xunit.Assert.Equal(JsonToken.StartObject, jsonTextReader.TokenType);

        Xunit.Assert.True(jsonTextReader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, jsonTextReader.TokenType);

        ExceptionAssert.Throws<JsonReaderException>(() => jsonTextReader.ReadAsInt32(), "JSON integer 333333333333333333333333333333333333333 is too large or small for an Int32. Path 'ChildId', line 2, position 52.");

        Xunit.Assert.True(jsonTextReader.Read());
        Xunit.Assert.Equal(JsonToken.EndObject, jsonTextReader.TokenType);

        Xunit.Assert.False(jsonTextReader.Read());
    }

    [Fact]
    public void ReadIntegerWithErrorInArray()
    {
        var json = @"[
  333333333333333333333333333333333333333,
  3.3,
  ,
  0f
]";

        var jsonTextReader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(jsonTextReader.Read());
        Xunit.Assert.Equal(JsonToken.StartArray, jsonTextReader.TokenType);

        ExceptionAssert.Throws<JsonReaderException>(() => jsonTextReader.ReadAsInt32(), "JSON integer 333333333333333333333333333333333333333 is too large or small for an Int32. Path '[0]', line 2, position 41.");

        ExceptionAssert.Throws<JsonReaderException>(() => jsonTextReader.ReadAsInt32(), "Input string '3.3' is not a valid integer. Path '[1]', line 3, position 5.");

        ExceptionAssert.Throws<JsonReaderException>(() => jsonTextReader.ReadAsInt32(), "Unexpected character encountered while parsing value: ,. Path '[2]', line 4, position 3.");

        ExceptionAssert.Throws<JsonReaderException>(() => jsonTextReader.ReadAsInt32(), "Input string '0f' is not a valid integer. Path '[3]', line 5, position 4.");

        Xunit.Assert.True(jsonTextReader.Read());
        Xunit.Assert.Equal(JsonToken.EndArray, jsonTextReader.TokenType);

        Xunit.Assert.False(jsonTextReader.Read());
    }

    [Fact]
    public void ReadBytesWithError()
    {
        var json = @"{
    ChildId: '123'
}";

        var jsonTextReader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(jsonTextReader.Read());
        Xunit.Assert.Equal(JsonToken.StartObject, jsonTextReader.TokenType);

        Xunit.Assert.True(jsonTextReader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, jsonTextReader.TokenType);

        try
        {
            jsonTextReader.ReadAsBytes();
        }
        catch (FormatException)
        {
        }

        Xunit.Assert.True(jsonTextReader.Read());
        Xunit.Assert.Equal(JsonToken.EndObject, jsonTextReader.TokenType);

        Xunit.Assert.False(jsonTextReader.Read());
    }

    [Fact]
    public void ReadInt32Overflow()
    {
        long i = int.MaxValue;

        var reader = new JsonTextReader(new StringReader(i.ToString(CultureInfo.InvariantCulture)));
        reader.Read();
        Xunit.Assert.Equal(typeof(long), reader.ValueType);

        for (var j = 1; j < 1000; j++)
        {
            var total = j + i;
            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                reader = new JsonTextReader(new StringReader(total.ToString(CultureInfo.InvariantCulture)));
                reader.ReadAsInt32();
            }, "JSON integer " + total + " is too large or small for an Int32. Path '', line 1, position 10.");
        }
    }

    [Fact]
    public void ReadInt32Overflow_Negative()
    {
        long i = int.MinValue;

        var reader = new JsonTextReader(new StringReader(i.ToString(CultureInfo.InvariantCulture)));
        reader.Read();
        Xunit.Assert.Equal(typeof(long), reader.ValueType);
        Xunit.Assert.Equal(i, reader.Value);

        for (var j = 1; j < 1000; j++)
        {
            var total = -j + i;
            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                reader = new JsonTextReader(new StringReader(total.ToString(CultureInfo.InvariantCulture)));
                reader.ReadAsInt32();
            }, "JSON integer " + total + " is too large or small for an Int32. Path '', line 1, position 11.");
        }
    }

    [Fact]
    public void ReadInt64Overflow()
    {
        var i = new BigInteger(long.MaxValue);

        var reader = new JsonTextReader(new StringReader(i.ToString(CultureInfo.InvariantCulture)));
        reader.Read();
        Xunit.Assert.Equal(typeof(long), reader.ValueType);

        for (var j = 1; j < 1000; j++)
        {
            var total = i + j;

            reader = new JsonTextReader(new StringReader(total.ToString(CultureInfo.InvariantCulture)));
            reader.Read();

            Xunit.Assert.Equal(typeof(BigInteger), reader.ValueType);
        }
    }

    [Fact]
    public void ReadInt64Overflow_Negative()
    {
        var i = new BigInteger(long.MinValue);

        var reader = new JsonTextReader(new StringReader(i.ToString(CultureInfo.InvariantCulture)));
        reader.Read();
        Xunit.Assert.Equal(typeof(long), reader.ValueType);

        for (var j = 1; j < 1000; j++)
        {
            var total = i + -j;

            reader = new JsonTextReader(new StringReader(total.ToString(CultureInfo.InvariantCulture)));
            reader.Read();

            Xunit.Assert.Equal(typeof(BigInteger), reader.ValueType);
        }
    }

    [Fact]
    public void ReadAsString_Null_AdditionalBadData()
    {
        var json = @"nullllll";

        var reader = new JsonTextReader(new StringReader(json));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsString(); }, "Error parsing null value. Path '', line 1, position 4.");
    }

    [Fact]
    public void ReadAsBoolean_AdditionalBadData()
    {
        var json = @"falseeeee";

        var reader = new JsonTextReader(new StringReader(json));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsBoolean(); }, "Unexpected character encountered while parsing value: e. Path '', line 1, position 5.");
    }

    [Fact]
    public void ReadAsString_AdditionalBadData()
    {
        var json = @"falseeeee";

        var reader = new JsonTextReader(new StringReader(json));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsString(); }, "Unexpected character encountered while parsing value: e. Path '', line 1, position 5.");
    }

    [Fact]
    public void ReadAsBoolean_UnexpectedEnd()
    {
        var json = @"tru";

        var reader = new JsonTextReader(new StringReader(json));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsBoolean(); }, "Unexpected end when reading JSON. Path '', line 1, position 3.");
    }

    [Fact]
    public void ReadAsBoolean_BadData()
    {
        var json = @"pie";

        var reader = new JsonTextReader(new StringReader(json));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsBoolean(); }, "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
    }

    [Fact]
    public void ReadAsString_BadData()
    {
        var json = @"pie";

        var reader = new JsonTextReader(new StringReader(json));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsString(); }, "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
    }

    [Fact]
    public void ReadAsDouble_BadData()
    {
        var json = @"pie";

        var reader = new JsonTextReader(new StringReader(json));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDouble(); }, "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
    }

    [Fact]
    public void ReadAsDouble_Boolean()
    {
        var json = @"true";

        var reader = new JsonTextReader(new StringReader(json));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDouble(); }, "Unexpected character encountered while parsing value: t. Path '', line 1, position 1.");
    }

    [Fact]
    public void ReadAsBytes_BadData()
    {
        var json = @"pie";

        var reader = new JsonTextReader(new StringReader(json));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsBytes(); }, "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
    }

    [Fact]
    public void ReadAsBytesIntegerArrayWithNoEnd()
    {
        var reader = new JsonTextReader(new StringReader(@"[1"));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsBytes(); }, "Unexpected end when reading bytes. Path '[0]', line 1, position 2.");
    }

    [Fact]
    public void ReadAsBytesArrayWithBadContent()
    {
        var reader = new JsonTextReader(new StringReader(@"[1.0]"));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsBytes(); }, "Unexpected token when reading bytes: Float. Path '[0]', line 1, position 4.");
    }

    [Fact]
    public void ReadAsBytesBadContent()
    {
        var reader = new JsonTextReader(new StringReader(@"new Date()"));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsBytes(); }, "Unexpected character encountered while parsing value: e. Path '', line 1, position 2.");
    }

    [Fact]
    public void ReadAsBytes_CommaErrors()
    {
        var reader = new JsonTextReader(new StringReader("[,'']"));
        reader.Read();

        ExceptionAssert.Throws<JsonReaderException>(() =>
        {
            reader.ReadAsBytes();
        }, "Unexpected character encountered while parsing value: ,. Path '[0]', line 1, position 2.");

        Xunit.Assert.Equal(new byte[0], reader.ReadAsBytes());
        Xunit.Assert.True(reader.Read());
    }

    [Fact]
    public void ReadAsBytes_InvalidEndArray()
    {
        var reader = new JsonTextReader(new StringReader("]"));

        ExceptionAssert.Throws<JsonReaderException>(() =>
        {
            reader.ReadAsBytes();
        }, "Unexpected character encountered while parsing value: ]. Path '', line 1, position 1.");
    }

    [Fact]
    public void ReadAsBytes_CommaErrors_Multiple()
    {
        var reader = new JsonTextReader(new StringReader("['',,'']"));
        reader.Read();
        Xunit.Assert.Equal(new byte[0], reader.ReadAsBytes());

        ExceptionAssert.Throws<JsonReaderException>(() =>
        {
            reader.ReadAsBytes();
        }, "Unexpected character encountered while parsing value: ,. Path '[1]', line 1, position 5.");

        Xunit.Assert.Equal(new byte[0], reader.ReadAsBytes());
        Xunit.Assert.True(reader.Read());
    }

    [Fact]
    public void ReadBytesWithBadCharacter()
    {
        JsonReader reader = new JsonTextReader(new StringReader(@"true"));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsBytes(); }, "Unexpected character encountered while parsing value: t. Path '', line 1, position 1.");
    }

    [Fact]
    public void ReadBytesWithUnexpectedEnd()
    {
        var helloWorld = "Hello world!";
        var helloWorldData = Encoding.UTF8.GetBytes(helloWorld);

        JsonReader reader = new JsonTextReader(new StringReader(@"'" + Convert.ToBase64String(helloWorldData)));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsBytes(); }, "Unterminated string. Expected delimiter: '. Path '', line 1, position 17.");
    }

    [Fact]
    public void ReadAsDateTime_BadData()
    {
        var json = @"pie";

        var reader = new JsonTextReader(new StringReader(json));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDateTime(); }, "Unexpected character encountered while parsing value: p. Path '', line 1, position 1.");
    }

    [Fact]
    public void ReadAsDateTime_Boolean()
    {
        var json = @"true";

        var reader = new JsonTextReader(new StringReader(json));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDateTime(); }, "Unexpected character encountered while parsing value: t. Path '', line 1, position 1.");
    }

    [Fact]
    public void ReadAsDateTimeOffsetBadContent()
    {
        var reader = new JsonTextReader(new StringReader(@"new Date()"));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDateTimeOffset(); }, "Unexpected character encountered while parsing value: e. Path '', line 1, position 2.");
    }

    [Fact]
    public void ReadAsDecimalBadContent()
    {
        var reader = new JsonTextReader(new StringReader(@"new Date()"));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDecimal(); }, "Unexpected character encountered while parsing value: e. Path '', line 1, position 2.");
    }

    [Fact]
    public void ReadAsDecimalBadContent_SecondLine()
    {
        var reader = new JsonTextReader(new StringReader(@"
new Date()"));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDecimal(); }, "Unexpected character encountered while parsing value: e. Path '', line 2, position 2.");
    }

    [Fact]
    public void ReadInt32WithBadCharacter()
    {
        JsonReader reader = new JsonTextReader(new StringReader(@"true"));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsInt32(); }, "Unexpected character encountered while parsing value: t. Path '', line 1, position 1.");
    }

    [Fact]
    public void ReadNumberValue_CommaErrors()
    {
        var reader = new JsonTextReader(new StringReader("[,1]"));
        reader.Read();

        ExceptionAssert.Throws<JsonReaderException>(() =>
        {
            reader.ReadAsInt32();
        }, "Unexpected character encountered while parsing value: ,. Path '[0]', line 1, position 2.");

        Xunit.Assert.Equal(1, reader.ReadAsInt32());
        Xunit.Assert.True(reader.Read());
    }

    [Fact]
    public void ReadNumberValue_InvalidEndArray()
    {
        var reader = new JsonTextReader(new StringReader("]"));

        ExceptionAssert.Throws<JsonReaderException>(() =>
        {
            reader.ReadAsInt32();
        }, "Unexpected character encountered while parsing value: ]. Path '', line 1, position 1.");
    }

    [Fact]
    public void ReadNumberValue_CommaErrors_Multiple()
    {
        var reader = new JsonTextReader(new StringReader("[1,,1]"));
        reader.Read();
        reader.ReadAsInt32();

        ExceptionAssert.Throws<JsonReaderException>(() =>
        {
            reader.ReadAsInt32();
        }, "Unexpected character encountered while parsing value: ,. Path '[1]', line 1, position 4.");

        Xunit.Assert.Equal(1, reader.ReadAsInt32());
        Xunit.Assert.True(reader.Read());
    }

    [Fact]
    public void ReadAsString_UnexpectedEnd()
    {
        var json = @"tru";

        var reader = new JsonTextReader(new StringReader(json));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsString(); }, "Unexpected end when reading JSON. Path '', line 1, position 3.");
    }

    [Fact]
    public void ReadAsString_Null_UnexpectedEnd()
    {
        var json = @"nul";

        var reader = new JsonTextReader(new StringReader(json));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsString(); }, "Unexpected end when reading JSON. Path '', line 1, position 3.");
    }

    [Fact]
    public void ReadStringValue_InvalidEndArray()
    {
        var reader = new JsonTextReader(new StringReader("]"));

        ExceptionAssert.Throws<JsonReaderException>(() =>
        {
            reader.ReadAsDateTime();
        }, "Unexpected character encountered while parsing value: ]. Path '', line 1, position 1.");
    }

    [Fact]
    public void ReadStringValue_CommaErrors()
    {
        var reader = new JsonTextReader(new StringReader("[,'']"));
        reader.Read();

        ExceptionAssert.Throws<JsonReaderException>(() =>
        {
            reader.ReadAsString();
        }, "Unexpected character encountered while parsing value: ,. Path '[0]', line 1, position 2.");

        Xunit.Assert.Equal(string.Empty, reader.ReadAsString());
        Xunit.Assert.True(reader.Read());
    }

    [Fact]
    public void ReadStringValue_CommaErrors_Multiple()
    {
        var reader = new JsonTextReader(new StringReader("['',,'']"));
        reader.Read();
        reader.ReadAsInt32();

        ExceptionAssert.Throws<JsonReaderException>(() =>
        {
            reader.ReadAsString();
        }, "Unexpected character encountered while parsing value: ,. Path '[1]', line 1, position 5.");

        Xunit.Assert.Equal(string.Empty, reader.ReadAsString());
        Xunit.Assert.True(reader.Read());
    }

    [Fact]
    public void ReadStringValue_Numbers_NotString()
    {
        var reader = new JsonTextReader(new StringReader("[56,56]"));
        reader.Read();

        ExceptionAssert.Throws<JsonReaderException>(() =>
        {
            reader.ReadAsDateTime();
        }, "Unexpected character encountered while parsing value: 5. Path '', line 1, position 2.");

        ExceptionAssert.Throws<JsonReaderException>(() =>
        {
            reader.ReadAsDateTime();
        }, "Unexpected character encountered while parsing value: 6. Path '', line 1, position 3.");

        ExceptionAssert.Throws<JsonReaderException>(() =>
        {
            reader.ReadAsDateTime();
        }, "Unexpected character encountered while parsing value: ,. Path '[0]', line 1, position 4.");

        Xunit.Assert.Equal(56, reader.ReadAsInt32());
        Xunit.Assert.True(reader.Read());
    }

    [Fact]
    public void ErrorReadingComment()
    {
        var json = @"/";

        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected end while parsing comment. Path '', line 1, position 1.");
    }

    [Fact]
    public void EscapedPathInExceptionMessage()
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

        ExceptionAssert.Throws<JsonReaderException>(
            () =>
            {
                var reader = new JsonTextReader(new StringReader(json));
                while (reader.Read())
                {
                }
            },
            "Unexpected character encountered while parsing value: !. Path 'frameworks.NET5_0_OR_GREATER.dependencies['System.Xml.ReaderWriter'].source', line 6, position 20.");
    }

    [Fact]
    public void MaxDepth()
    {
        var json = "[[]]";

        var reader = new JsonTextReader(new StringReader(json))
        {
            MaxDepth = 1
        };

        Xunit.Assert.True(reader.Read());

        ExceptionAssert.Throws<JsonReaderException>(() => { Xunit.Assert.True(reader.Read()); }, "The reader's MaxDepth of 1 has been exceeded. Path '[0]', line 1, position 2.");
    }

    [Fact]
    public void MaxDepthDoesNotRecursivelyError()
    {
        var json = "[[[[]]],[[]]]";

        var reader = new JsonTextReader(new StringReader(json))
        {
            MaxDepth = 1
        };

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(0, reader.Depth);

        ExceptionAssert.Throws<JsonReaderException>(() => { Xunit.Assert.True(reader.Read()); }, "The reader's MaxDepth of 1 has been exceeded. Path '[0]', line 1, position 2.");
        Xunit.Assert.Equal(1, reader.Depth);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(2, reader.Depth);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(3, reader.Depth);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(3, reader.Depth);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(2, reader.Depth);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(1, reader.Depth);

        ExceptionAssert.Throws<JsonReaderException>(() => { Xunit.Assert.True(reader.Read()); }, "The reader's MaxDepth of 1 has been exceeded. Path '[1]', line 1, position 9.");
        Xunit.Assert.Equal(1, reader.Depth);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(2, reader.Depth);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(2, reader.Depth);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(1, reader.Depth);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(0, reader.Depth);

        Xunit.Assert.False(reader.Read());
    }

    [Fact]
    public void UnexpectedEndWhenParsingUnquotedProperty()
    {
        JsonReader reader = new JsonTextReader(new StringReader(@"{aww"));
        Xunit.Assert.True(reader.Read());

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.Read(); }, "Unexpected end while parsing unquoted property name. Path '', line 1, position 4.");
    }
}