// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class MiscAsyncTests : TestFixtureBase
{
    [Fact]
    public async Task ReadWithSupportMultipleContentCommaDelimitedAsync()
    {
        var json = @"{ 'name': 'Admin' },{ 'name': 'Publisher' },1,null,[],,'string'";

        var reader = new JsonTextReader(new StringReader(json));
        reader.SupportMultipleContent = true;

        await reader.VerifyReaderState();
    }

    [Fact]
    public async Task LineInfoAndNewLinesAsync()
    {
        var json = "{}";

        var jsonTextReader = new JsonTextReader(new StringReader(json));

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, jsonTextReader.TokenType);
        Assert.Equal(1, jsonTextReader.LineNumber);
        Assert.Equal(1, jsonTextReader.LinePosition);

        Assert.True(await jsonTextReader.ReadAsync());

        Assert.Equal(JsonToken.EndObject, jsonTextReader.TokenType);
        Assert.Equal(1, jsonTextReader.LineNumber);
        Assert.Equal(2, jsonTextReader.LinePosition);

        json = "\n{\"a\":\"bc\"}";

        jsonTextReader = new(new StringReader(json));

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, jsonTextReader.TokenType);
        Assert.Equal(2, jsonTextReader.LineNumber);
        Assert.Equal(1, jsonTextReader.LinePosition);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, jsonTextReader.TokenType);
        Assert.Equal(2, jsonTextReader.LineNumber);
        Assert.Equal(5, jsonTextReader.LinePosition);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.String, jsonTextReader.TokenType);
        Assert.Equal(2, jsonTextReader.LineNumber);
        Assert.Equal(9, jsonTextReader.LinePosition);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, jsonTextReader.TokenType);
        Assert.Equal(2, jsonTextReader.LineNumber);
        Assert.Equal(10, jsonTextReader.LinePosition);

        json = "\n{\"a\":\n\"bc\",\"d\":true\n}";

        jsonTextReader = new(new StringReader(json));

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.StartObject, jsonTextReader.TokenType);
        Assert.Equal(2, jsonTextReader.LineNumber);
        Assert.Equal(1, jsonTextReader.LinePosition);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, jsonTextReader.TokenType);
        Assert.Equal(2, jsonTextReader.LineNumber);
        Assert.Equal(5, jsonTextReader.LinePosition);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.String, jsonTextReader.TokenType);
        Assert.Equal(3, jsonTextReader.LineNumber);
        Assert.Equal(4, jsonTextReader.LinePosition);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, jsonTextReader.TokenType);
        Assert.Equal(3, jsonTextReader.LineNumber);
        Assert.Equal(9, jsonTextReader.LinePosition);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.Boolean, jsonTextReader.TokenType);
        Assert.Equal(3, jsonTextReader.LineNumber);
        Assert.Equal(13, jsonTextReader.LinePosition);

        Assert.True(await jsonTextReader.ReadAsync());
        Assert.Equal(JsonToken.EndObject, jsonTextReader.TokenType);
        Assert.Equal(4, jsonTextReader.LineNumber);
        Assert.Equal(1, jsonTextReader.LinePosition);
    }

    [Fact]
    public Task UnescapeDoubleQuotesAsync()
    {
        var json = @"{""recipe_id"":""12"",""recipe_name"":""Apocalypse Leather Armors"",""recipe_text"":""#C16------------------------------\r\n#C12Ingredients #C20\r\n#C16------------------------------\r\n\r\na piece of Leather Armor\r\n( ie #L \""Enhanced Leather Armor Boots\"" \""85644\"" )\r\n<img src=rdb:\/\/13264>\r\n\r\n#L \""Hacker Tool\"" \""87814\""\r\n<img src=rdb:\/\/99282>\r\n\r\n#L \""Clanalizer\"" \""208313\""\r\n<img src=rdb:\/\/156479>\r\n\r\n#C16------------------------------\r\n#C12Recipe #C16\r\n#C16------------------------------#C20\r\n\r\nHacker Tool\r\n#C15+#C20\r\na piece of Leather Armor\r\n#C15=#C20\r\n<img src=rdb:\/\/13264>\r\na piece of Hacked Leather Armor\r\n( ie : #L \""Hacked Leather Armor Boots\"" \""245979\"" )\r\n#C16Skills: |  BE  |#C20\r\n\r\n#C14------------------------------#C20\r\n\r\nClanalizer\r\n#C15+#C20\r\na piece of Hacked Leather Armor\r\n#C15=#C20\r\n<img src=rdb:\/\/13264>\r\na piece of Apocalypse Leather Armor\r\n( ie : #L \""Apocalypse Leather Armor Boots\"" \""245966\"" )\r\n#C16Skills: |  ??  |#C20\r\n\r\n#C16------------------------------\r\n#C12Details#C16\r\n#C16------------------------------#C20\r\n\r\n#L \""Apocalypse Leather Armor Boots\"" \""245967\""\r\n#L \""Apocalypse Leather Armor Gloves\"" \""245969\""\r\n#L \""Apocalypse Leather Armor Helmet\"" \""245975\""\r\n#L \""Apocalypse Leather Armor Pants\"" \""245971\""\r\n#L \""Apocalypse Leather Armor Sleeves\"" \""245973\""\r\n#L \""Apocalypse Leather Body Armor\"" \""245965\""\r\n\r\n#C16------------------------------\r\n#C12Comments#C16\r\n#C16------------------------------#C20\r\n\r\nNice froob armor.. but ugleh!\r\n\r\n"",""recipe_author"":null}";

        var reader = new JsonTextReader(new StringReader(json));

        return reader.VerifyReaderState();
    }

    [Fact]
    public async Task SurrogatePairValidAsync()
    {
        var json = @"{ ""MATHEMATICAL ITALIC CAPITAL ALPHA"": ""\uD835\uDEE2"" }";

        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.True(await reader.ReadAsync());

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.String, reader.TokenType);

        var s = reader.Value.ToString();
        Assert.Equal(2, s.Length);

        var stringInfo = new StringInfo(s);
        Assert.Equal(1, stringInfo.LengthInTextElements);
    }

    [Fact]
    public async Task SurrogatePairReplacementAsync()
    {
        // existing good surrogate pair
        Assert.Equal("ABC \ud800\udc00 DEF", await ReadStringAsync("ABC \\ud800\\udc00 DEF"));

        // invalid surrogates (two high back-to-back)
        Assert.Equal("ABC \ufffd\ufffd DEF", await ReadStringAsync("ABC \\ud800\\ud800 DEF"));

        // invalid surrogates (two high back-to-back)
        Assert.Equal("ABC \ufffd\ufffd\u1234 DEF", await ReadStringAsync("ABC \\ud800\\ud800\\u1234 DEF"));

        // invalid surrogates (three high back-to-back)
        Assert.Equal("ABC \ufffd\ufffd\ufffd DEF", await ReadStringAsync("ABC \\ud800\\ud800\\ud800 DEF"));

        // invalid surrogates (high followed by a good surrogate pair)
        Assert.Equal("ABC \ufffd\ud800\udc00 DEF", await ReadStringAsync("ABC \\ud800\\ud800\\udc00 DEF"));

        // invalid high surrogate at end of string
        Assert.Equal("ABC \ufffd", await ReadStringAsync("ABC \\ud800"));

        // high surrogate not followed by low surrogate
        Assert.Equal("ABC \ufffd DEF", await ReadStringAsync("ABC \\ud800 DEF"));

        // low surrogate not preceded by high surrogate
        Assert.Equal("ABC \ufffd\ufffd DEF", await ReadStringAsync("ABC \\udc00\\ud800 DEF"));

        // make sure unencoded invalid surrogate characters don't make it through
        Assert.Equal("\ufffd\ufffd\ufffd", await ReadStringAsync("\udc00\ud800\ud800"));

        Assert.Equal("ABC \ufffd\b", await ReadStringAsync("ABC \\ud800\\b"));
        Assert.Equal("ABC \ufffd ", await ReadStringAsync("ABC \\ud800 "));
        Assert.Equal("ABC \b\ufffd", await ReadStringAsync("ABC \\b\\ud800"));
    }

    static async Task<string> ReadStringAsync(string input)
    {
        var ms = new MemoryStream(Encoding.UTF8.GetBytes($@"""{input}"""));

        var reader = new JsonTextReader(new StreamReader(ms));
        await reader.ReadAsync();

        return (string) reader.Value;
    }

    [Fact]
    public async Task YahooFinanceAsync()
    {
        var input = @"{
""matches"" : [
{""t"":""C"", ""n"":""Citigroup Inc."", ""e"":""NYSE"", ""id"":""662713""}
,{""t"":""CHL"", ""n"":""China Mobile Ltd. (ADR)"", ""e"":""NYSE"", ""id"":""660998""}
,{""t"":""PTR"", ""n"":""PetroChina Company Limited (ADR)"", ""e"":""NYSE"", ""id"":""664536""}
,{""t"":""RIO"", ""n"":""Companhia Vale do Rio Doce (ADR)"", ""e"":""NYSE"", ""id"":""671472""}
,{""t"":""RIOPR"", ""n"":""Companhia Vale do Rio Doce (ADR)"", ""e"":""NYSE"", ""id"":""3512643""}
,{""t"":""CSCO"", ""n"":""Cisco Systems, Inc."", ""e"":""NASDAQ"", ""id"":""99624""}
,{""t"":""CVX"", ""n"":""Chevron Corporation"", ""e"":""NYSE"", ""id"":""667226""}
,{""t"":""TM"", ""n"":""Toyota Motor Corporation (ADR)"", ""e"":""NYSE"", ""id"":""655880""}
,{""t"":""JPM"", ""n"":""JPMorgan Chase \\x26 Co."", ""e"":""NYSE"", ""id"":""665639""}
,{""t"":""COP"", ""n"":""ConocoPhillips"", ""e"":""NYSE"", ""id"":""1691168""}
,{""t"":""LFC"", ""n"":""China Life Insurance Company Ltd. (ADR)"", ""e"":""NYSE"", ""id"":""688679""}
,{""t"":""NOK"", ""n"":""Nokia Corporation (ADR)"", ""e"":""NYSE"", ""id"":""657729""}
,{""t"":""KO"", ""n"":""The Coca-Cola Company"", ""e"":""NYSE"", ""id"":""6550""}
,{""t"":""VZ"", ""n"":""Verizon Communications Inc."", ""e"":""NYSE"", ""id"":""664887""}
,{""t"":""AMX"", ""n"":""America Movil S.A.B de C.V. (ADR)"", ""e"":""NYSE"", ""id"":""665834""}],
""all"" : false
}
";

        using var jsonReader = new JsonTextReader(new StringReader(input));
        while (await jsonReader.ReadAsync())
        {
        }
    }

    [Fact]
    public async Task DepthAsync()
    {
        var input = @"{
  value:'Purple',
  array:[1,2,'2014-06-04T00:00:00Z'],
  subobject:{prop:1,proparray:[1]}
}";

        var sr = new StringReader(input);

        using var reader = new JsonTextReader(sr);
        await reader.VerifyReaderState();
    }

    [Fact]
    public async Task AppendCharsWhileReadingNullAsync()
    {
        var json = @"[
  {
    '$id': '1',
    Name: 'e1',
    Manager: null
  },
  {
    '$id': '2',
    Name: 'e2',
    Manager: null
  },
  {
    '$ref': '1'
  },
  {
    '$ref': '2'
  }
]";

        var reader = new JsonTextReader(new StringReader(json));
