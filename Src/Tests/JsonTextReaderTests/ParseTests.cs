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

namespace Argon.Tests.JsonTextReaderTests
{
    [TestFixture]
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
            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("hi\r\nbye", reader.Value);
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual(1L, reader.Value);
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
            Assert.IsFalse(reader.Read());
        }

        [Fact]
        public void ParseIntegers()
        {
            JsonTextReader reader = null;

            reader = new JsonTextReader(new StringReader("1"));
            Assert.AreEqual(1, reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader("-1"));
            Assert.AreEqual(-1, reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader("0"));
            Assert.AreEqual(0, reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader("-0"));
            Assert.AreEqual(0, reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader(int.MaxValue.ToString()));
            Assert.AreEqual(int.MaxValue, reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader(int.MinValue.ToString()));
            Assert.AreEqual(int.MinValue, reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader(long.MaxValue.ToString()));
            ExceptionAssert.Throws<JsonReaderException>(() => reader.ReadAsInt32(), "JSON integer 9223372036854775807 is too large or small for an Int32. Path '', line 1, position 19.");

            reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
            ExceptionAssert.Throws<JsonReaderException>(() => reader.ReadAsInt32(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

            reader = new JsonTextReader(new StringReader("1E-06"));
            ExceptionAssert.Throws<JsonReaderException>(() => reader.ReadAsInt32(), "Input string '1E-06' is not a valid integer. Path '', line 1, position 5.");

            reader = new JsonTextReader(new StringReader("1.1"));
            ExceptionAssert.Throws<JsonReaderException>(() => reader.ReadAsInt32(), "Input string '1.1' is not a valid integer. Path '', line 1, position 3.");

            reader = new JsonTextReader(new StringReader(""));
            Assert.AreEqual(null, reader.ReadAsInt32());

            reader = new JsonTextReader(new StringReader("-"));
            ExceptionAssert.Throws<JsonReaderException>(() => reader.ReadAsInt32(), "Input string '-' is not a valid integer. Path '', line 1, position 1.");
        }

        [Fact]
        public void ParseDecimals()
        {
            JsonTextReader reader = null;

            reader = new JsonTextReader(new StringReader("1.1"));
            Assert.AreEqual(1.1m, reader.ReadAsDecimal());

            reader = new JsonTextReader(new StringReader("-1.1"));
            Assert.AreEqual(-1.1m, reader.ReadAsDecimal());

            reader = new JsonTextReader(new StringReader("0.0"));
            Assert.AreEqual(0.0m, reader.ReadAsDecimal());

            reader = new JsonTextReader(new StringReader("-0.0"));
            Assert.AreEqual(0, reader.ReadAsDecimal());

            reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
            ExceptionAssert.Throws<JsonReaderException>(() => reader.ReadAsDecimal(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

            reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
            reader.FloatParseHandling = Argon.FloatParseHandling.Decimal;
            ExceptionAssert.Throws<JsonReaderException>(() => reader.Read(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

            reader = new JsonTextReader(new StringReader("1E-06"));
            Assert.AreEqual(0.000001m, reader.ReadAsDecimal());

            reader = new JsonTextReader(new StringReader(""));
            Assert.AreEqual(null, reader.ReadAsDecimal());

            reader = new JsonTextReader(new StringReader("-"));
            ExceptionAssert.Throws<JsonReaderException>(() => reader.ReadAsDecimal(), "Input string '-' is not a valid decimal. Path '', line 1, position 1.");
        }

        [Fact]
        public void ParseDoubles()
        {
            JsonTextReader reader = null;

            reader = new JsonTextReader(new StringReader("1.1"));
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(typeof(double), reader.ValueType);
            Assert.AreEqual(1.1d, reader.Value);

            reader = new JsonTextReader(new StringReader("-1.1"));
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(typeof(double), reader.ValueType);
            Assert.AreEqual(-1.1d, reader.Value);

            reader = new JsonTextReader(new StringReader("0.0"));
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(typeof(double), reader.ValueType);
            Assert.AreEqual(0.0d, reader.Value);

            reader = new JsonTextReader(new StringReader("-0.0"));
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(typeof(double), reader.ValueType);
            Assert.AreEqual(-0.0d, reader.Value);

            reader = new JsonTextReader(new StringReader("9999999999999999999999999999999999999999999999999999999999999999999999999999asdasdasd"));
            ExceptionAssert.Throws<JsonReaderException>(() => reader.Read(), "Unexpected character encountered while parsing number: s. Path '', line 1, position 77.");

            reader = new JsonTextReader(new StringReader("1E-06"));
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(typeof(double), reader.ValueType);
            Assert.AreEqual(0.000001d, reader.Value);

            reader = new JsonTextReader(new StringReader(""));
            Assert.IsFalse(reader.Read());

            reader = new JsonTextReader(new StringReader("-"));
            ExceptionAssert.Throws<JsonReaderException>(() => reader.Read(), "Input string '-' is not a valid number. Path '', line 1, position 1.");

            reader = new JsonTextReader(new StringReader("1.7976931348623157E+308"));
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(typeof(double), reader.ValueType);
            Assert.AreEqual(Double.MaxValue, reader.Value);

            reader = new JsonTextReader(new StringReader("-1.7976931348623157E+308"));
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(typeof(double), reader.ValueType);
            Assert.AreEqual(Double.MinValue, reader.Value);

            reader = new JsonTextReader(new StringReader("1E+309"));
#if !(NETSTANDARD2_0)
            ExceptionAssert.Throws<JsonReaderException>(() => reader.Read(), "Input string '1E+309' is not a valid number. Path '', line 1, position 6.");
#else
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(typeof(double), reader.ValueType);
            Assert.AreEqual(Double.PositiveInfinity, reader.Value);
#endif

            reader = new JsonTextReader(new StringReader("-1E+5000"));
#if !(NETSTANDARD2_0)
            ExceptionAssert.Throws<JsonReaderException>(() => reader.Read(), "Input string '-1E+5000' is not a valid number. Path '', line 1, position 8.");
#else
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(typeof(double), reader.ValueType);
            Assert.AreEqual(Double.NegativeInfinity, reader.Value);
#endif

            reader = new JsonTextReader(new StringReader("5.1231231E"));
            ExceptionAssert.Throws<JsonReaderException>(() => reader.Read(), "Input string '5.1231231E' is not a valid number. Path '', line 1, position 10.");

            reader = new JsonTextReader(new StringReader("1E-23"));
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(typeof(double), reader.ValueType);
            Assert.AreEqual(1e-23, reader.Value);
        }

        [Fact]
        public void ParseArrayWithMissingValues()
        {
            var json = "[,,, \n\r\n \0   \r  , ,    ]";
            var reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Undefined, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Undefined, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Undefined, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Undefined, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Undefined, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
        }

        [Fact]
        public void ParseBooleanWithNoExtraContent()
        {
            var json = "[true ";
            var reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.IsFalse(reader.Read());
        }

        [Fact]
        public void ParseContentDelimitedByNonStandardWhitespace()
        {
            var json = "\x00a0{\x00a0'h\x00a0i\x00a0'\x00a0:\x00a0[\x00a0true\x00a0,\x00a0new\x00a0Date\x00a0(\x00a0)\x00a0]\x00a0/*\x00a0comment\x00a0*/\x00a0}\x00a0";
            var reader = new JsonTextReader(new StreamReader(new SlowStream(json, new UTF8Encoding(false), 1)));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Boolean, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndConstructor, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Comment, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(reader.Read());
        }

        [Fact]
        public void ParseObjectWithNoEnd()
        {
            var json = "{hi:1, ";
            var reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.IsFalse(reader.Read());
        }

        [Fact]
        public void ParseEmptyArray()
        {
            var json = "[]";
            var reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
        }

        [Fact]
        public void ParseEmptyObject()
        {
            var json = "{}";
            var reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
        }

        [Fact]
        public void ParseEmptyConstructor()
        {
            var json = "new Date()";
            var reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndConstructor, reader.TokenType);
        }

        [Fact]
        public void ParseHexNumber()
        {
            var json = @"0x20";

            var reader = new JsonTextReader(new StringReader(json));

            reader.ReadAsDecimal();
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(32m, reader.Value);
        }

        [Fact]
        public void ParseNumbers()
        {
            var json = @"[0,1,2 , 3]";

            var reader = new JsonTextReader(new StringReader(json));

            reader.Read();
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            reader.Read();
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);

            reader.Read();
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);

            reader.Read();
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);

            reader.Read();
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);

            reader.Read();
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
        }

        [Fact]
        public void ParseLineFeedDelimitedConstructor()
        {
            var json = "new Date\n()";
            var reader = new JsonTextReader(new StringReader(json));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("Date", reader.Value);
            Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndConstructor, reader.TokenType);
        }

        [Fact]
        public void ParseNullStringConstructor()
        {
            var json = "new Date\0()";
            var reader = new JsonTextReader(new StringReader(json));
#if DEBUG
            reader.CharBuffer = new char[7];
#endif

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("Date", reader.Value);
            Assert.AreEqual(JsonToken.StartConstructor, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndConstructor, reader.TokenType);
        }

        [Fact]
        public void ParseOctalNumber()
        {
            var json = @"010";

            var reader = new JsonTextReader(new StringReader(json));

            reader.ReadAsDecimal();
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(8m, reader.Value);
        }

        [Fact]
        public void DateParseHandling()
        {
            var json = @"[""1970-01-01T00:00:00Z"",""\/Date(0)\/""]";

            var reader = new JsonTextReader(new StringReader(json));
            reader.DateParseHandling = Argon.DateParseHandling.DateTime;

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
            Assert.AreEqual(typeof(DateTime), reader.ValueType);
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
            Assert.AreEqual(typeof(DateTime), reader.ValueType);
            Assert.IsTrue(reader.Read());

            reader = new JsonTextReader(new StringReader(json));
            reader.DateParseHandling = Argon.DateParseHandling.DateTimeOffset;

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
            Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
            Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
            Assert.IsTrue(reader.Read());

            reader = new JsonTextReader(new StringReader(json));
            reader.DateParseHandling = Argon.DateParseHandling.None;

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(@"1970-01-01T00:00:00Z", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(@"/Date(0)/", reader.Value);
            Assert.AreEqual(typeof(string), reader.ValueType);
            Assert.IsTrue(reader.Read());

            reader = new JsonTextReader(new StringReader(json));
            reader.DateParseHandling = Argon.DateParseHandling.DateTime;

            Assert.IsTrue(reader.Read());
            reader.ReadAsDateTimeOffset();
            Assert.AreEqual(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
            Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
            reader.ReadAsDateTimeOffset();
            Assert.AreEqual(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero), reader.Value);
            Assert.AreEqual(typeof(DateTimeOffset), reader.ValueType);
            Assert.IsTrue(reader.Read());

            reader = new JsonTextReader(new StringReader(json));
            reader.DateParseHandling = Argon.DateParseHandling.DateTimeOffset;

            Assert.IsTrue(reader.Read());
            reader.ReadAsDateTime();
            Assert.AreEqual(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
            Assert.AreEqual(typeof(DateTime), reader.ValueType);
            reader.ReadAsDateTime();
            Assert.AreEqual(new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc), reader.Value);
            Assert.AreEqual(typeof(DateTime), reader.ValueType);
            Assert.IsTrue(reader.Read());
        }
    }
}
