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

public class ReadAsyncTests : TestFixtureBase
{
    [Fact]
    public async Task Read_EmptyStream_ReturnsFalse()
    {
        var ms = new MemoryStream();
        var sr = new StreamReader(ms);

        var reader = new JsonTextReader(sr);
        Xunit.Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadAsInt32Async_IntegerTooLarge_ThrowsJsonReaderException()
    {
        var token = new JValue(long.MaxValue);

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(
            () => token.CreateReader().ReadAsInt32Async(),
            "Could not convert to integer: 9223372036854775807. Path ''."
        );
    }

    [Fact]
    public async Task ReadAsDecimalAsync_IntegerTooLarge_ThrowsJsonReaderException()
    {
        var token = new JValue(double.MaxValue);

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(
            () => token.CreateReader().ReadAsDecimalAsync(),
            "Could not convert to decimal: 1.79769313486232E+308. Path ''.",
            "Could not convert to decimal: 1.7976931348623157E+308. Path ''."
        );
    }

    [Fact]
    public async Task ReadAsInt32Async_BigIntegerValue_Success()
    {
        var token = new JValue(BigInteger.Parse("1"));

        var i = await token.CreateReader().ReadAsInt32Async();
        Xunit.Assert.Equal(1, i);
    }

    [Fact]
    public async Task ReadMissingInt64()
    {
        var json = "{ A: \"\", B: 1, C: , D: 1.23, E: 3.45, F: null }";

        var reader = new JsonTextReader(new StringReader(json));

        await reader.ReadAsync();
        await reader.ReadAsync();
        await reader.ReadAsync();
        await reader.ReadAsync();
        await reader.ReadAsync();
        await reader.ReadAsync();
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("C", reader.Value);

        await reader.ReadAsync();
        Xunit.Assert.Equal(JsonToken.Undefined, reader.TokenType);
        Xunit.Assert.Equal(null, reader.Value);
    }

    [Fact]
    public async Task ReadAsInt32AsyncWithUndefined()
    {
        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
            {
                var reader = new JsonTextReader(new StringReader("undefined"));
                await reader.ReadAsInt32Async();
            },
            "Unexpected character encountered while parsing value: u. Path '', line 1, position 1.");
    }

    [Fact]
    public async Task ReadAsBooleanAsync()
    {
        var json = @"[
  1,
  0,
  1.1,
  0.0,
  0.000000000001,
  9999999999,
  -9999999999,
  9999999999999999999999999999999999999999999999999999999999999999999999,
  -9999999999999999999999999999999999999999999999999999999999999999999999,
  'true',
  'TRUE',
  'false',
  'FALSE',
  // comment!
  /* comment! */
  '',
  null
]";

        var reader = new JsonTextReader(new StringReader(json));
#if DEBUG
        reader.CharBuffer = new char[10];
#endif

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal("", reader.Path);

        XUnitAssert.True(await reader.ReadAsBooleanAsync());
        Xunit.Assert.Equal("[0]", reader.Path);

        XUnitAssert.False(await reader.ReadAsBooleanAsync());
        Xunit.Assert.Equal("[1]", reader.Path);

        XUnitAssert.True(await reader.ReadAsBooleanAsync());
        Xunit.Assert.Equal("[2]", reader.Path);

        XUnitAssert.False(await reader.ReadAsBooleanAsync());
        Xunit.Assert.Equal("[3]", reader.Path);

        XUnitAssert.True(await reader.ReadAsBooleanAsync());
        Xunit.Assert.Equal("[4]", reader.Path);

        XUnitAssert.True(await reader.ReadAsBooleanAsync());
        Xunit.Assert.Equal("[5]", reader.Path);

        XUnitAssert.True(await reader.ReadAsBooleanAsync());
        Xunit.Assert.Equal("[6]", reader.Path);

        XUnitAssert.True(await reader.ReadAsBooleanAsync());
        Xunit.Assert.Equal("[7]", reader.Path);

        XUnitAssert.True(await reader.ReadAsBooleanAsync());
        Xunit.Assert.Equal("[8]", reader.Path);

        XUnitAssert.True(await reader.ReadAsBooleanAsync());
        Xunit.Assert.Equal("[9]", reader.Path);

        XUnitAssert.True(await reader.ReadAsBooleanAsync());
        Xunit.Assert.Equal("[10]", reader.Path);

        XUnitAssert.False(await reader.ReadAsBooleanAsync());
        Xunit.Assert.Equal("[11]", reader.Path);

        XUnitAssert.False(await reader.ReadAsBooleanAsync());
        Xunit.Assert.Equal("[12]", reader.Path);

        Xunit.Assert.Equal(null, await reader.ReadAsBooleanAsync());
        Xunit.Assert.Equal("[13]", reader.Path);

        Xunit.Assert.Equal(null, await reader.ReadAsBooleanAsync());
        Xunit.Assert.Equal("[14]", reader.Path);

