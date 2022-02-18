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

public class MiscAsyncTests : TestFixtureBase
{
    [Fact]
    public async Task ReadWithSupportMultipleContentCommaDelimitedAsync()
    {
        var json = @"{ 'name': 'Admin' },{ 'name': 'Publisher' },1,null,[],,'string'";

        var reader = new JsonTextReader(new StringReader(json));
        reader.SupportMultipleContent = true;

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.String, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.String, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Integer, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Null, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Undefined, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.String, reader.TokenType);

        Xunit.Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task LineInfoAndNewLinesAsync()
    {
        var json = "{}";

        var jsonTextReader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Assert.AreEqual(JsonToken.StartObject, jsonTextReader.TokenType);
        Assert.AreEqual(1, jsonTextReader.LineNumber);
        Assert.AreEqual(1, jsonTextReader.LinePosition);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());

        Assert.AreEqual(JsonToken.EndObject, jsonTextReader.TokenType);
        Assert.AreEqual(1, jsonTextReader.LineNumber);
        Assert.AreEqual(2, jsonTextReader.LinePosition);

        json = "\n{\"a\":\"bc\"}";

        jsonTextReader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Assert.AreEqual(JsonToken.StartObject, jsonTextReader.TokenType);
        Assert.AreEqual(2, jsonTextReader.LineNumber);
        Assert.AreEqual(1, jsonTextReader.LinePosition);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Assert.AreEqual(JsonToken.PropertyName, jsonTextReader.TokenType);
        Assert.AreEqual(2, jsonTextReader.LineNumber);
        Assert.AreEqual(5, jsonTextReader.LinePosition);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Assert.AreEqual(JsonToken.String, jsonTextReader.TokenType);
        Assert.AreEqual(2, jsonTextReader.LineNumber);
        Assert.AreEqual(9, jsonTextReader.LinePosition);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Assert.AreEqual(JsonToken.EndObject, jsonTextReader.TokenType);
        Assert.AreEqual(2, jsonTextReader.LineNumber);
        Assert.AreEqual(10, jsonTextReader.LinePosition);

        json = "\n{\"a\":\n\"bc\",\"d\":true\n}";

        jsonTextReader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Assert.AreEqual(JsonToken.StartObject, jsonTextReader.TokenType);
        Assert.AreEqual(2, jsonTextReader.LineNumber);
        Assert.AreEqual(1, jsonTextReader.LinePosition);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Assert.AreEqual(JsonToken.PropertyName, jsonTextReader.TokenType);
        Assert.AreEqual(2, jsonTextReader.LineNumber);
        Assert.AreEqual(5, jsonTextReader.LinePosition);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Assert.AreEqual(JsonToken.String, jsonTextReader.TokenType);
        Assert.AreEqual(3, jsonTextReader.LineNumber);
        Assert.AreEqual(4, jsonTextReader.LinePosition);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Assert.AreEqual(JsonToken.PropertyName, jsonTextReader.TokenType);
        Assert.AreEqual(3, jsonTextReader.LineNumber);
        Assert.AreEqual(9, jsonTextReader.LinePosition);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Assert.AreEqual(JsonToken.Boolean, jsonTextReader.TokenType);
        Assert.AreEqual(3, jsonTextReader.LineNumber);
        Assert.AreEqual(13, jsonTextReader.LinePosition);

