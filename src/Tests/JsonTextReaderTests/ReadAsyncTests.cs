// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class ReadAsyncTests : TestFixtureBase
{
    [Fact]
    public async Task Read_EmptyStream_ReturnsFalse()
    {
        var ms = new MemoryStream();
        var sr = new StreamReader(ms);

        var reader = new JsonTextReader(sr);
        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadAsInt32Async_IntegerTooLarge_ThrowsJsonReaderException()
    {
        var token = new JValue(long.MaxValue);

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => token.CreateReader().ReadAsInt32Async(),
            "Could not convert to integer: 9223372036854775807. Path ''."
        );
    }

    [Fact]
    public async Task ReadAsDecimalAsync_IntegerTooLarge_ThrowsJsonReaderException()
    {
        var token = new JValue(double.MaxValue);

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
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
        Assert.Equal(1, i);
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
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("C", reader.Value);

        await reader.ReadAsync();
        Assert.Equal(JsonToken.Undefined, reader.TokenType);
        Assert.Equal(null, reader.Value);
    }

    [Fact]
    public Task ReadAsInt32AsyncWithUndefined()
    {
        var reader = new JsonTextReader(new StringReader("undefined"));
        return XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsInt32Async(),
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
#if !RELEASE
        reader.CharBuffer = new char[10];
#endif

        Assert.True(await reader.ReadAsync());
        Assert.Equal("", reader.Path);

        XUnitAssert.True(await reader.ReadAsBooleanAsync());
        Assert.Equal("[0]", reader.Path);

        XUnitAssert.False(await reader.ReadAsBooleanAsync());
        Assert.Equal("[1]", reader.Path);

        XUnitAssert.True(await reader.ReadAsBooleanAsync());
        Assert.Equal("[2]", reader.Path);

        XUnitAssert.False(await reader.ReadAsBooleanAsync());
        Assert.Equal("[3]", reader.Path);

        XUnitAssert.True(await reader.ReadAsBooleanAsync());
        Assert.Equal("[4]", reader.Path);

        XUnitAssert.True(await reader.ReadAsBooleanAsync());
        Assert.Equal("[5]", reader.Path);

        XUnitAssert.True(await reader.ReadAsBooleanAsync());
        Assert.Equal("[6]", reader.Path);

        XUnitAssert.True(await reader.ReadAsBooleanAsync());
        Assert.Equal("[7]", reader.Path);

        XUnitAssert.True(await reader.ReadAsBooleanAsync());
        Assert.Equal("[8]", reader.Path);

        XUnitAssert.True(await reader.ReadAsBooleanAsync());
        Assert.Equal("[9]", reader.Path);

        XUnitAssert.True(await reader.ReadAsBooleanAsync());
        Assert.Equal("[10]", reader.Path);

        XUnitAssert.False(await reader.ReadAsBooleanAsync());
        Assert.Equal("[11]", reader.Path);

        XUnitAssert.False(await reader.ReadAsBooleanAsync());
        Assert.Equal("[12]", reader.Path);

        Assert.Equal(null, await reader.ReadAsBooleanAsync());
        Assert.Equal("[13]", reader.Path);

        Assert.Equal(null, await reader.ReadAsBooleanAsync());
        Assert.Equal("[14]", reader.Path);

        Assert.Equal(null, await reader.ReadAsBooleanAsync());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);
        Assert.Equal("", reader.Path);

        Assert.Equal(null, await reader.ReadAsBooleanAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
        Assert.Equal("", reader.Path);
    }

    [Fact]
    public async Task ReadAsBoolean_NullCharAsync()
    {
        var json = "\0true\0\0";

        var reader = new JsonTextReader(new StringReader(json));

        XUnitAssert.True(await reader.ReadAsBooleanAsync());
        Assert.Equal(null, await reader.ReadAsBooleanAsync());
    }

    [Fact]
    public async Task ReadAsBytesAsync()
    {
        var data = Encoding.UTF8.GetBytes("Hello world");

        var json = $@"""{Convert.ToBase64String(data)}""";

        var reader = new JsonTextReader(new StringReader(json));

        var result = await reader.ReadAsBytesAsync();

        Assert.Equal(data, result);
    }

    [Fact]
    public async Task ReadAsBooleanNoContentAsync()
    {
        var reader = new JsonTextReader(new StringReader(@""));

        Assert.Null(await reader.ReadAsBooleanAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsBytesIntegerArrayWithCommentsAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"[/*hi*/1/*hi*/,2/*hi*/]"));

        var data = await reader.ReadAsBytesAsync();
        Assert.Equal(2, data.Length);
        Assert.Equal(1, data[0]);
        Assert.Equal(2, data[1]);
    }

    [Fact]
    public async Task ReadUnicodeAsync()
    {
        var json = @"{""Message"":""Hi,I\u0092ve send you smth""}";

        var reader = new JsonTextReader(new StringReader(json));
#if !RELEASE
        reader.CharBuffer = new char[5];
#endif

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("Message", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal(@"Hi,Ive send you smth", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadHexidecimalWithAllLettersAsync()
    {
        var json = @"{""text"":0xabcdef12345}";

        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Integer, reader.TokenType);
        Assert.Equal(11806310474565, reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

#if !RELEASE
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

        using var ms = new MemoryStream();
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

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal(largeBufferLength, reader.CharBuffer.Length);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        // buffer has been shifted before reading the second string
        Assert.Equal(largeBufferLength, reader.CharBuffer.Length);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Assert.False(await reader.ReadAsync());
    }
#endif

    [Fact]
    public async Task ReadSingleBytesAsync()
    {
        var s = new StringReader(@"""SGVsbG8gd29ybGQu""");
        var reader = new JsonTextReader(s);

        var data = await reader.ReadAsBytesAsync();
        Assert.NotNull(data);

        var text = Encoding.UTF8.GetString(data, 0, data.Length);
        Assert.Equal("Hello world.", text);
    }

    [Fact]
    public async Task ReadOctalNumberAsync()
    {
        var s = new StringReader(@"[0372, 0xFA, 0XFA]");
        var jsonReader = new JsonTextReader(s);

        Assert.True(await jsonReader.ReadAsync());
        Assert.Equal(JsonToken.StartArray, jsonReader.TokenType);

        Assert.True(await jsonReader.ReadAsync());
        Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
        Assert.Equal(250L, jsonReader.Value);

        Assert.True(await jsonReader.ReadAsync());
        Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
        Assert.Equal(250L, jsonReader.Value);

        Assert.True(await jsonReader.ReadAsync());
        Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
        Assert.Equal(250L, jsonReader.Value);

        Assert.True(await jsonReader.ReadAsync());
        Assert.Equal(JsonToken.EndArray, jsonReader.TokenType);

        Assert.False(await jsonReader.ReadAsync());
    }

    [Fact]
    public async Task ReadOctalNumberAsInt64Async()
    {
        var s = new StringReader(@"[0372, 0xFA, 0XFA]");
        var jsonReader = new JsonTextReader(s);

        Assert.True(await jsonReader.ReadAsync());
        Assert.Equal(JsonToken.StartArray, jsonReader.TokenType);

        await jsonReader.ReadAsync();
        Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
        Assert.Equal(typeof(long), jsonReader.ValueType);
        Assert.Equal(250L, (long) jsonReader.Value);

        await jsonReader.ReadAsync();
        Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
        Assert.Equal(typeof(long), jsonReader.ValueType);
        Assert.Equal(250L, (long) jsonReader.Value);

        await jsonReader.ReadAsync();
        Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
        Assert.Equal(typeof(long), jsonReader.ValueType);
        Assert.Equal(250L, (long) jsonReader.Value);

        Assert.True(await jsonReader.ReadAsync());
        Assert.Equal(JsonToken.EndArray, jsonReader.TokenType);

        Assert.False(await jsonReader.ReadAsync());
    }

    [Fact]
    public async Task ReadOctalNumberAsInt32Async()
    {
        var s = new StringReader(@"[0372, 0xFA, 0XFA]");
        var jsonReader = new JsonTextReader(s);

        Assert.True(await jsonReader.ReadAsync());
        Assert.Equal(JsonToken.StartArray, jsonReader.TokenType);

        await jsonReader.ReadAsInt32Async();
        Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
        Assert.Equal(typeof(int), jsonReader.ValueType);
        Assert.Equal(250, jsonReader.Value);

        await jsonReader.ReadAsInt32Async();
        Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
        Assert.Equal(typeof(int), jsonReader.ValueType);
        Assert.Equal(250, jsonReader.Value);

        await jsonReader.ReadAsInt32Async();
        Assert.Equal(JsonToken.Integer, jsonReader.TokenType);
        Assert.Equal(typeof(int), jsonReader.ValueType);
        Assert.Equal(250, jsonReader.Value);

        Assert.True(await jsonReader.ReadAsync());
        Assert.Equal(JsonToken.EndArray, jsonReader.TokenType);

        Assert.False(await jsonReader.ReadAsync());
    }

    [Fact]
    public async Task ReadAsDecimalNoContentAsync()
    {
        var reader = new JsonTextReader(new StringReader(@""));

        Assert.Null(await reader.ReadAsDecimalAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsBytesNoContentAsync()
    {
        var reader = new JsonTextReader(new StringReader(@""));

        Assert.Null(await reader.ReadAsBytesAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetNoContentAsync()
    {
        var reader = new JsonTextReader(new StringReader(@""));

        Assert.Null(await reader.ReadAsDateTimeOffsetAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetAsync()
    {
        var json = "{Offset:'2000-01-01T00:00:00.000+06'}";

        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsDateTimeOffsetAsync();
        Assert.Equal(JsonToken.Date, reader.TokenType);
        Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        Assert.Equal(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(6)), reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetNegativeAsync()
    {
        var json = @"{Offset:'2000-01-01T00:00:00.000-06'}";

        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsDateTimeOffsetAsync();
        Assert.Equal(JsonToken.Date, reader.TokenType);
        Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        Assert.Equal(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(-6)), reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetBadStringAsync()
    {
        var json = @"{""Offset"":""blablahbla""}";

        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await XUnitAssert.ThrowsAsync<JsonReaderException>(() => reader.ReadAsDateTimeOffsetAsync(),
            "Could not convert string to DateTimeOffset: blablahbla. Path 'Offset', line 1, position 22.");
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetHoursOnlyAsync()
    {
        var json = "{Offset:'2000-01-01T00:00:00.000+06'}";

        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsDateTimeOffsetAsync();
        Assert.Equal(JsonToken.Date, reader.TokenType);
        Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        Assert.Equal(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(6)), reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetWithMinutesAsync()
    {
        var json = "{Offset:'2000-01-01T00:00:00.000-0631'}";

        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsDateTimeOffsetAsync();
        Assert.Equal(JsonToken.Date, reader.TokenType);
        Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        Assert.Equal(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(-6).Add(TimeSpan.FromMinutes(-31))), reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetIsoDateAsync()
    {
        var json = @"{""Offset"":""2011-08-01T21:25Z""}";

        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsDateTimeOffsetAsync();
        Assert.Equal(JsonToken.Date, reader.TokenType);
        Assert.Equal(typeof(DateTimeOffset), reader.ValueType);
        Assert.Equal(new DateTimeOffset(new DateTime(2011, 8, 1, 21, 25, 0, DateTimeKind.Utc), TimeSpan.Zero), reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetUnitedStatesDateAsync()
    {
        var json = @"{""Offset"":""1/30/2011""}";

        var reader = new JsonTextReader(new StringReader(json));
        reader.Culture = new("en-US");

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsDateTimeOffsetAsync();
        Assert.Equal(JsonToken.Date, reader.TokenType);
        Assert.Equal(typeof(DateTimeOffset), reader.ValueType);

        var dt = (DateTimeOffset) reader.Value;
        Assert.Equal(new(2011, 1, 30, 0, 0, 0, DateTimeKind.Unspecified), dt.DateTime);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsDateTimeOffsetNewZealandDateAsync()
    {
        var json = @"{""Offset"":""30/1/2011""}";

        var reader = new JsonTextReader(new StringReader(json));
        reader.Culture = new("en-NZ");

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await reader.ReadAsDateTimeOffsetAsync();
        Assert.Equal(JsonToken.Date, reader.TokenType);
        Assert.Equal(typeof(DateTimeOffset), reader.ValueType);

        var dt = (DateTimeOffset) reader.Value;
        Assert.Equal(new(2011, 1, 30, 0, 0, 0, DateTimeKind.Unspecified), dt.DateTime);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsDecimalIntAsync()
    {
        var json = @"{""Name"":1}";

        var reader = new JsonTextReader(new StringReader(json));

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
    public async Task ReadAsIntDecimalAsync()
    {
        var json = @"{""Name"": 1.1}";

        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsInt32Async(),
            "Input string '1.1' is not a valid integer. Path 'Name', line 1, position 12.");
    }

    [Fact]
    public async Task ReadAsDecimalAsync()
    {
        var json = @"{""decimal"":-7.92281625142643E+28}";

        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        var d = await reader.ReadAsDecimalAsync();
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(typeof(decimal), reader.ValueType);
        Assert.Equal(-79228162514264300000000000000m, d);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public async Task ReadAsDecimalFrenchAsync()
    {
        var json = @"{""decimal"":""9,99""}";

        var reader = new JsonTextReader(new StringReader(json));
        reader.Culture = new("fr-FR");

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        var d = await reader.ReadAsDecimalAsync();
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(typeof(decimal), reader.ValueType);
        Assert.Equal(9.99m, d);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public async Task ReadBufferOnControlCharAsync()
    {
        var json = @"[
  {
    Name: 'Jim',
    BirthDate: '2000-01-01T00:00:00.000Z',
    LastModified: '2000-01-01T00:00:00.000Z'
  },
  {
    Name: 'Jim',
    BirthDate: '2000-01-01T00:00:00.000Z',
    LastModified: '2000-01-01T00:00:00.000Z'
  }
]";

        var reader = new JsonTextReader(new StringReader(json));
#if !RELEASE
        reader.CharBuffer = new char[5];
#endif

        for (var i = 0; i < 13; i++)
        {
            await reader.ReadAsync();
        }

        Assert.True(await reader.ReadAsync());
        Assert.Equal(new(2000, 01, 01, 0, 0, 0, DateTimeKind.Utc), (DateTime) reader.Value);
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
#if !RELEASE
        reader.CharBuffer = new char[5];
#endif

        for (var i = 0; i < 26; i++)
        {
            Assert.True(await reader.ReadAsync());
        }

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Comment, reader.TokenType);

        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadAsDouble_NullAsync()
    {
        var reader = new JsonTextReader(new StringReader("null"));
        Assert.Equal(null, await reader.ReadAsDoubleAsync());
    }

    [Fact]
    public async Task ReadAsDouble_SuccessAsync()
    {
        var reader = new JsonTextReader(new StringReader("'12.34'"));
        Assert.Equal(12.34d, await reader.ReadAsDoubleAsync());
    }

    [Fact]
    public async Task ReadAsDouble_HexAsync()
    {
        var reader = new JsonTextReader(new StringReader("0XCAFEBABE"));
        Assert.Equal(3405691582d, await reader.ReadAsDoubleAsync());
    }

    [Fact]
    public async Task ReadAsDouble_AllowThousandsAsync()
    {
        var reader = new JsonTextReader(new StringReader("'1,112.34'"));
        Assert.Equal(1112.34d, await reader.ReadAsDoubleAsync());
    }

    [Fact]
    public async Task ReadAsDouble_FailureAsync()
    {
        var reader = new JsonTextReader(new StringReader("['Trump',1]"));

        Assert.True(await reader.ReadAsync());

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsDoubleAsync(),
            "Could not convert string to double: Trump. Path '[0]', line 1, position 8.");

        Assert.Equal(1d, await reader.ReadAsDoubleAsync());
        Assert.True(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadAsString_BooleanAsync()
    {
        var reader = new JsonTextReader(new StringReader("{\"Test1\":false}"));

        Assert.True(await reader.ReadAsync());
        Assert.True(await reader.ReadAsync());

        var s = await reader.ReadAsStringAsync();
        Assert.Equal("false", s);

        Assert.True(await reader.ReadAsync());
        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task Read_Boolean_FailureAsync()
    {
        var reader = new JsonTextReader(new StringReader("{\"Test1\":false1}"));

        Assert.True(await reader.ReadAsync());
        Assert.True(await reader.ReadAsync());

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsync(),
            "Error parsing boolean value. Path 'Test1', line 1, position 14.");

        Assert.True(await reader.ReadAsync());
        Assert.True(await reader.ReadAsync());
        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadAsString_Boolean_FailureAsync()
    {
        var reader = new JsonTextReader(new StringReader("{\"Test1\":false1}"));

        Assert.True(await reader.ReadAsync());
        Assert.True(await reader.ReadAsync());

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsStringAsync(),
            "Unexpected character encountered while parsing value: 1. Path 'Test1', line 1, position 14.");

        Assert.True(await reader.ReadAsync());
        Assert.True(await reader.ReadAsync());
        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadValue_EmptyString_PositionAsync()
    {
        var json = @"['','','','','','','']";

        var reader = new JsonTextReader(new StringReader(json));

        await reader.ReadAsync();
        await reader.ReadAsInt32Async();
        Assert.Equal("[0]", reader.Path);
        await reader.ReadAsDecimalAsync();
        Assert.Equal("[1]", reader.Path);
        await reader.ReadAsDateTimeAsync();
        Assert.Equal("[2]", reader.Path);
        await reader.ReadAsDateTimeOffsetAsync();
        Assert.Equal("[3]", reader.Path);
        await reader.ReadAsStringAsync();
        Assert.Equal("[4]", reader.Path);
        await reader.ReadAsBytesAsync();
        Assert.Equal("[5]", reader.Path);
        await reader.ReadAsDoubleAsync();
        Assert.Equal("[6]", reader.Path);

        Assert.Null(await reader.ReadAsStringAsync());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Assert.Null(await reader.ReadAsStringAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);

        Assert.Null(await reader.ReadAsBytesAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadValueCommentsAsync()
    {
        var json = @"/*comment*/[/*comment*/1/*comment*/,2,/*comment*//*comment*/""three""/*comment*/,""four""/*comment*/,null,/*comment*/null,3.99,1.1/*comment*/,''/*comment*/]/*comment*/";

        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Comment, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Assert.Equal(1, await reader.ReadAsInt32Async());
        Assert.Equal(JsonToken.Integer, reader.TokenType);

        Assert.Equal(2, await reader.ReadAsInt32Async());
        Assert.Equal(JsonToken.Integer, reader.TokenType);

        Assert.Equal("three", await reader.ReadAsStringAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);

        Assert.Equal("four", await reader.ReadAsStringAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);

        Assert.Equal(null, await reader.ReadAsStringAsync());
        Assert.Equal(JsonToken.Null, reader.TokenType);

        Assert.Equal(null, await reader.ReadAsInt32Async());
        Assert.Equal(JsonToken.Null, reader.TokenType);

        Assert.Equal(3.99m, await reader.ReadAsDecimalAsync());
        Assert.Equal(JsonToken.Float, reader.TokenType);

        Assert.Equal(1.1m, await reader.ReadAsDecimalAsync());
        Assert.Equal(JsonToken.Float, reader.TokenType);

        Assert.Equal(new byte[0], await reader.ReadAsBytesAsync());
        Assert.Equal(JsonToken.Bytes, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Comment, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Assert.Equal(null, await reader.ReadAsInt32Async());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadContentDelimitedByCommentsAsync()
    {
        var json = @"/*comment*/{/*comment*/Name:/*comment*/true/*comment*/,/*comment*/
        ExpiryDate:'2014-06-04T00:00:00Z',
        Price: 3.99,
        Sizes:/*comment*/[/*comment*/
          ""Small""/*comment*/]/*comment*/}/*comment*/";

        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        await reader.VerifyReaderState();
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
        Assert.Equal(1, reader.LineNumber);

        await reader.ReadAsInt32Async();
        Assert.Equal(2, reader.LineNumber);
        Assert.Equal("[0]", reader.Path);

        await reader.ReadAsInt32Async();
        Assert.Equal(3, reader.LineNumber);
        Assert.Equal("[1]", reader.Path);

        await reader.ReadAsInt32Async();
        Assert.Equal(4, reader.LineNumber);
        Assert.Equal("[2]", reader.Path);

        await reader.ReadAsInt32Async();
        Assert.Equal(5, reader.LineNumber);
        Assert.Equal("[3]", reader.Path);

        await reader.ReadAsync();
        Assert.Equal(6, reader.LineNumber);
        Assert.Equal(string.Empty, reader.Path);

        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadingFromSlowStreamAsync()
    {
        var json = "[false, true, true, false, 'test!', 1.11, 0e-10, 0E-10, 0.25e-5, 0.3e10, 6.0221418e23, 'Purple\\r \\n monkey\\'s:\\tdishwasher']";

        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        Assert.True(await reader.ReadAsync());

        Assert.True(await reader.ReadAsync());
        XUnitAssert.False(reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Boolean, reader.TokenType);
        XUnitAssert.True(reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Boolean, reader.TokenType);
        XUnitAssert.True(reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Boolean, reader.TokenType);
        XUnitAssert.False(reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("test!", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(1.11d, reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(0d, reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(0d, reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(0.0000025d, reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(3000000000d, reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(602214180000000000000000d, reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal(reader.Value, "Purple\r \n monkey's:\tdishwasher");

        Assert.True(await reader.ReadAsync());
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
        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, jsonTextReader.TokenType);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, jsonTextReader.TokenType);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.StartArray, jsonTextReader.TokenType);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.String, jsonTextReader.TokenType);
        Assert.Equal("src", jsonTextReader.Value);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.Comment, jsonTextReader.TokenType);
        Assert.Equal(@"""", jsonTextReader.Value);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.String, jsonTextReader.TokenType);
        Assert.Equal("test", jsonTextReader.Value);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.EndArray, jsonTextReader.TokenType);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, jsonTextReader.TokenType);
    }

    [Fact]
    public async Task ReadAsBytes_Base64AndGuidAsync()
    {
        var jsonTextReader = new JsonTextReader(new StringReader("'AAAAAAAAAAAAAAAAAAAAAAAAAAABAAAA'"));
        var data = await jsonTextReader.ReadAsBytesAsync();
        var expected = Convert.FromBase64String("AAAAAAAAAAAAAAAAAAAAAAAAAAABAAAA");

        Assert.Equal(expected, data);

        jsonTextReader = new(new StringReader("'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAABAAAA'"));
        data = await jsonTextReader.ReadAsBytesAsync();
        expected = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAABAAAA").ToByteArray();

        Assert.Equal(expected, data);
    }

    [Fact]
    public async Task ReadSingleQuoteInsideDoubleQuoteStringAsync()
    {
        var json = @"{""NameOfStore"":""Forest's Bakery And Cafe""}";

        var jsonTextReader = new JsonTextReader(new StringReader(json));
        await jsonTextReader.ReadAsync();
        await jsonTextReader.ReadAsync();
        await jsonTextReader.ReadAsync();

        Assert.Equal(@"Forest's Bakery And Cafe", jsonTextReader.Value);
    }

    [Fact]
    public async Task ReadMultilineStringAsync()
    {
        var json = @"""first line
second line
third line""";

        var jsonTextReader = new JsonTextReader(new StringReader(json));

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.String, jsonTextReader.TokenType);

        Assert.Equal(@"first line
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

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, jsonTextReader.TokenType);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, jsonTextReader.TokenType);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.Integer, jsonTextReader.TokenType);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, jsonTextReader.TokenType);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.Integer, jsonTextReader.TokenType);
        Assert.Equal(typeof(BigInteger), jsonTextReader.ValueType);
        Assert.Equal(BigInteger.Parse("333333333333333333333333333333333333333"), jsonTextReader.Value);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, jsonTextReader.TokenType);

        Assert.False(await jsonTextReader.ReadAsync());

        var o = JObject.Parse(json);
        var i = (BigInteger) ((JValue) o["ChildId"]).Value;
        Assert.Equal(BigInteger.Parse("333333333333333333333333333333333333333"), i);
    }

    [Fact]
    public async Task ReadBadMSDateAsStringAsync()
    {
        var json = @"{
    ChildId: '\/Date(9467082_PIE_340000-0631)\/'
}";

        var jsonTextReader = new JsonTextReader(new StringReader(json));

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, jsonTextReader.TokenType);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, jsonTextReader.TokenType);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.String, jsonTextReader.TokenType);
        Assert.Equal(@"/Date(9467082_PIE_340000-0631)/", jsonTextReader.Value);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, jsonTextReader.TokenType);

        Assert.False(await jsonTextReader.ReadAsync());
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

        using var jsonReader = new JsonTextReader(sr);
#if !RELEASE
        jsonReader.CharBuffer = new char[5];
#endif

        Assert.Equal(jsonReader.TokenType, JsonToken.None);
        Assert.Equal(0, jsonReader.LineNumber);
        Assert.Equal(0, jsonReader.LinePosition);

        await jsonReader.ReadAsync();
        Assert.Equal(jsonReader.TokenType, JsonToken.StartObject);
        Assert.Equal(1, jsonReader.LineNumber);
        Assert.Equal(1, jsonReader.LinePosition);

        await jsonReader.ReadAsync();
        Assert.Equal(jsonReader.TokenType, JsonToken.PropertyName);
        Assert.Equal(jsonReader.Value, "CPU");
        Assert.Equal(2, jsonReader.LineNumber);
        Assert.Equal(6, jsonReader.LinePosition);

        await jsonReader.ReadAsync();
        Assert.Equal(JsonToken.String, jsonReader.TokenType);
        Assert.Equal("Intel", jsonReader.Value);
        Assert.Equal(2, jsonReader.LineNumber);
        Assert.Equal(14, jsonReader.LinePosition);

        await jsonReader.ReadAsync();
        Assert.Equal(jsonReader.TokenType, JsonToken.PropertyName);
        Assert.Equal(jsonReader.Value, "Drives");
        Assert.Equal(3, jsonReader.LineNumber);
        Assert.Equal(9, jsonReader.LinePosition);

        await jsonReader.ReadAsync();
        Assert.Equal(jsonReader.TokenType, JsonToken.StartArray);
        Assert.Equal(3, jsonReader.LineNumber);
        Assert.Equal(11, jsonReader.LinePosition);

        await jsonReader.ReadAsync();
        Assert.Equal(jsonReader.TokenType, JsonToken.String);
        Assert.Equal(jsonReader.Value, "DVD read/writer");
        Assert.Equal(jsonReader.QuoteChar, '\'');
        Assert.Equal(4, jsonReader.LineNumber);
        Assert.Equal(21, jsonReader.LinePosition);

        await jsonReader.ReadAsync();
        Assert.Equal(jsonReader.TokenType, JsonToken.String);
        Assert.Equal(jsonReader.Value, "500 gigabyte hard drive");
        Assert.Equal(jsonReader.QuoteChar, '"');
        Assert.Equal(5, jsonReader.LineNumber);
        Assert.Equal(29, jsonReader.LinePosition);

        await jsonReader.ReadAsync();
        Assert.Equal(jsonReader.TokenType, JsonToken.EndArray);
        Assert.Equal(6, jsonReader.LineNumber);
        Assert.Equal(3, jsonReader.LinePosition);

        await jsonReader.ReadAsync();
        Assert.Equal(jsonReader.TokenType, JsonToken.EndObject);
        Assert.Equal(7, jsonReader.LineNumber);
        Assert.Equal(1, jsonReader.LinePosition);

        Assert.False(await jsonReader.ReadAsync());
    }

    [Fact]
    public async Task ReadLongStringAsync()
    {
        var s = new string('a', 10000);
        JsonReader reader = new JsonTextReader(new StringReader($"'{s}'"));
        await reader.ReadAsync();

        Assert.Equal(s, reader.Value);
    }

    [Fact]
    public async Task ReadLongJsonArrayAsync()
    {
        var valueCount = 10000;
        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter);
        jsonWriter.WriteStartArray();
        for (var i = 0; i < valueCount; i++)
        {
            jsonWriter.WriteValue(i);
        }

        jsonWriter.WriteEndArray();

        var json = stringWriter.ToString();

        var reader = new JsonTextReader(new StringReader(json));
        Assert.True(await reader.ReadAsync());
        for (var i = 0; i < valueCount; i++)
        {
            Assert.True(await reader.ReadAsync());
            Assert.Equal((long) i, reader.Value);
        }

        Assert.True(await reader.ReadAsync());
        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task NullCharReadingAsync()
    {
        var json = "\0{\0'\0h\0i\0'\0:\0[\01\0,\0'\0'\0\0,\0null\0]\0,\0do\0:true\0}\0\0/*\0sd\0f\0*/\0/*\0sd\0f\0*/ \0";
        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Integer, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Null, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Boolean, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Comment, reader.TokenType);
        Assert.Equal("\0sd\0f\0", reader.Value);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Comment, reader.TokenType);
        Assert.Equal("\0sd\0f\0", reader.Value);

        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadNullTerminatorStringsAsync()
    {
        JsonReader reader = new JsonTextReader(new StringReader("'h\0i'"));
        Assert.True(await reader.ReadAsync());

        Assert.Equal("h\0i", reader.Value);
    }

    [Fact]
    public async Task ReadBytesNoStartWithUnexpectedEndAsync()
    {
        JsonReader reader = new JsonTextReader(new StringReader(@"[  "));
        Assert.True(await reader.ReadAsync());

        Assert.Null(await reader.ReadAsBytesAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task ReadNewLinesAsync()
    {
        var newLinesText = $"{StringUtils.CarriageReturn}{StringUtils.CarriageReturnLineFeed}{StringUtils.LineFeed}{StringUtils.CarriageReturnLineFeed} {StringUtils.CarriageReturn}{StringUtils.CarriageReturnLineFeed}";

        var json = $"{newLinesText}{{{newLinesText}'{newLinesText}name1{newLinesText}'{newLinesText}:{newLinesText}[{newLinesText}'2014-06-04T00:00:00Z'{newLinesText},{newLinesText}1.1111{newLinesText}]{newLinesText},{newLinesText}name2{newLinesText}:{newLinesText}{{{newLinesText}}}{newLinesText}}}{newLinesText}";

        var count = 0;
        var sr = new StringReader(newLinesText);
        while (sr.ReadLine() != null)
        {
            count++;
        }

        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));
        await reader.VerifyReaderState();
    }

    [Fact]
    public async Task ReadBytesFollowingNumberInArrayAsync()
    {
        var helloWorld = "Hello world!";
        var helloWorldData = Encoding.UTF8.GetBytes(helloWorld);

        JsonReader reader = new JsonTextReader(new StringReader($@"[1,'{Convert.ToBase64String(helloWorldData)}']"));
        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartArray, reader.TokenType);
        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Integer, reader.TokenType);
        var data = await reader.ReadAsBytesAsync();
        Assert.Equal(helloWorldData, data);
        Assert.Equal(JsonToken.Bytes, reader.TokenType);
        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadBytesFollowingNumberInObjectAsync()
    {
        var helloWorld = "Hello world!";
        var helloWorldData = Encoding.UTF8.GetBytes(helloWorld);

        JsonReader reader = new JsonTextReader(new StringReader($@"{{num:1,data:'{Convert.ToBase64String(helloWorldData)}'}}"));
        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, reader.TokenType);
        Assert.True(await reader.ReadAsync());
        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Integer, reader.TokenType);
        Assert.True(await reader.ReadAsync());
        var data = await reader.ReadAsBytesAsync();
        Assert.Equal(helloWorldData, data);
        Assert.Equal(JsonToken.Bytes, reader.TokenType);
        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ReadingEscapedStringsAsync()
    {
        var input = "{value:'Purple\\r \\n monkey\\'s:\\tdishwasher'}";

        var sr = new StringReader(input);

        using var jsonReader = new JsonTextReader(sr);
        Assert.Equal(0, jsonReader.Depth);

        await jsonReader.ReadAsync();
        Assert.Equal(JsonToken.StartObject, jsonReader.TokenType);
        Assert.Equal(0, jsonReader.Depth);

        await jsonReader.ReadAsync();
        Assert.Equal(JsonToken.PropertyName, jsonReader.TokenType);
        Assert.Equal(1, jsonReader.Depth);

        await jsonReader.ReadAsync();
        Assert.Equal(jsonReader.TokenType, JsonToken.String);
        Assert.Equal("Purple\r \n monkey's:\tdishwasher", jsonReader.Value);
        Assert.Equal('\'', jsonReader.QuoteChar);
        Assert.Equal(1, jsonReader.Depth);

        await jsonReader.ReadAsync();
        Assert.Equal(JsonToken.EndObject, jsonReader.TokenType);
        Assert.Equal(0, jsonReader.Depth);
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
        Assert.True(reader.ReadAsync(token).IsCanceled);
        Assert.True(reader.ReadAsBooleanAsync(token).IsCanceled);
        Assert.True(reader.ReadAsBytesAsync(token).IsCanceled);
        Assert.True(reader.ReadAsDateTimeAsync(token).IsCanceled);
        Assert.True(reader.ReadAsDateTimeOffsetAsync(token).IsCanceled);
        Assert.True(reader.ReadAsDecimalAsync(token).IsCanceled);
        Assert.True(reader.ReadAsInt32Async(token).IsCanceled);
        Assert.True(reader.ReadAsStringAsync(token).IsCanceled);
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
        public override bool Read()
        {
            return true;
        }
    }

    [Fact]
    public void AsyncMethodsAlreadyCancelledOnTextReaderSubclass()
    {
        var source = new CancellationTokenSource();
        var token = source.Token;
        source.Cancel();

        JsonTextReader reader = new NoOverridesDerivedJsonTextAsync();
        Assert.True(reader.ReadAsync(token).IsCanceled);
        Assert.True(reader.ReadAsBooleanAsync(token).IsCanceled);
        Assert.True(reader.ReadAsBytesAsync(token).IsCanceled);
        Assert.True(reader.ReadAsDateTimeAsync(token).IsCanceled);
        Assert.True(reader.ReadAsDateTimeOffsetAsync(token).IsCanceled);
        Assert.True(reader.ReadAsDecimalAsync(token).IsCanceled);
        Assert.True(reader.ReadAsInt32Async(token).IsCanceled);
        Assert.True(reader.ReadAsStringAsync(token).IsCanceled);
    }

    [Fact]
    public void AsyncMethodsAlreadyCancelledOnReaderSubclass()
    {
        var source = new CancellationTokenSource();
        var token = source.Token;
        source.Cancel();

        JsonReader reader = new MinimalOverridesDerivedJsonReader();
        Assert.True(reader.ReadAsync(token).IsCanceled);
        Assert.True(reader.ReadAsBooleanAsync(token).IsCanceled);
        Assert.True(reader.ReadAsBytesAsync(token).IsCanceled);
        Assert.True(reader.ReadAsDateTimeAsync(token).IsCanceled);
        Assert.True(reader.ReadAsDateTimeOffsetAsync(token).IsCanceled);
        Assert.True(reader.ReadAsDecimalAsync(token).IsCanceled);
        Assert.True(reader.ReadAsInt32Async(token).IsCanceled);
        Assert.True(reader.ReadAsStringAsync(token).IsCanceled);
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

        var settings = new JsonLoadSettings();

        var reader = new JsonTextReader(new StringReader(json));
        await XUnitAssert.ThrowsAsync<JsonReaderException>(() => JToken.ReadFromAsync(reader, settings));
    }

    [Fact]
    public async Task MaxDepth_GreaterThanDefaultAsync()
    {
        var json = NestedJson.Build(150);

        var reader = new JsonTextReader(new StringReader(json));
        reader.MaxDepth = 150;

        while (await reader.ReadAsync())
        {
        }
    }

    [Fact]
    public async Task MaxDepth_NullAsync()
    {
        var json = NestedJson.Build(150);

        var reader = new JsonTextReader(new StringReader(json));
        reader.MaxDepth = null;

        while (await reader.ReadAsync())
        {
        }
    }

    [Fact]
    public async Task MaxDepth_MaxValueAsync()
    {
        var json = NestedJson.Build(150);

        var reader = new JsonTextReader(new StringReader(json));
        reader.MaxDepth = int.MaxValue;

        while (await reader.ReadAsync())
        {
        }
    }
}