#if !RELEASE
        reader.CharBuffer = new char[129];
#endif

        for (var i = 0; i < 15; i++)
        {
            await reader.ReadAsync();
        }

        await reader.ReadAsync();
        Assert.Equal(JsonToken.Null, reader.TokenType);
    }

    [Fact]
    public async Task AppendCharsWhileReadingNewLineAsync()
    {
        var json = @"
{
  ""description"": ""A person"",
  ""type"": ""object"",
  ""properties"":
  {
    ""name"": {""type"":""string""},
    ""hobbies"": {
      ""type"": ""array"",
      ""items"": {""type"":""string""}
    }
  }
}
";

        var reader = new JsonTextReader(new StringReader(json));
#if !RELEASE
        reader.CharBuffer = new char[129];
#endif

        for (var i = 0; i < 14; i++)
        {
            Assert.True(await reader.ReadAsync());
        }

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Assert.Equal("type", reader.Value);
    }

    [Fact]
    public async Task BufferTestAsync()
    {
        var json = $@"{{
              ""CPU"": ""Intel"",
              ""Description"": ""Amazing!\nBuy now!"",
              ""Drives"": [
                ""DVD read/writer"",
                ""500 gigabyte hard drive"",
                ""Amazing Drive{new string('!', 9000)}""
              ]
            }}";

        var arrayPool = new FakeArrayPool();

        for (var i = 0; i < 1000; i++)
        {
            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                reader.ArrayPool = arrayPool;

                while (await reader.ReadAsync())
                {
                }
            }

            if ((i + 1) % 100 == 0)
            {
                Console.WriteLine($"Allocated buffers: {arrayPool.FreeArrays.Count}");
            }
        }

        Assert.Equal(0, arrayPool.UsedArrays.Count);
        Assert.Equal(6, arrayPool.FreeArrays.Count);
    }

    [Fact]
    public async Task BufferTest_WithErrorAsync()
    {
        var json = @"{
              ""CPU"": ""Intel?\nYes"",
              ""Description"": ""Amazin";

        var arrayPool = new FakeArrayPool();

        try
        {
            // dispose will free used buffers
            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                reader.ArrayPool = arrayPool;

                while (await reader.ReadAsync())
                {
                }
            }

            XUnitAssert.Fail();
        }
        catch
        {
        }

        Assert.Equal(0, arrayPool.UsedArrays.Count);
        Assert.Equal(2, arrayPool.FreeArrays.Count);
    }

    [Fact]
    public async Task WriteReadWriteAsync()
    {
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);

        using (var jsonWriter = new JsonTextWriter(stringWriter)
               {
                   Formatting = Formatting.Indented
               })
        {
            await jsonWriter.WriteStartArrayAsync();
            await jsonWriter.WriteValueAsync(true);

            await jsonWriter.WriteStartObjectAsync();
            await jsonWriter.WritePropertyNameAsync("integer");
            await jsonWriter.WriteValueAsync(99);
            await jsonWriter.WritePropertyNameAsync("string");
            await jsonWriter.WriteValueAsync("how now brown cow?");
            await jsonWriter.WritePropertyNameAsync("array");

            await jsonWriter.WriteStartArrayAsync();
            for (var i = 0; i < 5; i++)
            {
                await jsonWriter.WriteValueAsync(i);
            }

            await jsonWriter.WriteStartObjectAsync();
            await jsonWriter.WritePropertyNameAsync("decimal");
            await jsonWriter.WriteValueAsync(990.00990099m);
            await jsonWriter.WriteEndObjectAsync();

            await jsonWriter.WriteValueAsync(5);
            await jsonWriter.WriteEndArrayAsync();

            await jsonWriter.WriteEndObjectAsync();

            await jsonWriter.WriteValueAsync("This is a string.");
            await jsonWriter.WriteNullAsync();
            await jsonWriter.WriteNullAsync();
            await jsonWriter.WriteEndArrayAsync();
        }

        var json = stringBuilder.ToString();

        var serializer = new JsonSerializer();

        var jsonObject = serializer.Deserialize(new JsonTextReader(new StringReader(json)));

        stringBuilder = new();
        stringWriter = new(stringBuilder);

        using (var jsonWriter = new JsonTextWriter(stringWriter)
               {
                   Formatting = Formatting.Indented
               })
        {
            serializer.Serialize(jsonWriter, jsonObject);
        }

        Assert.Equal(json, stringBuilder.ToString());
    }

    [Fact]
    public async Task LongStringTestAsync()
    {
        var length = 20000;
        var json = $@"[""{new string(' ', length)}""]";

        var reader = new JsonTextReader(new StringReader(json));

        await reader.ReadAsync();
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        await reader.ReadAsync();
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal(typeof(string), reader.ValueType);
        Assert.Equal(20000, reader.Value.ToString().Length);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Assert.False(await reader.ReadAsync());
        Assert.Equal(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task EscapedUnicodeTextAsync()
    {
        var json = @"[""\u003c"",""\u5f20""]";

        var reader = new JsonTextReader(new StringReader(json));
#if !RELEASE
        reader.CharBuffer = new char[2];
#endif

        await reader.ReadAsync();
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        await reader.ReadAsync();
        Assert.Equal("<", reader.Value);

        await reader.ReadAsync();
        Assert.Equal(24352, Convert.ToInt32(Convert.ToChar((string) reader.Value)));

        await reader.ReadAsync();
        Assert.Equal(JsonToken.EndArray, reader.TokenType);
    }

    [Fact]
    public Task SupportMultipleContentAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"{'prop1':[1]} 1 2 ""name"" [][]null {}{} 1.1"));
        reader.SupportMultipleContent = true;

        return reader.VerifyReaderState();
    }

    [Fact]
    public Task SingleLineCommentsAsync()
    {
        var json = $@"//comment*//*hi*/
{{//comment
Name://comment
true//comment after true{StringUtils.CarriageReturn},//comment after comma{StringUtils.CarriageReturnLineFeed}""ExpiryDate""://comment{StringUtils.LineFeed}'2014-06-04T00:00:00Z',
        ""Price"": 3.99,
        ""Sizes"": //comment
[//comment

          ""Small""//comment
]//comment
}}//comment 
//comment 1 ";

        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        return reader.VerifyReaderState();
    }

    [Fact]
    public async Task JustSinglelineCommentAsync()
    {
        var json = @"//comment";

        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(JsonToken.Comment, reader.TokenType);
        Assert.Equal("comment", reader.Value);

        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ScientificNotationAsync()
    {
        var d = Convert.ToDouble("6.0221418e23", CultureInfo.InvariantCulture);

        Assert.Equal("6,0221418E+23", d.ToString(new CultureInfo("fr-FR")));
        Assert.Equal("602214180000000000000000", d.ToString("0.#############################################################################"));

        var json = @"[0e-10,0E-10,0.25e-5,0.3e10,6.0221418e23]";

        var reader = new JsonTextReader(new StringReader(json));

        await reader.ReadAsync();

        await reader.ReadAsync();
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(0d, reader.Value);

        await reader.ReadAsync();
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(0d, reader.Value);

        await reader.ReadAsync();
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(0.0000025d, reader.Value);

        await reader.ReadAsync();
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(3000000000d, reader.Value);

        await reader.ReadAsync();
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(602214180000000000000000d, reader.Value);

        await reader.ReadAsync();

        reader = new(new StringReader(json));

        await reader.ReadAsync();

        await reader.ReadAsDecimalAsync();
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(0m, reader.Value);

        await reader.ReadAsDecimalAsync();
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(0m, reader.Value);

        await reader.ReadAsDecimalAsync();
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(0.0000025m, reader.Value);

        await reader.ReadAsDecimalAsync();
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(3000000000m, reader.Value);

        await reader.ReadAsDecimalAsync();
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(602214180000000000000000m, reader.Value);

        await reader.ReadAsync();
    }

    [Fact]
    public async Task WriteReadBoundaryDecimalsAsync()
    {
        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter);

        await jsonWriter.WriteStartArrayAsync();
        await jsonWriter.WriteValueAsync(decimal.MaxValue);
        await jsonWriter.WriteValueAsync(decimal.MinValue);
        await jsonWriter.WriteEndArrayAsync();

        var json = stringWriter.ToString();

        var sr = new StringReader(json);
        var reader = new JsonTextReader(sr);

        Assert.True(await reader.ReadAsync());

        var max = await reader.ReadAsDecimalAsync();
        Assert.Equal(decimal.MaxValue, max);

        var min = await reader.ReadAsDecimalAsync();
        Assert.Equal(decimal.MinValue, min);

        Assert.True(await reader.ReadAsync());
    }

    [Fact]
    public async Task LinePositionOnNewLineAsync()
    {
        var json1 = "{'a':'bc'}";

        var reader = new JsonTextReader(new StringReader(json1));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(1, reader.LineNumber);
        Assert.Equal(1, reader.LinePosition);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(1, reader.LineNumber);
        Assert.Equal(5, reader.LinePosition);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(1, reader.LineNumber);
        Assert.Equal(9, reader.LinePosition);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(1, reader.LineNumber);
        Assert.Equal(10, reader.LinePosition);

        Assert.False(await reader.ReadAsync());

        var json2 = "\n{'a':'bc'}";

        reader = new(new StringReader(json2));

        Assert.True(await reader.ReadAsync());
        Assert.Equal(2, reader.LineNumber);
        Assert.Equal(1, reader.LinePosition);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(2, reader.LineNumber);
        Assert.Equal(5, reader.LinePosition);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(2, reader.LineNumber);
        Assert.Equal(9, reader.LinePosition);

        Assert.True(await reader.ReadAsync());
        Assert.Equal(2, reader.LineNumber);
        Assert.Equal(10, reader.LinePosition);

        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task InvalidUnicodeSequenceAsync()
    {
        var json = @"{'prop':'\u123!'}";

        var reader = new JsonTextReader(new StringReader(json));

        Assert.True(await reader.ReadAsync());
        Assert.True(await reader.ReadAsync());

        await XUnitAssert.ThrowsAsync<JsonReaderException>(
            () => reader.ReadAsync(),
            @"Invalid Unicode escape sequence: \u123!. Path 'prop', line 1, position 11.");
    }
}