        Xunit.Assert.True(await jsonTextReader.ReadAsync());
        Assert.AreEqual(JsonToken.EndObject, jsonTextReader.TokenType);
        Assert.AreEqual(4, jsonTextReader.LineNumber);
        Assert.AreEqual(1, jsonTextReader.LinePosition);
    }

    [Fact]
    public async Task UnescapeDoubleQuotesAsync()
    {
        var json = @"{""recipe_id"":""12"",""recipe_name"":""Apocalypse Leather Armors"",""recipe_text"":""#C16------------------------------\r\n#C12Ingredients #C20\r\n#C16------------------------------\r\n\r\na piece of Leather Armor\r\n( ie #L \""Enhanced Leather Armor Boots\"" \""85644\"" )\r\n<img src=rdb:\/\/13264>\r\n\r\n#L \""Hacker Tool\"" \""87814\""\r\n<img src=rdb:\/\/99282>\r\n\r\n#L \""Clanalizer\"" \""208313\""\r\n<img src=rdb:\/\/156479>\r\n\r\n#C16------------------------------\r\n#C12Recipe #C16\r\n#C16------------------------------#C20\r\n\r\nHacker Tool\r\n#C15+#C20\r\na piece of Leather Armor\r\n#C15=#C20\r\n<img src=rdb:\/\/13264>\r\na piece of Hacked Leather Armor\r\n( ie : #L \""Hacked Leather Armor Boots\"" \""245979\"" )\r\n#C16Skills: |  BE  |#C20\r\n\r\n#C14------------------------------#C20\r\n\r\nClanalizer\r\n#C15+#C20\r\na piece of Hacked Leather Armor\r\n#C15=#C20\r\n<img src=rdb:\/\/13264>\r\na piece of Apocalypse Leather Armor\r\n( ie : #L \""Apocalypse Leather Armor Boots\"" \""245966\"" )\r\n#C16Skills: |  ??  |#C20\r\n\r\n#C16------------------------------\r\n#C12Details#C16\r\n#C16------------------------------#C20\r\n\r\n#L \""Apocalypse Leather Armor Boots\"" \""245967\""\r\n#L \""Apocalypse Leather Armor Gloves\"" \""245969\""\r\n#L \""Apocalypse Leather Armor Helmet\"" \""245975\""\r\n#L \""Apocalypse Leather Armor Pants\"" \""245971\""\r\n#L \""Apocalypse Leather Armor Sleeves\"" \""245973\""\r\n#L \""Apocalypse Leather Body Armor\"" \""245965\""\r\n\r\n#C16------------------------------\r\n#C12Comments#C16\r\n#C16------------------------------#C20\r\n\r\nNice froob armor.. but ugleh!\r\n\r\n"",""recipe_author"":null}";

        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.String, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.String, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
        Assert.AreEqual("recipe_text", reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.String, reader.TokenType);

        Assert.AreEqual("#C16------------------------------\r\n#C12Ingredients #C20\r\n#C16------------------------------\r\n\r\na piece of Leather Armor\r\n( ie #L \"Enhanced Leather Armor Boots\" \"85644\" )\r\n<img src=rdb://13264>\r\n\r\n#L \"Hacker Tool\" \"87814\"\r\n<img src=rdb://99282>\r\n\r\n#L \"Clanalizer\" \"208313\"\r\n<img src=rdb://156479>\r\n\r\n#C16------------------------------\r\n#C12Recipe #C16\r\n#C16------------------------------#C20\r\n\r\nHacker Tool\r\n#C15+#C20\r\na piece of Leather Armor\r\n#C15=#C20\r\n<img src=rdb://13264>\r\na piece of Hacked Leather Armor\r\n( ie : #L \"Hacked Leather Armor Boots\" \"245979\" )\r\n#C16Skills: |  BE  |#C20\r\n\r\n#C14------------------------------#C20\r\n\r\nClanalizer\r\n#C15+#C20\r\na piece of Hacked Leather Armor\r\n#C15=#C20\r\n<img src=rdb://13264>\r\na piece of Apocalypse Leather Armor\r\n( ie : #L \"Apocalypse Leather Armor Boots\" \"245966\" )\r\n#C16Skills: |  ??  |#C20\r\n\r\n#C16------------------------------\r\n#C12Details#C16\r\n#C16------------------------------#C20\r\n\r\n#L \"Apocalypse Leather Armor Boots\" \"245967\"\r\n#L \"Apocalypse Leather Armor Gloves\" \"245969\"\r\n#L \"Apocalypse Leather Armor Helmet\" \"245975\"\r\n#L \"Apocalypse Leather Armor Pants\" \"245971\"\r\n#L \"Apocalypse Leather Armor Sleeves\" \"245973\"\r\n#L \"Apocalypse Leather Body Armor\" \"245965\"\r\n\r\n#C16------------------------------\r\n#C12Comments#C16\r\n#C16------------------------------#C20\r\n\r\nNice froob armor.. but ugleh!\r\n\r\n", reader.Value);
    }

    [Fact]
    public async Task SurrogatePairValidAsync()
    {
        var json = @"{ ""MATHEMATICAL ITALIC CAPITAL ALPHA"": ""\uD835\uDEE2"" }";

        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(await reader.ReadAsync());
        Xunit.Assert.True(await reader.ReadAsync());

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.String, reader.TokenType);

        var s = reader.Value.ToString();
        Assert.AreEqual(2, s.Length);

        var stringInfo = new StringInfo(s);
        Assert.AreEqual(1, stringInfo.LengthInTextElements);
    }

    [Fact]
    public async Task SurrogatePairReplacementAsync()
    {
        // existing good surrogate pair
        Assert.AreEqual("ABC \ud800\udc00 DEF", await ReadStringAsync("ABC \\ud800\\udc00 DEF"));

        // invalid surrogates (two high back-to-back)
        Assert.AreEqual("ABC \ufffd\ufffd DEF", await ReadStringAsync("ABC \\ud800\\ud800 DEF"));

        // invalid surrogates (two high back-to-back)
        Assert.AreEqual("ABC \ufffd\ufffd\u1234 DEF", await ReadStringAsync("ABC \\ud800\\ud800\\u1234 DEF"));

        // invalid surrogates (three high back-to-back)
        Assert.AreEqual("ABC \ufffd\ufffd\ufffd DEF", await ReadStringAsync("ABC \\ud800\\ud800\\ud800 DEF"));

        // invalid surrogates (high followed by a good surrogate pair)
        Assert.AreEqual("ABC \ufffd\ud800\udc00 DEF", await ReadStringAsync("ABC \\ud800\\ud800\\udc00 DEF"));

        // invalid high surrogate at end of string
        Assert.AreEqual("ABC \ufffd", await ReadStringAsync("ABC \\ud800"));

        // high surrogate not followed by low surrogate
        Assert.AreEqual("ABC \ufffd DEF", await ReadStringAsync("ABC \\ud800 DEF"));

        // low surrogate not preceded by high surrogate
        Assert.AreEqual("ABC \ufffd\ufffd DEF", await ReadStringAsync("ABC \\udc00\\ud800 DEF"));

        // make sure unencoded invalid surrogate characters don't make it through
        Assert.AreEqual("\ufffd\ufffd\ufffd", await ReadStringAsync("\udc00\ud800\ud800"));

        Assert.AreEqual("ABC \ufffd\b", await ReadStringAsync("ABC \\ud800\\b"));
        Assert.AreEqual("ABC \ufffd ", await ReadStringAsync("ABC \\ud800 "));
        Assert.AreEqual("ABC \b\ufffd", await ReadStringAsync("ABC \\b\\ud800"));
    }

    async Task<string> ReadStringAsync(string input)
    {
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(@"""" + input + @""""));

        var reader = new JsonTextReader(new StreamReader(ms));
        await reader.ReadAsync();

        return (string)reader.Value;
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

        using (JsonReader jsonReader = new JsonTextReader(new StringReader(input)))
        {
            while (await jsonReader.ReadAsync())
            {
            }
        }
    }

    [Fact]
    public async Task DepthAsync()
    {
        var input = @"{
  value:'Purple',
  array:[1,2,new Date(1)],
  subobject:{prop:1,proparray:[1]}
}";

        var sr = new StringReader(input);

        using (JsonReader reader = new JsonTextReader(sr))
        {
            Assert.AreEqual(0, reader.Depth);

            await reader.ReadAsync();
            Assert.AreEqual(reader.TokenType, JsonToken.StartObject);
            Assert.AreEqual(0, reader.Depth);
            Assert.AreEqual("", reader.Path);

            await reader.ReadAsync();
            Assert.AreEqual(reader.TokenType, JsonToken.PropertyName);
            Assert.AreEqual(1, reader.Depth);
            Assert.AreEqual("value", reader.Path);

            await reader.ReadAsync();
            Assert.AreEqual(reader.TokenType, JsonToken.String);
            Assert.AreEqual(reader.Value, @"Purple");
            Assert.AreEqual(reader.QuoteChar, '\'');
            Assert.AreEqual(1, reader.Depth);
            Assert.AreEqual("value", reader.Path);

            await reader.ReadAsync();
            Assert.AreEqual(reader.TokenType, JsonToken.PropertyName);
            Assert.AreEqual(1, reader.Depth);
            Assert.AreEqual("array", reader.Path);

            await reader.ReadAsync();
            Assert.AreEqual(reader.TokenType, JsonToken.StartArray);
            Assert.AreEqual(1, reader.Depth);
            Assert.AreEqual("array", reader.Path);

            await reader.ReadAsync();
            Assert.AreEqual(reader.TokenType, JsonToken.Integer);
            Assert.AreEqual(1L, reader.Value);
            Assert.AreEqual(2, reader.Depth);
            Assert.AreEqual("array[0]", reader.Path);

            await reader.ReadAsync();
            Assert.AreEqual(reader.TokenType, JsonToken.Integer);
            Assert.AreEqual(2L, reader.Value);
            Assert.AreEqual(2, reader.Depth);
            Assert.AreEqual("array[1]", reader.Path);

            await reader.ReadAsync();
            Assert.AreEqual(reader.TokenType, JsonToken.StartConstructor);
            Assert.AreEqual("Date", reader.Value);
            Assert.AreEqual(2, reader.Depth);
            Assert.AreEqual("array[2]", reader.Path);

            await reader.ReadAsync();
            Assert.AreEqual(reader.TokenType, JsonToken.Integer);
            Assert.AreEqual(1L, reader.Value);
            Assert.AreEqual(3, reader.Depth);
            Assert.AreEqual("array[2][0]", reader.Path);

            await reader.ReadAsync();
            Assert.AreEqual(reader.TokenType, JsonToken.EndConstructor);
            Assert.AreEqual(null, reader.Value);
            Assert.AreEqual(2, reader.Depth);
            Assert.AreEqual("array[2]", reader.Path);

            await reader.ReadAsync();
            Assert.AreEqual(reader.TokenType, JsonToken.EndArray);
            Assert.AreEqual(1, reader.Depth);
            Assert.AreEqual("array", reader.Path);

            await reader.ReadAsync();
            Assert.AreEqual(reader.TokenType, JsonToken.PropertyName);
            Assert.AreEqual(1, reader.Depth);
            Assert.AreEqual("subobject", reader.Path);

            await reader.ReadAsync();
            Assert.AreEqual(reader.TokenType, JsonToken.StartObject);
            Assert.AreEqual(1, reader.Depth);
            Assert.AreEqual("subobject", reader.Path);

            await reader.ReadAsync();
            Assert.AreEqual(reader.TokenType, JsonToken.PropertyName);
            Assert.AreEqual(2, reader.Depth);
            Assert.AreEqual("subobject.prop", reader.Path);

            await reader.ReadAsync();
            Assert.AreEqual(reader.TokenType, JsonToken.Integer);
            Assert.AreEqual(2, reader.Depth);
            Assert.AreEqual("subobject.prop", reader.Path);

            await reader.ReadAsync();
            Assert.AreEqual(reader.TokenType, JsonToken.PropertyName);
            Assert.AreEqual(2, reader.Depth);
            Assert.AreEqual("subobject.proparray", reader.Path);

            await reader.ReadAsync();
            Assert.AreEqual(reader.TokenType, JsonToken.StartArray);
            Assert.AreEqual(2, reader.Depth);
            Assert.AreEqual("subobject.proparray", reader.Path);

            await reader.ReadAsync();
            Assert.AreEqual(reader.TokenType, JsonToken.Integer);
            Assert.AreEqual(3, reader.Depth);
            Assert.AreEqual("subobject.proparray[0]", reader.Path);

            await reader.ReadAsync();
            Assert.AreEqual(reader.TokenType, JsonToken.EndArray);
            Assert.AreEqual(2, reader.Depth);
            Assert.AreEqual("subobject.proparray", reader.Path);

            await reader.ReadAsync();
            Assert.AreEqual(reader.TokenType, JsonToken.EndObject);
            Assert.AreEqual(1, reader.Depth);
            Assert.AreEqual("subobject", reader.Path);

            await reader.ReadAsync();
            Assert.AreEqual(reader.TokenType, JsonToken.EndObject);
            Assert.AreEqual(0, reader.Depth);
            Assert.AreEqual("", reader.Path);
        }
    }

    [Fact]
    public async Task AppendCharsWhileReadingNullAsync()
    {
        var json = @"[
  {
    ""$id"": ""1"",
    ""Name"": ""e1"",
    ""Manager"": null
  },
  {
    ""$id"": ""2"",
    ""Name"": ""e2"",
    ""Manager"": null
  },
  {
    ""$ref"": ""1""
  },
  {
    ""$ref"": ""2""
  }
]";

        var reader = new JsonTextReader(new StringReader(json));