        Xunit.Assert.Equal(null, await reader.ReadAsBooleanAsync());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);
        Xunit.Assert.Equal("", reader.Path);

        Xunit.Assert.Equal(null, await reader.ReadAsBooleanAsync());
        Xunit.Assert.Equal(JsonToken.None, reader.TokenType);
        Xunit.Assert.Equal("", reader.Path);
    }

    [Fact]
    public async Task ReadAsBoolean_NullCharAsync()
    {
        var json = '\0' + @"true" + '\0' + '\0';

        var reader = new JsonTextReader(new StringReader(json));

        XUnitAssert.True(await reader.ReadAsBooleanAsync());
        Xunit.Assert.Equal(null, await reader.ReadAsBooleanAsync());
    }

    [Fact]
    public async Task ReadAsBytesAsync()
    {
        var data = Encoding.UTF8.GetBytes("Hello world");

        var json = @"""" + Convert.ToBase64String(data) + @"""";

        var reader = new JsonTextReader(new StringReader(json));

        var result = await reader.ReadAsBytesAsync();

        Xunit.Assert.Equal(data, result);
    }

    [Fact]
    public async Task ReadAsBooleanNoContentAsync()
    {
        var reader = new JsonTextReader(new StringReader(@""));

        Xunit.Assert.Null(await reader.ReadAsBooleanAsync());
        Xunit.Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsBytesIntegerArrayWithCommentsAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"[/*hi*/1/*hi*/,2/*hi*/]"));

        var data = await reader.ReadAsBytesAsync();
        Xunit.Assert.Equal(2, data.Length);
        Xunit.Assert.Equal(1, data[0]);
        Xunit.Assert.Equal(2, data[1]);
    }

    [Fact]
    public async Task ReadUnicodeAsync()
    {
        var json = @"{""Message"":""Hi,I\u0092ve send you smth""}";

        var reader = new JsonTextReader(new StringReader(json));
#if DEBUG
        reader.CharBuffer = new char[5];
#endif

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("Message", reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal(@"Hi,I" + '\u0092' + "ve send you smth", reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Xunit.Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadHexidecimalWithAllLettersAsync()
    {
        var json = @"{""text"":0xabcdef12345}";

        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);
        Xunit.Assert.Equal(11806310474565, reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

#if DEBUG
    [Fact]
    public async Task ReadLargeObjectsAsync()
    {
        const int nrItems = 2;
        const int length = 1200;
        const int largeBufferLength = 2048;

        var apostrophe = Encoding.ASCII.GetBytes(@"""").First();
        var openingBracket = Encoding.ASCII.GetBytes(@"[").First();
        var comma = Encoding.ASCII.GetBytes(@",").First();
        var closingBracket = Encoding.ASCII.GetBytes(@"]").First();

        using (var ms = new MemoryStream())
        {
            ms.WriteByte(openingBracket);
            for (var i = 0; i < nrItems; i++)
            {
                ms.WriteByte(apostrophe);

                for (var j = 0; j <= length; j++)
                {
                    var current = Convert.ToByte(j % 10 + 48);
                    ms.WriteByte(current);
                }

                ms.WriteByte(apostrophe);
                if (i < nrItems - 1)
                {
                    ms.WriteByte(comma);
                }
            }

            ms.WriteByte(closingBracket);
            ms.Seek(0, SeekOrigin.Begin);

            var reader = new JsonTextReader(new StreamReader(ms));
            reader.LargeBufferLength = largeBufferLength;

            Xunit.Assert.True(await reader.ReadAsync());
            Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

            Xunit.Assert.True(await reader.ReadAsync());
            Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
            Xunit.Assert.Equal(largeBufferLength, reader.CharBuffer.Length);

            Xunit.Assert.True(await reader.ReadAsync());
            Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
            // buffer has been shifted before reading the second string
            Xunit.Assert.Equal(largeBufferLength, reader.CharBuffer.Length);

            Xunit.Assert.True(await reader.ReadAsync());
            Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);

            Xunit.Assert.False(await reader.ReadAsync());
        }
    }
#endif

    [Fact]
    public async Task ReadSingleBytesAsync()
    {
        var s = new StringReader(@"""SGVsbG8gd29ybGQu""");
        var reader = new JsonTextReader(s);

        var data = await reader.ReadAsBytesAsync();
        Xunit.Assert.NotNull(data);

        var text = Encoding.UTF8.GetString(data, 0, data.Length);
        Xunit.Assert.Equal("Hello world.", text);
    }

    [Fact]
    public async Task ReadOctalNumberAsync()
    {
        var s = new StringReader(@"[0372, 0xFA, 0XFA]");
        var jsonReader = new JsonTextReader(s);

        Xunit.Assert.True(await jsonReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartArray, jsonReader.TokenType);

        Xunit.Assert.True(await jsonReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
        Xunit.Assert.Equal(250L, jsonReader.Value);

        Xunit.Assert.True(await jsonReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
        Xunit.Assert.Equal(250L, jsonReader.Value);

        Xunit.Assert.True(await jsonReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
        Xunit.Assert.Equal(250L, jsonReader.Value);

        Xunit.Assert.True(await jsonReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndArray, jsonReader.TokenType);

        Xunit.Assert.False(await jsonReader.ReadAsync());
    }

    [Fact]
    public async Task ReadOctalNumberAsInt64Async()
    {
        var s = new StringReader(@"[0372, 0xFA, 0XFA]");
        var jsonReader = new JsonTextReader(s);

        Xunit.Assert.True(await jsonReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartArray, jsonReader.TokenType);

        await jsonReader.ReadAsync();
        Xunit.Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
        Xunit.Assert.Equal(typeof(long), jsonReader.ValueType);
        Xunit.Assert.Equal(250L, (long)jsonReader.Value);

        await jsonReader.ReadAsync();
        Xunit.Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
        Xunit.Assert.Equal(typeof(long), jsonReader.ValueType);
        Xunit.Assert.Equal(250L, (long)jsonReader.Value);

        await jsonReader.ReadAsync();
        Xunit.Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
        Xunit.Assert.Equal(typeof(long), jsonReader.ValueType);
        Xunit.Assert.Equal(250L, (long)jsonReader.Value);

        Xunit.Assert.True(await jsonReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndArray, jsonReader.TokenType);

        Xunit.Assert.False(await jsonReader.ReadAsync());
    }

    [Fact]
    public async Task ReadOctalNumberAsInt32Async()
    {
        var s = new StringReader(@"[0372, 0xFA, 0XFA]");
        var jsonReader = new JsonTextReader(s);

        Xunit.Assert.True(await jsonReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartArray, jsonReader.TokenType);

        await jsonReader.ReadAsInt32Async();
        Xunit.Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
        Xunit.Assert.Equal(typeof(int), jsonReader.ValueType);
        Xunit.Assert.Equal(250, jsonReader.Value);

        await jsonReader.ReadAsInt32Async();
        Xunit.Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
        Xunit.Assert.Equal(typeof(int), jsonReader.ValueType);
        Xunit.Assert.Equal(250, jsonReader.Value);

        await jsonReader.ReadAsInt32Async();
        Xunit.Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
        Xunit.Assert.Equal(typeof(int), jsonReader.ValueType);
        Xunit.Assert.Equal(250, jsonReader.Value);

        Xunit.Assert.True(await jsonReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndArray, jsonReader.TokenType);

        Xunit.Assert.False(await jsonReader.ReadAsync());
    }

    [Fact]
    public async Task ReadAsDecimalNoContentAsync()
    {
        var reader = new JsonTextReader(new StringReader(@""));

        Xunit.Assert.Null(await reader.ReadAsDecimalAsync());
        Xunit.Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsBytesNoContentAsync()
    {
        var reader = new JsonTextReader(new StringReader(@""));

        Xunit.Assert.Null(await reader.ReadAsBytesAsync());
        Xunit.Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetNoContentAsync()
    {
        var reader = new JsonTextReader(new StringReader(@""));

        Xunit.Assert.Null(await reader.ReadAsDateTimeOffsetAsync());
        Xunit.Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetAsync()
    {
        var json = "{\"Offset\":\"\\/Date(946663200000+0600)\\/\"}";

        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsDateTimeOffsetAsync();
        Xunit.Assert.Equal(JsonToken.Date, reader.TokenType);
        Xunit.Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        Xunit.Assert.Equal(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(6)), reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetNegativeAsync()
    {
        var json = @"{""Offset"":""\/Date(946706400000-0600)\/""}";

        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsDateTimeOffsetAsync();
        Xunit.Assert.Equal(JsonToken.Date, reader.TokenType);
        Xunit.Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        Xunit.Assert.Equal(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(-6)), reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetBadStringAsync()
    {
        var json = @"{""Offset"":""blablahbla""}";

        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
        {
            await reader.ReadAsDateTimeOffsetAsync();
        }, "Could not convert string to DateTimeOffset: blablahbla. Path 'Offset', line 1, position 22.");
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetHoursOnlyAsync()
    {
        var json = "{\"Offset\":\"\\/Date(946663200000+06)\\/\"}";

        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsDateTimeOffsetAsync();
        Xunit.Assert.Equal(JsonToken.Date, reader.TokenType);
        Xunit.Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        Xunit.Assert.Equal(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(6)), reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetWithMinutesAsync()
    {
        var json = @"{""Offset"":""\/Date(946708260000-0631)\/""}";

        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsDateTimeOffsetAsync();
        Xunit.Assert.Equal(JsonToken.Date, reader.TokenType);
        Xunit.Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        Xunit.Assert.Equal(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(-6).Add(TimeSpan.FromMinutes(-31))), reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetIsoDateAsync()
    {
        var json = @"{""Offset"":""2011-08-01T21:25Z""}";

        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsDateTimeOffsetAsync();
        Xunit.Assert.Equal(JsonToken.Date, reader.TokenType);
        Xunit.Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        Xunit.Assert.Equal(new DateTimeOffset(new DateTime(2011, 8, 1, 21, 25, 0, DateTimeKind.Utc), TimeSpan.Zero), reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetUnitedStatesDateAsync()
    {
        var json = @"{""Offset"":""1/30/2011""}";

        var reader = new JsonTextReader(new StringReader(json));
        reader.Culture = new CultureInfo("en-US");

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsDateTimeOffsetAsync();
        Xunit.Assert.Equal(JsonToken.Date, reader.TokenType);
        Xunit.Assert.Equal(typeof(DateTimeOffset), reader.ValueType);

        var dt = (DateTimeOffset)reader.Value;
        Xunit.Assert.Equal(new DateTime(2011, 1, 30, 0, 0, 0, DateTimeKind.Unspecified), dt.DateTime);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetNewZealandDateAsync()
    {
        var json = @"{""Offset"":""30/1/2011""}";

        var reader = new JsonTextReader(new StringReader(json));
        reader.Culture = new CultureInfo("en-NZ");

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsDateTimeOffsetAsync();
        Xunit.Assert.Equal(JsonToken.Date, reader.TokenType);
        Xunit.Assert.Equal(typeof(DateTimeOffset), reader.ValueType);

        var dt = (DateTimeOffset)reader.Value;
        Xunit.Assert.Equal(new DateTime(2011, 1, 30, 0, 0, 0, DateTimeKind.Unspecified), dt.DateTime);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsDecimalIntAsync()
    {
        var json = @"{""Name"":1}";

        var reader = new JsonTextReader(new StringReader(json));

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
    public async Task ReadAsIntDecimalAsync()
    {
        var json = @"{""Name"": 1.1}";

        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
        {
            await reader.ReadAsInt32Async();
        }, "Input string '1.1' is not a valid integer. Path 'Name', line 1, position 12.");
    }

    [Fact]
    public async Task ReadAsDecimalAsync()
    {
        var json = @"{""decimal"":-7.92281625142643E+28}";

        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        var d = await reader.ReadAsDecimalAsync();
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);
        Xunit.Assert.Equal(typeof(decimal), reader.ValueType);
        Xunit.Assert.Equal(-79228162514264300000000000000m, d);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsDecimalFrenchAsync()
    {
        var json = @"{""decimal"":""9,99""}";

        var reader = new JsonTextReader(new StringReader(json));
        reader.Culture = new CultureInfo("fr-FR");

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        var d = await reader.ReadAsDecimalAsync();
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);
        Xunit.Assert.Equal(typeof(decimal), reader.ValueType);
        Xunit.Assert.Equal(9.99m, d);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public async Task ReadBufferOnControlCharAsync()
    {
        var json = @"[
  {
    ""Name"": ""Jim"",
    ""BirthDate"": ""\/Date(978048000000)\/"",
    ""LastModified"": ""\/Date(978048000000)\/""
  },
  {
    ""Name"": ""Jim"",
    ""BirthDate"": ""\/Date(978048000000)\/"",
    ""LastModified"": ""\/Date(978048000000)\/""
  }
]";

        var reader = new JsonTextReader(new StringReader(json));
#if DEBUG
        reader.CharBuffer = new char[5];
#endif

        for (var i = 0; i < 13; i++)
        {
            await reader.ReadAsync();
        }

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(new DateTime(631136448000000000), reader.Value);
    }

    [Fact]
    public async Task ReadBufferOnEndCommentAsync()
    {
        var json = @"/*comment*/ { /*comment*/
        ""Name"": /*comment*/ ""Apple"" /*comment*/, /*comment*/
        ""ExpiryDate"": ""\/Date(1230422400000)\/"",
        ""Price"": 3.99,
        ""Sizes"": /*comment*/ [ /*comment*/
          ""Small"", /*comment*/
          ""Medium"" /*comment*/,
          /*comment*/ ""Large""
        /*comment*/ ] /*comment*/
      } /*comment*/";

        var reader = new JsonTextReader(new StringReader(json));
#if DEBUG
        reader.CharBuffer = new char[5];
#endif

        for (var i = 0; i < 26; i++)
        {
            Xunit.Assert.True(await reader.ReadAsync());
        }

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadAsDouble_NullAsync()
    {
        var reader = new JsonTextReader(new StringReader("null"));
        Xunit.Assert.Equal(null, await reader.ReadAsDoubleAsync());
    }

    [Fact]
    public async Task ReadAsDouble_SuccessAsync()
    {
        var reader = new JsonTextReader(new StringReader("'12.34'"));
        Xunit.Assert.Equal(12.34d, await reader.ReadAsDoubleAsync());
    }

    [Fact]
    public async Task ReadAsDouble_HexAsync()
    {
        var reader = new JsonTextReader(new StringReader("0XCAFEBABE"));
        Xunit.Assert.Equal(3405691582d, await reader.ReadAsDoubleAsync());
    }

    [Fact]
    public async Task ReadAsDouble_AllowThousandsAsync()
    {
        var reader = new JsonTextReader(new StringReader("'1,112.34'"));
        Xunit.Assert.Equal(1112.34d, await reader.ReadAsDoubleAsync());
    }

    [Fact]
    public async Task ReadAsDouble_FailureAsync()
    {
        var reader = new JsonTextReader(new StringReader("['Trump',1]"));

        Xunit.Assert.True(await reader.ReadAsync());

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
        {
            await reader.ReadAsDoubleAsync();
        }, "Could not convert string to double: Trump. Path '[0]', line 1, position 8.");

        Xunit.Assert.Equal(1d, await reader.ReadAsDoubleAsync());
        Xunit.Assert.True(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadAsString_BooleanAsync()
    {
        var reader = new JsonTextReader(new StringReader("{\"Test1\":false}"));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.True(await reader.ReadAsync());

        var s = await reader.ReadAsStringAsync();
        Xunit.Assert.Equal("false", s);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task Read_Boolean_FailureAsync()
    {
        var reader = new JsonTextReader(new StringReader("{\"Test1\":false1}"));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.True(await reader.ReadAsync());

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
        {
            await reader.ReadAsync();
        }, "Error parsing boolean value. Path 'Test1', line 1, position 14.");

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadAsString_Boolean_FailureAsync()
    {
        var reader = new JsonTextReader(new StringReader("{\"Test1\":false1}"));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.True(await reader.ReadAsync());

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () =>
        {
            await reader.ReadAsStringAsync();
        }, "Unexpected character encountered while parsing value: 1. Path 'Test1', line 1, position 14.");

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadValue_EmptyString_PositionAsync()
    {
        var json = @"['','','','','','','']";

        var reader = new JsonTextReader(new StringReader(json));

        await reader.ReadAsync();
        await reader.ReadAsInt32Async();
        Xunit.Assert.Equal("[0]", reader.Path);
        await reader.ReadAsDecimalAsync();
        Xunit.Assert.Equal("[1]", reader.Path);
        await reader.ReadAsDateTimeAsync();
        Xunit.Assert.Equal("[2]", reader.Path);
        await reader.ReadAsDateTimeOffsetAsync();
        Xunit.Assert.Equal("[3]", reader.Path);
        await reader.ReadAsStringAsync();
        Xunit.Assert.Equal("[4]", reader.Path);
        await reader.ReadAsBytesAsync();
        Xunit.Assert.Equal("[5]", reader.Path);
        await reader.ReadAsDoubleAsync();
        Xunit.Assert.Equal("[6]", reader.Path);

        Xunit.Assert.Null(await reader.ReadAsStringAsync());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.Null(await reader.ReadAsStringAsync());
        Xunit.Assert.Equal(JsonToken.None, reader.TokenType);

        Xunit.Assert.Null(await reader.ReadAsBytesAsync());
        Xunit.Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadValueCommentsAsync()
    {
        var json = @"/*comment*/[/*comment*/1/*comment*/,2,/*comment*//*comment*/""three""/*comment*/,""four""/*comment*/,null,/*comment*/null,3.99,1.1/*comment*/,''/*comment*/]/*comment*/";

        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.Equal(1, await reader.ReadAsInt32Async());
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);

        Xunit.Assert.Equal(2, await reader.ReadAsInt32Async());
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);

        Xunit.Assert.Equal("three", await reader.ReadAsStringAsync());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);

        Xunit.Assert.Equal("four", await reader.ReadAsStringAsync());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);

        Xunit.Assert.Equal(null, await reader.ReadAsStringAsync());
        Xunit.Assert.Equal(JsonToken.Null, reader.TokenType);

        Xunit.Assert.Equal(null, await reader.ReadAsInt32Async());
        Xunit.Assert.Equal(JsonToken.Null, reader.TokenType);

        Xunit.Assert.Equal(3.99m, await reader.ReadAsDecimalAsync());
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);

        Xunit.Assert.Equal(1.1m, await reader.ReadAsDecimalAsync());
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);

        Xunit.Assert.Equal(new byte[0], await reader.ReadAsBytesAsync());
        Xunit.Assert.Equal(JsonToken.Bytes, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.Equal(null, await reader.ReadAsInt32Async());
        Xunit.Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadContentDelimitedByCommentsAsync()
    {
        var json = @"/*comment*/{/*comment*/Name:/*comment*/true/*comment*/,/*comment*/
        ""ExpiryDate"":/*comment*/new
" + StringUtils.LineFeed + @"Date
(/*comment*/null/*comment*/),
        ""Price"": 3.99,
        ""Sizes"":/*comment*/[/*comment*/
          ""Small""/*comment*/]/*comment*/}/*comment*/";

        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("Name", reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Boolean, reader.TokenType);
        XUnitAssert.True(reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("ExpiryDate", reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartConstructor, reader.TokenType);
        Xunit.Assert.Equal(5, reader.LineNumber);
        Xunit.Assert.Equal("Date", reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Null, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndConstructor, reader.TokenType);
    }

    [Fact]
    public async Task ReadNullIntLineNumberAndPositionAsync()
    {
        var json = @"[
  1,
  2,
  3,
  null
]";

        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        await reader.ReadAsync();
        Xunit.Assert.Equal(1, reader.LineNumber);

        await reader.ReadAsInt32Async();
        Xunit.Assert.Equal(2, reader.LineNumber);
        Xunit.Assert.Equal("[0]", reader.Path);

        await reader.ReadAsInt32Async();
        Xunit.Assert.Equal(3, reader.LineNumber);
        Xunit.Assert.Equal("[1]", reader.Path);

        await reader.ReadAsInt32Async();
        Xunit.Assert.Equal(4, reader.LineNumber);
        Xunit.Assert.Equal("[2]", reader.Path);

        await reader.ReadAsInt32Async();
        Xunit.Assert.Equal(5, reader.LineNumber);
        Xunit.Assert.Equal("[3]", reader.Path);

        await reader.ReadAsync();
        Xunit.Assert.Equal(6, reader.LineNumber);
        Xunit.Assert.Equal(string.Empty, reader.Path);

        Xunit.Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadingFromSlowStreamAsync()
    {
        var json = "[false, true, true, false, 'test!', 1.11, 0e-10, 0E-10, 0.25e-5, 0.3e10, 6.0221418e23, 'Purple\\r \\n monkey\\'s:\\tdishwasher']";

        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        Xunit.Assert.True(await reader.ReadAsync());

        Xunit.Assert.True(await reader.ReadAsync());
        XUnitAssert.False(reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Boolean, reader.TokenType);
        XUnitAssert.True(reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Boolean, reader.TokenType);
        XUnitAssert.True(reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Boolean, reader.TokenType);
        XUnitAssert.False(reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal("test!", reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);
        Xunit.Assert.Equal(1.11d, reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);
        Xunit.Assert.Equal(0d, reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);
        Xunit.Assert.Equal(0d, reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);
        Xunit.Assert.Equal(0.0000025d, reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);
        Xunit.Assert.Equal(3000000000d, reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);
        Xunit.Assert.Equal(602214180000000000000000d, reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal(reader.Value, "Purple\r \n monkey's:\tdishwasher");

        Xunit.Assert.True(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadCommentInsideArrayAsync()
    {
        var json = @"{
    ""projects"": [
        ""src"",
        //""
        ""test""
    ]
}";

        var jsonTextReader = new JsonTextReader(new StringReader(json));
        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, jsonTextReader.TokenType);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, jsonTextReader.TokenType);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartArray, jsonTextReader.TokenType);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.String, jsonTextReader.TokenType);
        Xunit.Assert.Equal("src", jsonTextReader.Value);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Comment, jsonTextReader.TokenType);
        Xunit.Assert.Equal(@"""", jsonTextReader.Value);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.String, jsonTextReader.TokenType);
        Xunit.Assert.Equal("test", jsonTextReader.Value);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndArray, jsonTextReader.TokenType);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndObject, jsonTextReader.TokenType);
    }

    [Fact]
    public async Task ReadAsBytes_Base64AndGuidAsync()
    {
        var jsonTextReader = new JsonTextReader(new StringReader("'AAAAAAAAAAAAAAAAAAAAAAAAAAABAAAA'"));
        var data = await jsonTextReader.ReadAsBytesAsync();
        var expected = Convert.FromBase64String("AAAAAAAAAAAAAAAAAAAAAAAAAAABAAAA");

        Xunit.Assert.Equal(expected, data);

        jsonTextReader = new JsonTextReader(new StringReader("'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAABAAAA'"));
        data = await jsonTextReader.ReadAsBytesAsync();
        expected = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAABAAAA").ToByteArray();

        Xunit.Assert.Equal(expected, data);
    }

    [Fact]
    public async Task ReadSingleQuoteInsideDoubleQuoteStringAsync()
    {
        var json = @"{""NameOfStore"":""Forest's Bakery And Cafe""}";

        var jsonTextReader = new JsonTextReader(new StringReader(json));
        await jsonTextReader.ReadAsync();
        await jsonTextReader.ReadAsync();
        await jsonTextReader.ReadAsync();

        Xunit.Assert.Equal(@"Forest's Bakery And Cafe", jsonTextReader.Value);
    }

    [Fact]
    public async Task ReadMultilineStringAsync()
    {
        var json = @"""first line
second line
third line""";

        var jsonTextReader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.String, jsonTextReader.TokenType);

        Xunit.Assert.Equal(@"first line
second line
third line", jsonTextReader.Value);
    }

    [Fact]
    public async Task ReadBigIntegerAsync()
    {
        var json = @"{
    ParentId: 1,
    ChildId: 333333333333333333333333333333333333333,
}";

        var jsonTextReader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, jsonTextReader.TokenType);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, jsonTextReader.TokenType);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Integer, jsonTextReader.TokenType);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, jsonTextReader.TokenType);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Integer, jsonTextReader.TokenType);
        Xunit.Assert.Equal(typeof(BigInteger), jsonTextReader.ValueType);
        Xunit.Assert.Equal(BigInteger.Parse("333333333333333333333333333333333333333"), jsonTextReader.Value);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndObject, jsonTextReader.TokenType);

        Xunit.Assert.False(await jsonTextReader.ReadAsync());

        var o = JObject.Parse(json);
        var i = (BigInteger)((JValue)o["ChildId"]).Value;
        Xunit.Assert.Equal(BigInteger.Parse("333333333333333333333333333333333333333"), i);
    }

    [Fact]
    public async Task ReadBadMSDateAsStringAsync()
    {
        var json = @"{
    ChildId: '\/Date(9467082_PIE_340000-0631)\/'
}";

        var jsonTextReader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, jsonTextReader.TokenType);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, jsonTextReader.TokenType);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.String, jsonTextReader.TokenType);
        Xunit.Assert.Equal(@"/Date(9467082_PIE_340000-0631)/", jsonTextReader.Value);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndObject, jsonTextReader.TokenType);

        Xunit.Assert.False(await jsonTextReader.ReadAsync());
    }

    [Fact]
    public async Task ReadConstructorAsync()
    {
        var json = @"{""DefaultConverter"":new Date(0, ""hi""),""MemberConverter"":""1970-01-01T00:00:00Z""}";

        JsonReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartConstructor, reader.TokenType);
        Xunit.Assert.Equal("Date", reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(0L, reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal("hi", reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndConstructor, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal("MemberConverter", reader.Value);
    }

    [Fact]
    public async Task ReadingIndentedAsync()
    {
        var input = @"{
  CPU: 'Intel',
  Drives: [
    'DVD read/writer',
    ""500 gigabyte hard drive""
  ]
}";

        var sr = new StringReader(input);

        using (var jsonReader = new JsonTextReader(sr))
        {
#if DEBUG
            jsonReader.CharBuffer = new char[5];
#endif

            Xunit.Assert.Equal(jsonReader.TokenType, JsonToken.None);
            Xunit.Assert.Equal(0, jsonReader.LineNumber);
            Xunit.Assert.Equal(0, jsonReader.LinePosition);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(jsonReader.TokenType, JsonToken.StartObject);
            Xunit.Assert.Equal(1, jsonReader.LineNumber);
            Xunit.Assert.Equal(1, jsonReader.LinePosition);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(jsonReader.TokenType, JsonToken.PropertyName);
            Xunit.Assert.Equal(jsonReader.Value, "CPU");
            Xunit.Assert.Equal(2, jsonReader.LineNumber);
            Xunit.Assert.Equal(6, jsonReader.LinePosition);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(JsonToken.String, jsonReader.TokenType);
            Xunit.Assert.Equal("Intel", jsonReader.Value);
            Xunit.Assert.Equal(2, jsonReader.LineNumber);
            Xunit.Assert.Equal(14, jsonReader.LinePosition);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(jsonReader.TokenType, JsonToken.PropertyName);
            Xunit.Assert.Equal(jsonReader.Value, "Drives");
            Xunit.Assert.Equal(3, jsonReader.LineNumber);
            Xunit.Assert.Equal(9, jsonReader.LinePosition);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(jsonReader.TokenType, JsonToken.StartArray);
            Xunit.Assert.Equal(3, jsonReader.LineNumber);
            Xunit.Assert.Equal(11, jsonReader.LinePosition);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(jsonReader.TokenType, JsonToken.String);
            Xunit.Assert.Equal(jsonReader.Value, "DVD read/writer");
            Xunit.Assert.Equal(jsonReader.QuoteChar, '\'');
            Xunit.Assert.Equal(4, jsonReader.LineNumber);
            Xunit.Assert.Equal(21, jsonReader.LinePosition);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(jsonReader.TokenType, JsonToken.String);
            Xunit.Assert.Equal(jsonReader.Value, "500 gigabyte hard drive");
            Xunit.Assert.Equal(jsonReader.QuoteChar, '"');
            Xunit.Assert.Equal(5, jsonReader.LineNumber);
            Xunit.Assert.Equal(29, jsonReader.LinePosition);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(jsonReader.TokenType, JsonToken.EndArray);
            Xunit.Assert.Equal(6, jsonReader.LineNumber);
            Xunit.Assert.Equal(3, jsonReader.LinePosition);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(jsonReader.TokenType, JsonToken.EndObject);
            Xunit.Assert.Equal(7, jsonReader.LineNumber);
            Xunit.Assert.Equal(1, jsonReader.LinePosition);

            Xunit.Assert.False(await jsonReader.ReadAsync());
        }
    }

    [Fact]
    public async Task ReadLongStringAsync()
    {
        var s = new string('a', 10000);
        JsonReader reader = new JsonTextReader(new StringReader("'" + s + "'"));
        await reader.ReadAsync();

        Xunit.Assert.Equal(s, reader.Value);
    }

    [Fact]
    public async Task ReadLongJsonArrayAsync()
    {
        var valueCount = 10000;
        var sw = new StringWriter();
        var writer = new JsonTextWriter(sw);
        writer.WriteStartArray();
        for (var i = 0; i < valueCount; i++)
        {
            writer.WriteValue(i);
        }

        writer.WriteEndArray();

        var json = sw.ToString();

        var reader = new JsonTextReader(new StringReader(json));
        Xunit.Assert.True(await reader.ReadAsync());
        for (var i = 0; i < valueCount; i++)
        {
            Xunit.Assert.True(await reader.ReadAsync());
            Xunit.Assert.Equal((long)i, reader.Value);
        }

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task NullCharReadingAsync()
    {
        var json = "\0{\0'\0h\0i\0'\0:\0[\01\0,\0'\0'\0\0,\0null\0]\0,\0do\0:true\0}\0\0/*\0sd\0f\0*/\0/*\0sd\0f\0*/ \0";
        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Null, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Boolean, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Comment, reader.TokenType);
        Xunit.Assert.Equal("\0sd\0f\0", reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Comment, reader.TokenType);
        Xunit.Assert.Equal("\0sd\0f\0", reader.Value);

        Xunit.Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadNullTerminatorStringsAsync()
    {
        JsonReader reader = new JsonTextReader(new StringReader("'h\0i'"));
        Xunit.Assert.True(await reader.ReadAsync());

        Xunit.Assert.Equal("h\0i", reader.Value);
    }

    [Fact]
    public async Task ReadBytesNoStartWithUnexpectedEndAsync()
    {
        JsonReader reader = new JsonTextReader(new StringReader(@"[  "));
        Xunit.Assert.True(await reader.ReadAsync());

        Xunit.Assert.Null(await reader.ReadAsBytesAsync());
        Xunit.Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadNewLinesAsync()
    {
        var newLinesText = StringUtils.CarriageReturn + StringUtils.CarriageReturnLineFeed + StringUtils.LineFeed + StringUtils.CarriageReturnLineFeed + " " + StringUtils.CarriageReturn + StringUtils.CarriageReturnLineFeed;

        var json = newLinesText + "{" + newLinesText + "'" + newLinesText + "name1" + newLinesText + "'" + newLinesText + ":" + newLinesText + "[" + newLinesText + "new" + newLinesText + "Date" + newLinesText + "(" + newLinesText + "1" + newLinesText + "," + newLinesText + "null" + newLinesText + "/*" + newLinesText + "blah comment" + newLinesText + "*/" + newLinesText + ")" + newLinesText + "," + newLinesText + "1.1111" + newLinesText + "]" + newLinesText + "," + newLinesText + "name2" + newLinesText + ":" + newLinesText + "{" + newLinesText + "}" + newLinesText + "}" + newLinesText;

        var count = 0;
        var sr = new StringReader(newLinesText);
        while (sr.ReadLine() != null)
        {
            count++;
        }

        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(7, reader.LineNumber);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(31, reader.LineNumber);
        Xunit.Assert.Equal(newLinesText + "name1" + newLinesText, reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(37, reader.LineNumber);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(55, reader.LineNumber);
        Xunit.Assert.Equal(JsonToken.StartConstructor, reader.TokenType);
        Xunit.Assert.Equal("Date", reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(61, reader.LineNumber);
        Xunit.Assert.Equal(1L, reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(73, reader.LineNumber);
        Xunit.Assert.Equal(null, reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(91, reader.LineNumber);
        Xunit.Assert.Equal(newLinesText + "blah comment" + newLinesText, reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(97, reader.LineNumber);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(109, reader.LineNumber);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(115, reader.LineNumber);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(133, reader.LineNumber);
        Xunit.Assert.Equal("name2", reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(139, reader.LineNumber);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(145, reader.LineNumber);

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(151, reader.LineNumber);
    }

    [Fact]
    public async Task ReadBytesFollowingNumberInArrayAsync()
    {
        var helloWorld = "Hello world!";
        var helloWorldData = Encoding.UTF8.GetBytes(helloWorld);

        JsonReader reader = new JsonTextReader(new StringReader(@"[1,'" + Convert.ToBase64String(helloWorldData) + @"']"));
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);
        var data = await reader.ReadAsBytesAsync();
        Xunit.Assert.Equal(helloWorldData, data);
        Xunit.Assert.Equal(JsonToken.Bytes, reader.TokenType);
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadBytesFollowingNumberInObjectAsync()
    {
        var helloWorld = "Hello world!";
        var helloWorldData = Encoding.UTF8.GetBytes(helloWorld);

        JsonReader reader = new JsonTextReader(new StringReader(@"{num:1,data:'" + Convert.ToBase64String(helloWorldData) + @"'}"));
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);
        Xunit.Assert.True(await reader.ReadAsync());
        var data = await reader.ReadAsBytesAsync();
        Xunit.Assert.Equal(helloWorldData, data);
        Xunit.Assert.Equal(JsonToken.Bytes, reader.TokenType);
        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Xunit.Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadingEscapedStringsAsync()
    {
        var input = "{value:'Purple\\r \\n monkey\\'s:\\tdishwasher'}";

        var sr = new StringReader(input);

        using (JsonReader jsonReader = new JsonTextReader(sr))
        {
            Xunit.Assert.Equal(0, jsonReader.Depth);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(JsonToken.StartObject, jsonReader.TokenType);
            Xunit.Assert.Equal(0, jsonReader.Depth);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
            Xunit.Assert.Equal(1, jsonReader.Depth);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(jsonReader.TokenType, JsonToken.String);
            Xunit.Assert.Equal("Purple\r \n monkey's:\tdishwasher", jsonReader.Value);
            Xunit.Assert.Equal('\'', jsonReader.QuoteChar);
            Xunit.Assert.Equal(1, jsonReader.Depth);

            await jsonReader.ReadAsync();
            Xunit.Assert.Equal(JsonToken.EndObject, jsonReader.TokenType);
            Xunit.Assert.Equal(0, jsonReader.Depth);
        }
    }

    [Fact]
    public async Task ReadRandomJsonAsync()
    {
        var json = @"[
  true,
  {
    ""integer"": 99,
    ""string"": ""how now brown cow?"",
    ""array"": [
      0,
      1,
      2,
      3,
      4,
      {
        ""decimal"": 990.00990099
      },
      5
    ]
  },
  ""This is a string."",
  null,
  null
]";

        var reader = new JsonTextReader(new StringReader(json));
        while (await reader.ReadAsync())
        {
        }
    }

    [Fact]
    public void AsyncMethodsAlreadyCancelled()
    {
        var source = new CancellationTokenSource();
        var token = source.Token;
        source.Cancel();

        var reader = new JsonTextReader(new StreamReader(Stream.Null));
        Xunit.Assert.True(reader.ReadAsync(token).IsCanceled);
        Xunit.Assert.True(reader.ReadAsBooleanAsync(token).IsCanceled);
        Xunit.Assert.True(reader.ReadAsBytesAsync(token).IsCanceled);
        Xunit.Assert.True(reader.ReadAsDateTimeAsync(token).IsCanceled);
        Xunit.Assert.True(reader.ReadAsDateTimeOffsetAsync(token).IsCanceled);
        Xunit.Assert.True(reader.ReadAsDecimalAsync(token).IsCanceled);
        Xunit.Assert.True(reader.ReadAsInt32Async(token).IsCanceled);
        Xunit.Assert.True(reader.ReadAsStringAsync(token).IsCanceled);
    }

    class NoOverridesDerivedJsonTextAsync : JsonTextReader
    {
        public NoOverridesDerivedJsonTextAsync()
            : base(new StreamReader(Stream.Null))
        {
        }
    }

    class MinimalOverridesDerivedJsonReader : JsonReader
    {
        public override bool Read() => true;
    }

    [Fact]
    public void AsyncMethodsAlreadyCancelledOnTextReaderSubclass()
    {
        var source = new CancellationTokenSource();
        var token = source.Token;
        source.Cancel();

        JsonTextReader reader = new NoOverridesDerivedJsonTextAsync();
        Xunit.Assert.True(reader.ReadAsync(token).IsCanceled);
        Xunit.Assert.True(reader.ReadAsBooleanAsync(token).IsCanceled);
        Xunit.Assert.True(reader.ReadAsBytesAsync(token).IsCanceled);
        Xunit.Assert.True(reader.ReadAsDateTimeAsync(token).IsCanceled);
        Xunit.Assert.True(reader.ReadAsDateTimeOffsetAsync(token).IsCanceled);
        Xunit.Assert.True(reader.ReadAsDecimalAsync(token).IsCanceled);
        Xunit.Assert.True(reader.ReadAsInt32Async(token).IsCanceled);
        Xunit.Assert.True(reader.ReadAsStringAsync(token).IsCanceled);
    }

    [Fact]
    public void AsyncMethodsAlreadyCancelledOnReaderSubclass()
    {
        var source = new CancellationTokenSource();
        var token = source.Token;
        source.Cancel();

        JsonReader reader = new MinimalOverridesDerivedJsonReader();
        Xunit.Assert.True(reader.ReadAsync(token).IsCanceled);
        Xunit.Assert.True(reader.ReadAsBooleanAsync(token).IsCanceled);
        Xunit.Assert.True(reader.ReadAsBytesAsync(token).IsCanceled);
        Xunit.Assert.True(reader.ReadAsDateTimeAsync(token).IsCanceled);
        Xunit.Assert.True(reader.ReadAsDateTimeOffsetAsync(token).IsCanceled);
        Xunit.Assert.True(reader.ReadAsDecimalAsync(token).IsCanceled);
        Xunit.Assert.True(reader.ReadAsInt32Async(token).IsCanceled);
        Xunit.Assert.True(reader.ReadAsStringAsync(token).IsCanceled);
    }

    [Fact]
    public async Task ThrowOnDuplicateKeysDeserializingAsync()
    {
        var json = @"
                {
                    ""a"": 1,
                    ""b"": [
                        {
                            ""c"": {
                                ""d"": 1,
                                ""d"": ""2""
                            }
                        }
                    ]
                }
            ";

        var settings = new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error };

        var reader = new JsonTextReader(new StringReader(json));
        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await JToken.ReadFromAsync(reader, settings));
    }

    [Fact]
    public async Task MaxDepth_GreaterThanDefaultAsync()
    {
        var json = GetNestedJson(150);

        var reader = new JsonTextReader(new StringReader(json));
        reader.MaxDepth = 150;

        while (await reader.ReadAsync())
        {
        }
    }

    [Fact]
    public async Task MaxDepth_NullAsync()
    {
        var json = GetNestedJson(150);

        var reader = new JsonTextReader(new StringReader(json));
        reader.MaxDepth = null;

        while (await reader.ReadAsync())
        {
        }
    }

    [Fact]
    public async Task MaxDepth_MaxValueAsync()
    {
        var json = GetNestedJson(150);

        var reader = new JsonTextReader(new StringReader(json));
        reader.MaxDepth = int.MaxValue;

        while (await reader.ReadAsync())
        {
        }
    }
}