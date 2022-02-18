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

public class ReadTests : TestFixtureBase
{
    [Fact]
    public void Read_EmptyStream_ReturnsFalse()
    {
        var ms = new MemoryStream();
        var sr = new StreamReader(ms);

        var reader = new JsonTextReader(sr);
        Xunit.Assert.False(reader.Read());
    }

    [Fact]
    public void ReadAsInt32_IntegerTooLarge_ThrowsJsonReaderException()
    {
        var token = new JValue(long.MaxValue);

        ExceptionAssert.Throws<JsonReaderException>(
            () => token.CreateReader().ReadAsInt32(),
            "Could not convert to integer: 9223372036854775807. Path ''."
        );
    }

    [Fact]
    public void ReadAsDecimal_IntegerTooLarge_ThrowsJsonReaderException()
    {
        var token = new JValue(double.MaxValue);

        ExceptionAssert.Throws<JsonReaderException>(
            () => token.CreateReader().ReadAsDecimal(),
            "Could not convert to decimal: 1.79769313486232E+308. Path ''.",
            "Could not convert to decimal: 1.7976931348623157E+308. Path ''."
        );
    }

    [Fact]
    public void ReadAsInt32_BigIntegerValue_Success()
    {
        var token = new JValue(BigInteger.Parse("1"));

        var i = token.CreateReader().ReadAsInt32();
        Assert.AreEqual(1, i);
    }

    [Fact]
    public void ReadMissingInt64()
    {
        var json = "{ A: \"\", B: 1, C: , D: 1.23, E: 3.45, F: null }";

        var reader = new JsonTextReader(new StringReader(json));

        reader.Read();
        reader.Read();
        reader.Read();
        reader.Read();
        reader.Read();
        reader.Read();
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
        Assert.AreEqual("C", reader.Value);

        reader.Read();
        Assert.AreEqual(JsonToken.Undefined, reader.TokenType);
        Assert.AreEqual(null, reader.Value);
    }