#if DEBUG
        reader.CharBuffer = new char[129];
#endif

        for (var i = 0; i < 15; i++)
        {
            await reader.ReadAsync();
        }

        await reader.ReadAsync();
        Assert.AreEqual(JsonToken.Null, reader.TokenType);
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
#if DEBUG
        reader.CharBuffer = new char[129];
#endif

        for (var i = 0; i < 14; i++)
        {
            Xunit.Assert.True(await reader.ReadAsync());
        }

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
        Assert.AreEqual("type", reader.Value);
    }

    [Fact]
    public async Task BufferTestAsync()
    {
        var json = @"{
              ""CPU"": ""Intel"",
              ""Description"": ""Amazing!\nBuy now!"",
              ""Drives"": [
                ""DVD read/writer"",
                ""500 gigabyte hard drive"",
                ""Amazing Drive" + new string('!', 9000) + @"""
              ]
            }";

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
                Console.WriteLine("Allocated buffers: " + arrayPool.FreeArrays.Count);
            }
        }

        Assert.AreEqual(0, arrayPool.UsedArrays.Count);
        Assert.AreEqual(6, arrayPool.FreeArrays.Count);
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

            Assert.Fail();
        }
        catch
        {
        }

        Assert.AreEqual(0, arrayPool.UsedArrays.Count);
        Assert.AreEqual(2, arrayPool.FreeArrays.Count);
    }

    [Fact]
    public async Task WriteReadWriteAsync()
    {
        var sb = new StringBuilder();
        var sw = new StringWriter(sb);

        using (JsonWriter jsonWriter = new JsonTextWriter(sw)
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

        var json = sb.ToString();

        var serializer = new JsonSerializer();

        var jsonObject = serializer.Deserialize(new JsonTextReader(new StringReader(json)));

        sb = new StringBuilder();
        sw = new StringWriter(sb);

        using (JsonWriter jsonWriter = new JsonTextWriter(sw)
               {
                   Formatting = Formatting.Indented
               })
        {
            serializer.Serialize(jsonWriter, jsonObject);
        }

        Assert.AreEqual(json, sb.ToString());
    }

    [Fact]
    public async Task LongStringTestAsync()
    {
        var length = 20000;
        var json = @"[""" + new string(' ', length) + @"""]";

        var reader = new JsonTextReader(new StringReader(json));

        await reader.ReadAsync();
        Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

        await reader.ReadAsync();
        Assert.AreEqual(JsonToken.String, reader.TokenType);
        Assert.AreEqual(typeof(string), reader.ValueType);
        Assert.AreEqual(20000, reader.Value.ToString().Length);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.False(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public async Task EscapedUnicodeTextAsync()
    {
        var json = @"[""\u003c"",""\u5f20""]";

        var reader = new JsonTextReader(new StringReader(json));
#if DEBUG
        reader.CharBuffer = new char[2];
#endif

        await reader.ReadAsync();
        Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

        await reader.ReadAsync();
        Assert.AreEqual("<", reader.Value);

        await reader.ReadAsync();
        Assert.AreEqual(24352, Convert.ToInt32(Convert.ToChar((string)reader.Value)));

        await reader.ReadAsync();
        Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
    }

    [Fact]
    public async Task SupportMultipleContentAsync()
    {
        var reader = new JsonTextReader(new StringReader(@"{'prop1':[1]} 1 2 ""name"" [][]null {}{} 1.1"));
        reader.SupportMultipleContent = true;

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Integer, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Integer, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Integer, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.String, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Null, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Float, reader.TokenType);

        Xunit.Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task SingleLineCommentsAsync()
    {
        var json = @"//comment*//*hi*/
{//comment
Name://comment
true//comment after true" + StringUtils.CarriageReturn +
                   @",//comment after comma" + StringUtils.CarriageReturnLineFeed +
                   @"""ExpiryDate""://comment" + StringUtils.LineFeed +
                   @"new " + StringUtils.LineFeed +
                   @"Date
(//comment
null//comment
),
        ""Price"": 3.99,
        ""Sizes"": //comment
[//comment

          ""Small""//comment
]//comment
}//comment 
//comment 1 ";

        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);
        Assert.AreEqual("comment*//*hi*/", reader.Value);
        Assert.AreEqual(1, reader.LineNumber);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(2, reader.LineNumber);
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);
        Assert.AreEqual(2, reader.LineNumber);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
        Assert.AreEqual("Name", reader.Value);
        Assert.AreEqual(3, reader.LineNumber);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);
        Assert.AreEqual(3, reader.LineNumber);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Boolean, reader.TokenType);
        Assert.True( reader.Value);
        Assert.AreEqual(4, reader.LineNumber);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);
        Assert.AreEqual("comment after true", reader.Value);
        Assert.AreEqual(4, reader.LineNumber);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);
        Assert.AreEqual("comment after comma", reader.Value);
        Assert.AreEqual(5, reader.LineNumber);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
        Assert.AreEqual("ExpiryDate", reader.Value);
        Assert.AreEqual(6, reader.LineNumber);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);
        Assert.AreEqual(6, reader.LineNumber);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);
        Assert.AreEqual(9, reader.LineNumber);
        Assert.AreEqual("Date", reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Null, reader.TokenType);
        Assert.AreEqual(10, reader.LineNumber);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);
        Assert.AreEqual(10, reader.LineNumber);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.EndConstructor, reader.TokenType);
        Assert.AreEqual(11, reader.LineNumber);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
        Assert.AreEqual("Price", reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Float, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
        Assert.AreEqual("Sizes", reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.String, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);
        Assert.AreEqual("comment ", reader.Value);

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);
        Assert.AreEqual("comment 1 ", reader.Value);

        Xunit.Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task JustSinglelineCommentAsync()
    {
        var json = @"//comment";

        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        Xunit.Assert.True(await reader.ReadAsync());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);
        Assert.AreEqual("comment", reader.Value);

        Xunit.Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task ScientificNotationAsync()
    {
        var d = Convert.ToDouble("6.0221418e23", CultureInfo.InvariantCulture);

        Assert.AreEqual("6,0221418E+23", d.ToString(new CultureInfo("fr-FR")));
        Assert.AreEqual("602214180000000000000000", d.ToString("0.#############################################################################"));

        var json = @"[0e-10,0E-10,0.25e-5,0.3e10,6.0221418e23]";

        var reader = new JsonTextReader(new StringReader(json));

        await reader.ReadAsync();

        await reader.ReadAsync();
        Assert.AreEqual(JsonToken.Float, reader.TokenType);
        Assert.AreEqual(0d, reader.Value);

        await reader.ReadAsync();
        Assert.AreEqual(JsonToken.Float, reader.TokenType);
        Assert.AreEqual(0d, reader.Value);

        await reader.ReadAsync();
        Assert.AreEqual(JsonToken.Float, reader.TokenType);
        Assert.AreEqual(0.0000025d, reader.Value);

        await reader.ReadAsync();
        Assert.AreEqual(JsonToken.Float, reader.TokenType);
        Assert.AreEqual(3000000000d, reader.Value);

        await reader.ReadAsync();
        Assert.AreEqual(JsonToken.Float, reader.TokenType);
        Assert.AreEqual(602214180000000000000000d, reader.Value);

        await reader.ReadAsync();

        reader = new JsonTextReader(new StringReader(json));

        await reader.ReadAsync();

        await reader.ReadAsDecimalAsync();
        Assert.AreEqual(JsonToken.Float, reader.TokenType);
        Assert.AreEqual(0m, reader.Value);

        await reader.ReadAsDecimalAsync();
        Assert.AreEqual(JsonToken.Float, reader.TokenType);
        Assert.AreEqual(0m, reader.Value);

        await reader.ReadAsDecimalAsync();
        Assert.AreEqual(JsonToken.Float, reader.TokenType);
        Assert.AreEqual(0.0000025m, reader.Value);

        await reader.ReadAsDecimalAsync();
        Assert.AreEqual(JsonToken.Float, reader.TokenType);
        Assert.AreEqual(3000000000m, reader.Value);

        await reader.ReadAsDecimalAsync();
        Assert.AreEqual(JsonToken.Float, reader.TokenType);
        Assert.AreEqual(602214180000000000000000m, reader.Value);

        await reader.ReadAsync();
    }

    [Fact]
    public async Task WriteReadBoundaryDecimalsAsync()
    {
        var sw = new StringWriter();
        var writer = new JsonTextWriter(sw);

        await writer.WriteStartArrayAsync();
        await writer.WriteValueAsync(decimal.MaxValue);
        await writer.WriteValueAsync(decimal.MinValue);
        await writer.WriteEndArrayAsync();

        var json = sw.ToString();

        var sr = new StringReader(json);
        var reader = new JsonTextReader(sr);

        Xunit.Assert.True(await reader.ReadAsync());

        var max = await reader.ReadAsDecimalAsync();
        Assert.AreEqual(decimal.MaxValue, max);

        var min = await reader.ReadAsDecimalAsync();
        Assert.AreEqual(decimal.MinValue, min);

        Xunit.Assert.True(await reader.ReadAsync());
    }

    [Fact]
    public async Task LinePositionOnNewLineAsync()
    {
        var json1 = "{'a':'bc'}";

        var r = new JsonTextReader(new StringReader(json1));

        Xunit.Assert.True(await r.ReadAsync());
        Assert.AreEqual(1, r.LineNumber);
        Assert.AreEqual(1, r.LinePosition);

        Xunit.Assert.True(await r.ReadAsync());
        Assert.AreEqual(1, r.LineNumber);
        Assert.AreEqual(5, r.LinePosition);

        Xunit.Assert.True(await r.ReadAsync());
        Assert.AreEqual(1, r.LineNumber);
        Assert.AreEqual(9, r.LinePosition);

        Xunit.Assert.True(await r.ReadAsync());
        Assert.AreEqual(1, r.LineNumber);
        Assert.AreEqual(10, r.LinePosition);

        Xunit.Assert.False(await r.ReadAsync());

        var json2 = "\n{'a':'bc'}";

        r = new JsonTextReader(new StringReader(json2));

        Xunit.Assert.True(await r.ReadAsync());
        Assert.AreEqual(2, r.LineNumber);
        Assert.AreEqual(1, r.LinePosition);

        Xunit.Assert.True(await r.ReadAsync());
        Assert.AreEqual(2, r.LineNumber);
        Assert.AreEqual(5, r.LinePosition);

        Xunit.Assert.True(await r.ReadAsync());
        Assert.AreEqual(2, r.LineNumber);
        Assert.AreEqual(9, r.LinePosition);

        Xunit.Assert.True(await r.ReadAsync());
        Assert.AreEqual(2, r.LineNumber);
        Assert.AreEqual(10, r.LinePosition);

        Xunit.Assert.False(await r.ReadAsync());
    }

    [Fact]
    public async Task InvalidUnicodeSequenceAsync()
    {
        var json1 = @"{'prop':'\u123!'}";

        var r = new JsonTextReader(new StringReader(json1));

        Xunit.Assert.True(await r.ReadAsync());
        Xunit.Assert.True(await r.ReadAsync());

        await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await r.ReadAsync(); }, @"Invalid Unicode escape sequence: \u123!. Path 'prop', line 1, position 11.");
    }
}