    [Fact]
    public void ReadAsInt32WithUndefined()
    {
        ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                var reader = new JsonTextReader(new StringReader("undefined"));
                reader.ReadAsInt32();
            },
            "Unexpected character encountered while parsing value: u. Path '', line 1, position 1.");
    }

    [Fact]
    public void ReadAsBoolean()
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

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual("", reader.Path);

        Assert.True( reader.ReadAsBoolean());
        Assert.AreEqual("[0]", reader.Path);

        Assert.False( reader.ReadAsBoolean());
        Assert.AreEqual("[1]", reader.Path);

        Assert.True( reader.ReadAsBoolean());
        Assert.AreEqual("[2]", reader.Path);

        Assert.False( reader.ReadAsBoolean());
        Assert.AreEqual("[3]", reader.Path);

        Assert.True( reader.ReadAsBoolean());
        Assert.AreEqual("[4]", reader.Path);

        Assert.True( reader.ReadAsBoolean());
        Assert.AreEqual("[5]", reader.Path);

        Assert.True( reader.ReadAsBoolean());
        Assert.AreEqual("[6]", reader.Path);

        Assert.True( reader.ReadAsBoolean());
        Assert.AreEqual("[7]", reader.Path);

        Assert.True( reader.ReadAsBoolean());
        Assert.AreEqual("[8]", reader.Path);

        Assert.True( reader.ReadAsBoolean());
        Assert.AreEqual("[9]", reader.Path);

        Assert.True( reader.ReadAsBoolean());
        Assert.AreEqual("[10]", reader.Path);

        Assert.False( reader.ReadAsBoolean());
        Assert.AreEqual("[11]", reader.Path);

        Assert.False( reader.ReadAsBoolean());
        Assert.AreEqual("[12]", reader.Path);

        Assert.AreEqual(null, reader.ReadAsBoolean());
        Assert.AreEqual("[13]", reader.Path);

        Assert.AreEqual(null, reader.ReadAsBoolean());
        Assert.AreEqual("[14]", reader.Path);

        Assert.AreEqual(null, reader.ReadAsBoolean());
        Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
        Assert.AreEqual("", reader.Path);

        Assert.AreEqual(null, reader.ReadAsBoolean());
        Assert.AreEqual(JsonToken.None, reader.TokenType);
        Assert.AreEqual("", reader.Path);
    }

    [Fact]
    public void ReadAsBoolean_NullChar()
    {
        var json = '\0' + @"true" + '\0' + '\0';

        var reader = new JsonTextReader(new StringReader(json));

        Assert.True( reader.ReadAsBoolean());
        Assert.AreEqual(null, reader.ReadAsBoolean());
    }

    [Fact]
    public void ReadAsBytes()
    {
        var data = Encoding.UTF8.GetBytes("Hello world");

        var json = @"""" + Convert.ToBase64String(data) + @"""";

        var reader = new JsonTextReader(new StringReader(json));

        var result = reader.ReadAsBytes();

        Xunit.Assert.Equal(data, result);
    }

    [Fact]
    public void ReadAsBooleanNoContent()
    {
        var reader = new JsonTextReader(new StringReader(@""));

        Assert.IsNull(reader.ReadAsBoolean());
        Assert.AreEqual(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public void ReadAsBytesIntegerArrayWithComments()
    {
        var reader = new JsonTextReader(new StringReader(@"[/*hi*/1/*hi*/,2/*hi*/]"));

        var data = reader.ReadAsBytes();
        Assert.AreEqual(2, data.Length);
        Assert.AreEqual(1, data[0]);
        Assert.AreEqual(2, data[1]);
    }

    [Fact]
    public void ReadUnicode()
    {
        var json = @"{""Message"":""Hi,I\u0092ve send you smth""}";

        var reader = new JsonTextReader(new StringReader(json));
#if DEBUG
        reader.CharBuffer = new char[5];
#endif

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
        Assert.AreEqual("Message", reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.String, reader.TokenType);
        Assert.AreEqual(@"Hi,I" + '\u0092' + "ve send you smth", reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

        Xunit.Assert.False(reader.Read());
    }

    [Fact]
    public void ReadHexidecimalWithAllLetters()
    {
        var json = @"{""text"":0xabcdef12345}";

        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Integer, reader.TokenType);
        Assert.AreEqual(11806310474565, reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
    }

#if DEBUG
    [Fact]
    public void ReadLargeObjects()
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

            Xunit.Assert.True(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Xunit.Assert.True(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual(largeBufferLength, reader.CharBuffer.Length);

            Xunit.Assert.True(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            // buffer has been shifted before reading the second string
            Assert.AreEqual(largeBufferLength, reader.CharBuffer.Length);

            Xunit.Assert.True(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Xunit.Assert.False(reader.Read());
        }
    }
#endif

    [Fact]
    public void ReadSingleBytes()
    {
        var s = new StringReader(@"""SGVsbG8gd29ybGQu""");
        var reader = new JsonTextReader(s);

        var data = reader.ReadAsBytes();
        Assert.IsNotNull(data);

        var text = Encoding.UTF8.GetString(data, 0, data.Length);
        Assert.AreEqual("Hello world.", text);
    }

    [Fact]
    public void ReadOctalNumber()
    {
        var s = new StringReader(@"[0372, 0xFA, 0XFA]");
        var jsonReader = new JsonTextReader(s);

        Xunit.Assert.True(jsonReader.Read());
        Assert.AreEqual(JsonToken.StartArray, jsonReader.TokenType);

        Xunit.Assert.True(jsonReader.Read());
        Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
        Assert.AreEqual(250L, jsonReader.Value);

        Xunit.Assert.True(jsonReader.Read());
        Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
        Assert.AreEqual(250L, jsonReader.Value);

        Xunit.Assert.True(jsonReader.Read());
        Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
        Assert.AreEqual(250L, jsonReader.Value);

        Xunit.Assert.True(jsonReader.Read());
        Assert.AreEqual(JsonToken.EndArray, jsonReader.TokenType);

        Xunit.Assert.False(jsonReader.Read());
    }

    [Fact]
    public void ReadOctalNumberAsInt64()
    {
        var s = new StringReader(@"[0372, 0xFA, 0XFA]");
        var jsonReader = new JsonTextReader(s);

        Xunit.Assert.True(jsonReader.Read());
        Assert.AreEqual(JsonToken.StartArray, jsonReader.TokenType);

        jsonReader.Read();
        Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
        Assert.AreEqual(typeof(long), jsonReader.ValueType);
        Assert.AreEqual((long)250, (long)jsonReader.Value);

        jsonReader.Read();
        Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
        Assert.AreEqual(typeof(long), jsonReader.ValueType);
        Assert.AreEqual((long)250, (long)jsonReader.Value);

        jsonReader.Read();
        Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
        Assert.AreEqual(typeof(long), jsonReader.ValueType);
        Assert.AreEqual((long)250, (long)jsonReader.Value);

        Xunit.Assert.True(jsonReader.Read());
        Assert.AreEqual(JsonToken.EndArray, jsonReader.TokenType);

        Xunit.Assert.False(jsonReader.Read());
    }

    [Fact]
    public void ReadOctalNumberAsInt32()
    {
        var s = new StringReader(@"[0372, 0xFA, 0XFA]");
        var jsonReader = new JsonTextReader(s);

        Xunit.Assert.True(jsonReader.Read());
        Assert.AreEqual(JsonToken.StartArray, jsonReader.TokenType);

        jsonReader.ReadAsInt32();
        Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
        Assert.AreEqual(typeof(int), jsonReader.ValueType);
        Assert.AreEqual(250, jsonReader.Value);

        jsonReader.ReadAsInt32();
        Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
        Assert.AreEqual(typeof(int), jsonReader.ValueType);
        Assert.AreEqual(250, jsonReader.Value);

        jsonReader.ReadAsInt32();
        Assert.AreEqual(JsonToken.Integer, jsonReader.TokenType);
        Assert.AreEqual(typeof(int), jsonReader.ValueType);
        Assert.AreEqual(250, jsonReader.Value);

        Xunit.Assert.True(jsonReader.Read());
        Assert.AreEqual(JsonToken.EndArray, jsonReader.TokenType);

        Xunit.Assert.False(jsonReader.Read());
    }

    [Fact]
    public void ReadAsDecimalNoContent()
    {
        var reader = new JsonTextReader(new StringReader(@""));

        Assert.IsNull(reader.ReadAsDecimal());
        Assert.AreEqual(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public void ReadAsBytesNoContent()
    {
        var reader = new JsonTextReader(new StringReader(@""));

        Assert.IsNull(reader.ReadAsBytes());
        Assert.AreEqual(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public void ReadAsDateTimeOffsetNoContent()
    {
        var reader = new JsonTextReader(new StringReader(@""));

        Assert.IsNull(reader.ReadAsDateTimeOffset());
        Assert.AreEqual(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public void ReadAsDateTimeOffset()
    {
        var json = "{\"Offset\":\"\\/Date(946663200000+0600)\\/\"}";

        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        reader.ReadAsDateTimeOffset();
        Assert.AreEqual(JsonToken.Date, reader.TokenType);
        Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
        Assert.AreEqual(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(6)), reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public void ReadAsDateTimeOffsetNegative()
    {
        var json = @"{""Offset"":""\/Date(946706400000-0600)\/""}";

        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        reader.ReadAsDateTimeOffset();
        Assert.AreEqual(JsonToken.Date, reader.TokenType);
        Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
        Assert.AreEqual(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(-6)), reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public void ReadAsDateTimeOffsetBadString()
    {
        var json = @"{""Offset"":""blablahbla""}";

        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsDateTimeOffset(); }, "Could not convert string to DateTimeOffset: blablahbla. Path 'Offset', line 1, position 22.");
    }

    [Fact]
    public void ReadAsDateTimeOffsetHoursOnly()
    {
        var json = "{\"Offset\":\"\\/Date(946663200000+06)\\/\"}";

        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        reader.ReadAsDateTimeOffset();
        Assert.AreEqual(JsonToken.Date, reader.TokenType);
        Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
        Assert.AreEqual(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(6)), reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public void ReadAsDateTimeOffsetWithMinutes()
    {
        var json = @"{""Offset"":""\/Date(946708260000-0631)\/""}";

        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        reader.ReadAsDateTimeOffset();
        Assert.AreEqual(JsonToken.Date, reader.TokenType);
        Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
        Assert.AreEqual(new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(-6).Add(TimeSpan.FromMinutes(-31))), reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public void ReadAsDateTimeOffsetIsoDate()
    {
        var json = @"{""Offset"":""2011-08-01T21:25Z""}";

        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        reader.ReadAsDateTimeOffset();
        Assert.AreEqual(JsonToken.Date, reader.TokenType);
        Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
        Assert.AreEqual(new DateTimeOffset(new DateTime(2011, 8, 1, 21, 25, 0, DateTimeKind.Utc), TimeSpan.Zero), reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public void ReadAsDateTimeOffsetUnitedStatesDate()
    {
        var json = @"{""Offset"":""1/30/2011""}";

        var reader = new JsonTextReader(new StringReader(json));
        reader.Culture = new CultureInfo("en-US");

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        reader.ReadAsDateTimeOffset();
        Assert.AreEqual(JsonToken.Date, reader.TokenType);
        Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);

        var dt = (DateTimeOffset)reader.Value;
        Assert.AreEqual(new DateTime(2011, 1, 30, 0, 0, 0, DateTimeKind.Unspecified), dt.DateTime);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public void ReadAsDateTimeOffsetNewZealandDate()
    {
        var json = @"{""Offset"":""30/1/2011""}";

        var reader = new JsonTextReader(new StringReader(json));
        reader.Culture = new CultureInfo("en-NZ");

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        reader.ReadAsDateTimeOffset();
        Assert.AreEqual(JsonToken.Date, reader.TokenType);
        Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);

        var dt = (DateTimeOffset)reader.Value;
        Assert.AreEqual(new DateTime(2011, 1, 30, 0, 0, 0, DateTimeKind.Unspecified), dt.DateTime);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public void ReadAsDecimalInt()
    {
        var json = @"{""Name"":1}";

        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        reader.ReadAsDecimal();
        Assert.AreEqual(JsonToken.Float, reader.TokenType);
        Assert.AreEqual(typeof(decimal), reader.ValueType);
        Assert.AreEqual(1m, reader.Value);
    }

    [Fact]
    public void ReadAsIntDecimal()
    {
        var json = @"{""Name"": 1.1}";

        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        ExceptionAssert.Throws<JsonReaderException>(() => { reader.ReadAsInt32(); }, "Input string '1.1' is not a valid integer. Path 'Name', line 1, position 12.");
    }

    [Fact]
    public void ReadAsDecimal()
    {
        var json = @"{""decimal"":-7.92281625142643E+28}";

        var reader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        var d = reader.ReadAsDecimal();
        Assert.AreEqual(JsonToken.Float, reader.TokenType);
        Assert.AreEqual(typeof(decimal), reader.ValueType);
        Assert.AreEqual(-79228162514264300000000000000m, d);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public void ReadAsDecimalFrench()
    {
        var json = @"{""decimal"":""9,99""}";

        var reader = new JsonTextReader(new StringReader(json));
        reader.Culture = new CultureInfo("fr-FR");

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        var d = reader.ReadAsDecimal();
        Assert.AreEqual(JsonToken.Float, reader.TokenType);
        Assert.AreEqual(typeof(decimal), reader.ValueType);
        Assert.AreEqual(9.99m, d);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
    }

    [Fact]
    public void ReadBufferOnControlChar()
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
            reader.Read();
        }

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(new DateTime(631136448000000000), reader.Value);
    }

    [Fact]
    public void ReadBufferOnEndComment()
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
            Xunit.Assert.True(reader.Read());
        }

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.False(reader.Read());
    }

    [Fact]
    public void ReadAsDouble_Null()
    {
        var reader = new JsonTextReader(new StringReader("null"));
        Assert.AreEqual(null, reader.ReadAsDouble());
    }

    [Fact]
    public void ReadAsDouble_Success()
    {
        var reader = new JsonTextReader(new StringReader("'12.34'"));
        Assert.AreEqual(12.34d, reader.ReadAsDouble());
    }

    [Fact]
    public void ReadAsDouble_Hex()
    {
        var reader = new JsonTextReader(new StringReader("0XCAFEBABE"));
        Assert.AreEqual(3405691582d, reader.ReadAsDouble());
    }

    [Fact]
    public void ReadAsDouble_AllowThousands()
    {
        var reader = new JsonTextReader(new StringReader("'1,112.34'"));
        Assert.AreEqual(1112.34d, reader.ReadAsDouble());
    }

    [Fact]
    public void ReadAsDouble_Failure()
    {
        var reader = new JsonTextReader(new StringReader("['Trump',1]"));

        Xunit.Assert.True(reader.Read());

        ExceptionAssert.Throws<JsonReaderException>(
            () => { reader.ReadAsDouble(); },
            "Could not convert string to double: Trump. Path '[0]', line 1, position 8.");

        Assert.AreEqual(1d, reader.ReadAsDouble());
        Xunit.Assert.True(reader.Read());
    }

    [Fact]
    public void ReadAsString_Boolean()
    {
        var reader = new JsonTextReader(new StringReader("{\"Test1\":false}"));

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.True(reader.Read());

        var s = reader.ReadAsString();
        Assert.AreEqual("false", s);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.False(reader.Read());
    }

    [Fact]
    public void Read_Boolean_Failure()
    {
        var reader = new JsonTextReader(new StringReader("{\"Test1\":false1}"));

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.True(reader.Read());

        ExceptionAssert.Throws<JsonReaderException>(
            () => { reader.Read(); },
            "Error parsing boolean value. Path 'Test1', line 1, position 14.");

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.False(reader.Read());
    }

    [Fact]
    public void ReadAsString_Boolean_Failure()
    {
        var reader = new JsonTextReader(new StringReader("{\"Test1\":false1}"));

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.True(reader.Read());

        ExceptionAssert.Throws<JsonReaderException>(
            () => { reader.ReadAsString(); },
            "Unexpected character encountered while parsing value: 1. Path 'Test1', line 1, position 14.");

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.False(reader.Read());
    }

    [Fact]
    public void ReadValue_EmptyString_Position()
    {
        var json = @"['','','','','','','']";

        var reader = new JsonTextReader(new StringReader(json));

        reader.Read();
        reader.ReadAsInt32();
        Assert.AreEqual("[0]", reader.Path);
        reader.ReadAsDecimal();
        Assert.AreEqual("[1]", reader.Path);
        reader.ReadAsDateTime();
        Assert.AreEqual("[2]", reader.Path);
        reader.ReadAsDateTimeOffset();
        Assert.AreEqual("[3]", reader.Path);
        reader.ReadAsString();
        Assert.AreEqual("[4]", reader.Path);
        reader.ReadAsBytes();
        Assert.AreEqual("[5]", reader.Path);
        reader.ReadAsDouble();
        Assert.AreEqual("[6]", reader.Path);

        Assert.IsNull(reader.ReadAsString());
        Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

        Assert.IsNull(reader.ReadAsString());
        Assert.AreEqual(JsonToken.None, reader.TokenType);

        Assert.IsNull(reader.ReadAsBytes());
        Assert.AreEqual(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public void ReadValueComments()
    {
        var json = @"/*comment*/[/*comment*/1/*comment*/,2,/*comment*//*comment*/""three""/*comment*/,""four""/*comment*/,null,/*comment*/null,3.99,1.1/*comment*/,''/*comment*/]/*comment*/";

        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

        Assert.AreEqual(1, reader.ReadAsInt32());
        Assert.AreEqual(JsonToken.Integer, reader.TokenType);

        Assert.AreEqual(2, reader.ReadAsInt32());
        Assert.AreEqual(JsonToken.Integer, reader.TokenType);

        Assert.AreEqual("three", reader.ReadAsString());
        Assert.AreEqual(JsonToken.String, reader.TokenType);

        Assert.AreEqual("four", reader.ReadAsString());
        Assert.AreEqual(JsonToken.String, reader.TokenType);

        Assert.AreEqual(null, reader.ReadAsString());
        Assert.AreEqual(JsonToken.Null, reader.TokenType);

        Assert.AreEqual(null, reader.ReadAsInt32());
        Assert.AreEqual(JsonToken.Null, reader.TokenType);

        Assert.AreEqual(3.99m, reader.ReadAsDecimal());
        Assert.AreEqual(JsonToken.Float, reader.TokenType);

        Assert.AreEqual(1.1m, reader.ReadAsDecimal());
        Assert.AreEqual(JsonToken.Float, reader.TokenType);

        Xunit.Assert.Equal(new byte[0], reader.ReadAsBytes());
        Assert.AreEqual(JsonToken.Bytes, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

        Assert.AreEqual(null, reader.ReadAsInt32());
        Assert.AreEqual(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public void ReadContentDelimitedByComments()
    {
        var json = @"/*comment*/{/*comment*/Name:/*comment*/true/*comment*/,/*comment*/
        ""ExpiryDate"":/*comment*/new
" + StringUtils.LineFeed +
                   @"Date
(/*comment*/null/*comment*/),
        ""Price"": 3.99,
        ""Sizes"":/*comment*/[/*comment*/
          ""Small""/*comment*/]/*comment*/}/*comment*/";

        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
        Assert.AreEqual("Name", reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Boolean, reader.TokenType);
        Assert.True( reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
        Assert.AreEqual("ExpiryDate", reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);
        Assert.AreEqual(5, reader.LineNumber);
        Assert.AreEqual("Date", reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Null, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.EndConstructor, reader.TokenType);
    }

    [Fact]
    public void ReadNullIntLineNumberAndPosition()
    {
        var json = @"[
  1,
  2,
  3,
  null
]";

        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        reader.Read();
        Assert.AreEqual(1, reader.LineNumber);

        reader.ReadAsInt32();
        Assert.AreEqual(2, reader.LineNumber);
        Assert.AreEqual("[0]", reader.Path);

        reader.ReadAsInt32();
        Assert.AreEqual(3, reader.LineNumber);
        Assert.AreEqual("[1]", reader.Path);

        reader.ReadAsInt32();
        Assert.AreEqual(4, reader.LineNumber);
        Assert.AreEqual("[2]", reader.Path);

        reader.ReadAsInt32();
        Assert.AreEqual(5, reader.LineNumber);
        Assert.AreEqual("[3]", reader.Path);

        reader.Read();
        Assert.AreEqual(6, reader.LineNumber);
        Assert.AreEqual(string.Empty, reader.Path);

        Xunit.Assert.False(reader.Read());
    }

    [Fact]
    public void ReadingFromSlowStream()
    {
        var json = "[false, true, true, false, 'test!', 1.11, 0e-10, 0E-10, 0.25e-5, 0.3e10, 6.0221418e23, 'Purple\\r \\n monkey\\'s:\\tdishwasher']";

        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        Xunit.Assert.True(reader.Read());

        Xunit.Assert.True(reader.Read());
        Assert.False( reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Boolean, reader.TokenType);
        Assert.True( reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Boolean, reader.TokenType);
        Assert.True( reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Boolean, reader.TokenType);
        Assert.False( reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.String, reader.TokenType);
        Assert.AreEqual("test!", reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Float, reader.TokenType);
        Assert.AreEqual(1.11d, reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Float, reader.TokenType);
        Assert.AreEqual(0d, reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Float, reader.TokenType);
        Assert.AreEqual(0d, reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Float, reader.TokenType);
        Assert.AreEqual(0.0000025d, reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Float, reader.TokenType);
        Assert.AreEqual(3000000000d, reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Float, reader.TokenType);
        Assert.AreEqual(602214180000000000000000d, reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.String, reader.TokenType);
        Assert.AreEqual(reader.Value, "Purple\r \n monkey's:\tdishwasher");

        Xunit.Assert.True(reader.Read());
    }

    [Fact]
    public void ReadCommentInsideArray()
    {
        var json = @"{
    ""projects"": [
        ""src"",
        //""
        ""test""
    ]
}";

        var jsonTextReader = new JsonTextReader(new StringReader(json));
        Xunit.Assert.True(jsonTextReader.Read());
        Assert.AreEqual(JsonToken.StartObject, jsonTextReader.TokenType);

        Xunit.Assert.True(jsonTextReader.Read());
        Assert.AreEqual(JsonToken.PropertyName, jsonTextReader.TokenType);

        Xunit.Assert.True(jsonTextReader.Read());
        Assert.AreEqual(JsonToken.StartArray, jsonTextReader.TokenType);

        Xunit.Assert.True(jsonTextReader.Read());
        Assert.AreEqual(JsonToken.String, jsonTextReader.TokenType);
        Assert.AreEqual("src", jsonTextReader.Value);

        Xunit.Assert.True(jsonTextReader.Read());
        Assert.AreEqual(JsonToken.Comment, jsonTextReader.TokenType);
        Assert.AreEqual(@"""", jsonTextReader.Value);

        Xunit.Assert.True(jsonTextReader.Read());
        Assert.AreEqual(JsonToken.String, jsonTextReader.TokenType);
        Assert.AreEqual("test", jsonTextReader.Value);

        Xunit.Assert.True(jsonTextReader.Read());
        Assert.AreEqual(JsonToken.EndArray, jsonTextReader.TokenType);

        Xunit.Assert.True(jsonTextReader.Read());
        Assert.AreEqual(JsonToken.EndObject, jsonTextReader.TokenType);
    }

    [Fact]
    public void ReadAsBytes_Base64AndGuid()
    {
        var jsonTextReader = new JsonTextReader(new StringReader("'AAAAAAAAAAAAAAAAAAAAAAAAAAABAAAA'"));
        var data = jsonTextReader.ReadAsBytes();
        var expected = Convert.FromBase64String("AAAAAAAAAAAAAAAAAAAAAAAAAAABAAAA");

        Xunit.Assert.Equal(expected, data);

        jsonTextReader = new JsonTextReader(new StringReader("'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAABAAAA'"));
        data = jsonTextReader.ReadAsBytes();
        expected = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAABAAAA").ToByteArray();

        Xunit.Assert.Equal(expected, data);
    }

    [Fact]
    public void ReadSingleQuoteInsideDoubleQuoteString()
    {
        var json = @"{""NameOfStore"":""Forest's Bakery And Cafe""}";

        var jsonTextReader = new JsonTextReader(new StringReader(json));
        jsonTextReader.Read();
        jsonTextReader.Read();
        jsonTextReader.Read();

        Assert.AreEqual(@"Forest's Bakery And Cafe", jsonTextReader.Value);
    }

    [Fact]
    public void ReadMultilineString()
    {
        var json = @"""first line
second line
third line""";

        var jsonTextReader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(jsonTextReader.Read());
        Assert.AreEqual(JsonToken.String, jsonTextReader.TokenType);

        Assert.AreEqual(@"first line
second line
third line", jsonTextReader.Value);
    }

    [Fact]
    public void ReadBigInteger()
    {
        var json = @"{
    ParentId: 1,
    ChildId: 333333333333333333333333333333333333333,
}";

        var jsonTextReader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(jsonTextReader.Read());
        Assert.AreEqual(JsonToken.StartObject, jsonTextReader.TokenType);

        Xunit.Assert.True(jsonTextReader.Read());
        Assert.AreEqual(JsonToken.PropertyName, jsonTextReader.TokenType);

        Xunit.Assert.True(jsonTextReader.Read());
        Assert.AreEqual(JsonToken.Integer, jsonTextReader.TokenType);

        Xunit.Assert.True(jsonTextReader.Read());
        Assert.AreEqual(JsonToken.PropertyName, jsonTextReader.TokenType);

        Xunit.Assert.True(jsonTextReader.Read());
        Assert.AreEqual(JsonToken.Integer, jsonTextReader.TokenType);
        Assert.AreEqual(typeof(BigInteger), jsonTextReader.ValueType);
        Assert.AreEqual(BigInteger.Parse("333333333333333333333333333333333333333"), jsonTextReader.Value);

        Xunit.Assert.True(jsonTextReader.Read());
        Assert.AreEqual(JsonToken.EndObject, jsonTextReader.TokenType);

        Xunit.Assert.False(jsonTextReader.Read());

        var o = JObject.Parse(json);
        var i = (BigInteger)((JValue)o["ChildId"]).Value;
        Assert.AreEqual(BigInteger.Parse("333333333333333333333333333333333333333"), i);
    }

    [Fact]
    public void ReadBadMSDateAsString()
    {
        var json = @"{
    ChildId: '\/Date(9467082_PIE_340000-0631)\/'
}";

        var jsonTextReader = new JsonTextReader(new StringReader(json));

        Xunit.Assert.True(jsonTextReader.Read());
        Assert.AreEqual(JsonToken.StartObject, jsonTextReader.TokenType);

        Xunit.Assert.True(jsonTextReader.Read());
        Assert.AreEqual(JsonToken.PropertyName, jsonTextReader.TokenType);

        Xunit.Assert.True(jsonTextReader.Read());
        Assert.AreEqual(JsonToken.String, jsonTextReader.TokenType);
        Assert.AreEqual(@"/Date(9467082_PIE_340000-0631)/", jsonTextReader.Value);

        Xunit.Assert.True(jsonTextReader.Read());
        Assert.AreEqual(JsonToken.EndObject, jsonTextReader.TokenType);

        Xunit.Assert.False(jsonTextReader.Read());
    }

    [Fact]
    public void ReadConstructor()
    {
        var json = @"{""DefaultConverter"":new Date(0, ""hi""),""MemberConverter"":""1970-01-01T00:00:00Z""}";

        JsonReader reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);
        Assert.AreEqual("Date", reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(0L, reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual("hi", reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.EndConstructor, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual("MemberConverter", reader.Value);
    }

    [Fact]
    public void ReadingIndented()
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

            Assert.AreEqual(jsonReader.TokenType, JsonToken.None);
            Assert.AreEqual(0, jsonReader.LineNumber);
            Assert.AreEqual(0, jsonReader.LinePosition);

            jsonReader.Read();
            Assert.AreEqual(jsonReader.TokenType, JsonToken.StartObject);
            Assert.AreEqual(1, jsonReader.LineNumber);
            Assert.AreEqual(1, jsonReader.LinePosition);

            jsonReader.Read();
            Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);
            Assert.AreEqual(jsonReader.Value, "CPU");
            Assert.AreEqual(2, jsonReader.LineNumber);
            Assert.AreEqual(6, jsonReader.LinePosition);

            jsonReader.Read();
            Assert.AreEqual(JsonToken.String, jsonReader.TokenType);
            Assert.AreEqual("Intel", jsonReader.Value);
            Assert.AreEqual(2, jsonReader.LineNumber);
            Assert.AreEqual(14, jsonReader.LinePosition);

            jsonReader.Read();
            Assert.AreEqual(jsonReader.TokenType, JsonToken.PropertyName);
            Assert.AreEqual(jsonReader.Value, "Drives");
            Assert.AreEqual(3, jsonReader.LineNumber);
            Assert.AreEqual(9, jsonReader.LinePosition);

            jsonReader.Read();
            Assert.AreEqual(jsonReader.TokenType, JsonToken.StartArray);
            Assert.AreEqual(3, jsonReader.LineNumber);
            Assert.AreEqual(11, jsonReader.LinePosition);

            jsonReader.Read();
            Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
            Assert.AreEqual(jsonReader.Value, "DVD read/writer");
            Assert.AreEqual(jsonReader.QuoteChar, '\'');
            Assert.AreEqual(4, jsonReader.LineNumber);
            Assert.AreEqual(21, jsonReader.LinePosition);

            jsonReader.Read();
            Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
            Assert.AreEqual(jsonReader.Value, "500 gigabyte hard drive");
            Assert.AreEqual(jsonReader.QuoteChar, '"');
            Assert.AreEqual(5, jsonReader.LineNumber);
            Assert.AreEqual(29, jsonReader.LinePosition);

            jsonReader.Read();
            Assert.AreEqual(jsonReader.TokenType, JsonToken.EndArray);
            Assert.AreEqual(6, jsonReader.LineNumber);
            Assert.AreEqual(3, jsonReader.LinePosition);

            jsonReader.Read();
            Assert.AreEqual(jsonReader.TokenType, JsonToken.EndObject);
            Assert.AreEqual(7, jsonReader.LineNumber);
            Assert.AreEqual(1, jsonReader.LinePosition);

            Xunit.Assert.False(jsonReader.Read());
        }
    }

    [Fact]
    public void ReadLongString()
    {
        var s = new string('a', 10000);
        JsonReader reader = new JsonTextReader(new StringReader("'" + s + "'"));
        reader.Read();

        Assert.AreEqual(s, reader.Value);
    }

    [Fact]
    public void ReadLongJsonArray()
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
        Xunit.Assert.True(reader.Read());
        for (var i = 0; i < valueCount; i++)
        {
            Xunit.Assert.True(reader.Read());
            Assert.AreEqual((long)i, reader.Value);
        }
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.False(reader.Read());
    }

    [Fact]
    public void NullCharReading()
    {
        var json = "\0{\0'\0h\0i\0'\0:\0[\01\0,\0'\0'\0\0,\0null\0]\0,\0do\0:true\0}\0\0/*\0sd\0f\0*/\0/*\0sd\0f\0*/ \0";
        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Integer, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.String, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Null, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Boolean, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);
        Assert.AreEqual("\0sd\0f\0", reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Comment, reader.TokenType);
        Assert.AreEqual("\0sd\0f\0", reader.Value);

        Xunit.Assert.False(reader.Read());
    }

    [Fact]
    public void ReadNullTerminatorStrings()
    {
        JsonReader reader = new JsonTextReader(new StringReader("'h\0i'"));
        Xunit.Assert.True(reader.Read());

        Assert.AreEqual("h\0i", reader.Value);
    }

    [Fact]
    public void ReadBytesNoStartWithUnexpectedEnd()
    {
        JsonReader reader = new JsonTextReader(new StringReader(@"[  "));
        Xunit.Assert.True(reader.Read());

        Assert.IsNull(reader.ReadAsBytes());
        Assert.AreEqual(JsonToken.None, reader.TokenType);
    }

    [Fact]
    public void ReadNewLines()
    {
        var newLinesText = StringUtils.CarriageReturn + StringUtils.CarriageReturnLineFeed + StringUtils.LineFeed + StringUtils.CarriageReturnLineFeed + " " + StringUtils.CarriageReturn + StringUtils.CarriageReturnLineFeed;

        var json =
            newLinesText
            + "{" + newLinesText
            + "'" + newLinesText
            + "name1" + newLinesText
            + "'" + newLinesText
            + ":" + newLinesText
            + "[" + newLinesText
            + "new" + newLinesText
            + "Date" + newLinesText
            + "(" + newLinesText
            + "1" + newLinesText
            + "," + newLinesText
            + "null" + newLinesText
            + "/*" + newLinesText
            + "blah comment" + newLinesText
            + "*/" + newLinesText
            + ")" + newLinesText
            + "," + newLinesText
            + "1.1111" + newLinesText
            + "]" + newLinesText
            + "," + newLinesText
            + "name2" + newLinesText
            + ":" + newLinesText
            + "{" + newLinesText
            + "}" + newLinesText
            + "}" + newLinesText;

        var count = 0;
        var sr = new StringReader(newLinesText);
        while (sr.ReadLine() != null)
        {
            count++;
        }

        var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));
        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(7, reader.LineNumber);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(31, reader.LineNumber);
        Assert.AreEqual(newLinesText + "name1" + newLinesText, reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(37, reader.LineNumber);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(55, reader.LineNumber);
        Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);
        Assert.AreEqual("Date", reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(61, reader.LineNumber);
        Assert.AreEqual(1L, reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(73, reader.LineNumber);
        Assert.AreEqual(null, reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(91, reader.LineNumber);
        Assert.AreEqual(newLinesText + "blah comment" + newLinesText, reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(97, reader.LineNumber);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(109, reader.LineNumber);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(115, reader.LineNumber);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(133, reader.LineNumber);
        Assert.AreEqual("name2", reader.Value);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(139, reader.LineNumber);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(145, reader.LineNumber);

        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(151, reader.LineNumber);
    }

    [Fact]
    public void ReadBytesFollowingNumberInArray()
    {
        var helloWorld = "Hello world!";
        var helloWorldData = Encoding.UTF8.GetBytes(helloWorld);

        JsonReader reader = new JsonTextReader(new StringReader(@"[1,'" + Convert.ToBase64String(helloWorldData) + @"']"));
        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartArray, reader.TokenType);
        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Integer, reader.TokenType);
        var data = reader.ReadAsBytes();
        Xunit.Assert.Equal(helloWorldData, data);
        Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.False(reader.Read());
    }

    [Fact]
    public void ReadBytesFollowingNumberInObject()
    {
        var helloWorld = "Hello world!";
        var helloWorldData = Encoding.UTF8.GetBytes(helloWorld);

        JsonReader reader = new JsonTextReader(new StringReader(@"{num:1,data:'" + Convert.ToBase64String(helloWorldData) + @"'}"));
        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.StartObject, reader.TokenType);
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.Integer, reader.TokenType);
        Xunit.Assert.True(reader.Read());
        var data = reader.ReadAsBytes();
        Xunit.Assert.Equal(helloWorldData, data);
        Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
        Xunit.Assert.True(reader.Read());
        Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

        Xunit.Assert.False(reader.Read());
    }

    [Fact]
    public void ReadingEscapedStrings()
    {
        var input = "{value:'Purple\\r \\n monkey\\'s:\\tdishwasher'}";

        var sr = new StringReader(input);

        using (JsonReader jsonReader = new JsonTextReader(sr))
        {
            Assert.AreEqual(0, jsonReader.Depth);

            jsonReader.Read();
            Assert.AreEqual(JsonToken.StartObject, jsonReader.TokenType);
            Assert.AreEqual(0, jsonReader.Depth);

            jsonReader.Read();
            Assert.AreEqual(JsonToken.PropertyName, jsonReader.TokenType);
            Assert.AreEqual(1, jsonReader.Depth);

            jsonReader.Read();
            Assert.AreEqual(jsonReader.TokenType, JsonToken.String);
            Assert.AreEqual("Purple\r \n monkey's:\tdishwasher", jsonReader.Value);
            Assert.AreEqual('\'', jsonReader.QuoteChar);
            Assert.AreEqual(1, jsonReader.Depth);

            jsonReader.Read();
            Assert.AreEqual(JsonToken.EndObject, jsonReader.TokenType);
            Assert.AreEqual(0, jsonReader.Depth);
        }
    }

    [Fact]
    public void ReadNewlineLastCharacter()
    {
        var input = @"{
  CPU: 'Intel',
  Drives: [ /* Com*ment */
    'DVD read/writer',
    ""500 gigabyte hard drive""
  ]
}" + '\n';

        var o = JsonConvert.DeserializeObject(input);
    }

    [Fact]
    public void ReadRandomJson()
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
        while (reader.Read())
        {
        }
    }

    [Fact]
    public void ThrowOnDuplicateKeysDeserializing()
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

        var settings = new JsonLoadSettings {DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error};

        var reader = new JsonTextReader(new StringReader(json));
        ExceptionAssert.Throws<JsonException>(() =>
        {
            JToken.ReadFrom(reader, settings);
        });
    }

    [Fact]
    public void MaxDepth_GreaterThanDefault()
    {
        var json = GetNestedJson(150);

        var reader = new JsonTextReader(new StringReader(json));
        reader.MaxDepth = 150;

        while (reader.Read())
        {
        }
    }

    [Fact]
    public void MaxDepth_Null()
    {
        var json = GetNestedJson(150);

        var reader = new JsonTextReader(new StringReader(json));
        reader.MaxDepth = null;

        while (reader.Read())
        {
        }
    }

    [Fact]
    public void MaxDepth_MaxValue()
    {
        var json = GetNestedJson(150);

        var reader = new JsonTextReader(new StringReader(json));
        reader.MaxDepth = int.MaxValue;

        while (reader.Read())
        {
        }
    }
}