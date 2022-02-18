﻿#region License
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

namespace Argon.Tests.JsonTextReaderTests;

public class FloatTests : TestFixtureBase
{
    [Fact]
    public void Float_ReadAsString_Exact()
    {
        const string testJson = "{float: 0.0620}";

        var reader = new JsonTextReader(new StringReader(testJson));
        Assert.True(reader.Read());
        Assert.True(reader.Read());

        var s = reader.ReadAsString();
        Assert.Equal("0.0620", s);
    }

    [Fact]
    public void Float_NaN_Read()
    {
        const string testJson = "{float: NaN}";

        var reader = new JsonTextReader(new StringReader(testJson));

        Assert.True(reader.Read());
        Assert.True(reader.Read());
        Assert.True(reader.Read());

        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(double.NaN, reader.Value);

        Assert.True(reader.Read());
        Assert.False(reader.Read());
    }

    [Fact]
    public void Float_NaN_ReadAsInt32()
    {
        const string testJson = "{float: NaN}";

        var reader = new JsonTextReader(new StringReader(testJson));

        Assert.True(reader.Read());
        Assert.True(reader.Read());

        XUnitAssert.Throws<JsonReaderException>(() => reader.ReadAsInt32(), "Cannot read NaN value. Path 'float', line 1, position 11.");
    }

    [Fact]
    public void Float_NaNAndInifinity_ReadAsDouble()
    {
        const string testJson = @"[
  NaN,
  Infinity,
  -Infinity
]"; ;

        var reader = new JsonTextReader(new StringReader(testJson));

        Assert.True(reader.Read());

        Assert.Equal(double.NaN, reader.ReadAsDouble());
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(double.NaN, reader.Value);

        Assert.Equal(double.PositiveInfinity, reader.ReadAsDouble());
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(double.PositiveInfinity, reader.Value);

        Assert.Equal(double.NegativeInfinity, reader.ReadAsDouble());
        Assert.Equal(JsonToken.Float, reader.TokenType);
        Assert.Equal(double.NegativeInfinity, reader.Value);

        Assert.True(reader.Read());
        Assert.False(reader.Read());
    }

    [Fact]
    public void Float_NaNAndInifinity_ReadAsString()
    {
        const string testJson = @"[
  NaN,
  Infinity,
  -Infinity
]"; ;

        var reader = new JsonTextReader(new StringReader(testJson));

        Assert.True(reader.Read());

        Assert.Equal(JsonConvert.NaN, reader.ReadAsString());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal(JsonConvert.NaN, reader.Value);

        Assert.Equal(JsonConvert.PositiveInfinity, reader.ReadAsString());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal(JsonConvert.PositiveInfinity, reader.Value);

        Assert.Equal(JsonConvert.NegativeInfinity, reader.ReadAsString());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal(JsonConvert.NegativeInfinity, reader.Value);

        Assert.True(reader.Read());
        Assert.False(reader.Read());
    }

    [Fact]
    public void FloatParseHandling_ReadAsString()
    {
        var json = "[9223372036854775807, 1.7976931348623157E+308, 792281625142643375935439503.35, 792281625142643375935555555555555555555555555555555555555555555555555439503.35]";

        var reader = new JsonTextReader(new StringReader(json));
        reader.FloatParseHandling = Argon.FloatParseHandling.Decimal;

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Assert.Equal("9223372036854775807", reader.ReadAsString());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("9223372036854775807", reader.Value);

        Assert.Equal("1.7976931348623157E+308", reader.ReadAsString());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("1.7976931348623157E+308", reader.Value);

        Assert.Equal("792281625142643375935439503.35", reader.ReadAsString());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("792281625142643375935439503.35", reader.Value);

        Assert.Equal("792281625142643375935555555555555555555555555555555555555555555555555439503.35", reader.ReadAsString());
        Assert.Equal(JsonToken.String, reader.TokenType);
        Assert.Equal("792281625142643375935555555555555555555555555555555555555555555555555439503.35", reader.Value);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);
    }

    [Fact]
    public void FloatParseHandling()
    {
        var json = "[1.0,1,9.9,1E-06]";

        var reader = new JsonTextReader(new StringReader(json));
        reader.FloatParseHandling = Argon.FloatParseHandling.Decimal;

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(1.0m, reader.Value);
        Assert.Equal(typeof(decimal), reader.ValueType);
        Assert.Equal(JsonToken.Float, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(1L, reader.Value);
        Assert.Equal(typeof(long), reader.ValueType);
        Assert.Equal(JsonToken.Integer, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(9.9m, reader.Value);
        Assert.Equal(typeof(decimal), reader.ValueType);
        Assert.Equal(JsonToken.Float, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(Convert.ToDecimal(1E-06), reader.Value);
        Assert.Equal(typeof(decimal), reader.ValueType);
        Assert.Equal(JsonToken.Float, reader.TokenType);

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.EndArray, reader.TokenType);
    }

    [Fact]
    public void FloatParseHandling_NaN()
    {
        var json = "[NaN]";

        var reader = new JsonTextReader(new StringReader(json));
        reader.FloatParseHandling = Argon.FloatParseHandling.Decimal;

        Assert.True(reader.Read());
        Assert.Equal(JsonToken.StartArray, reader.TokenType);

        XUnitAssert.Throws<JsonReaderException>(() => reader.Read(), "Cannot read NaN value. Path '', line 1, position 4.");
    }

    [Fact]
    public void FloatingPointNonFiniteNumbers()
    {
        var input = @"[
  NaN,
  Infinity,
  -Infinity
]";

        var sr = new StringReader(input);

        using (JsonReader jsonReader = new JsonTextReader(sr))
        {
            jsonReader.Read();
            Assert.Equal(jsonReader.TokenType, JsonToken.StartArray);

            jsonReader.Read();
            Assert.Equal(jsonReader.TokenType, JsonToken.Float);
            Assert.Equal(jsonReader.Value, double.NaN);

            jsonReader.Read();
            Assert.Equal(jsonReader.TokenType, JsonToken.Float);
            Assert.Equal(jsonReader.Value, double.PositiveInfinity);

            jsonReader.Read();
            Assert.Equal(jsonReader.TokenType, JsonToken.Float);
            Assert.Equal(jsonReader.Value, double.NegativeInfinity);

            jsonReader.Read();
            Assert.Equal(jsonReader.TokenType, JsonToken.EndArray);
        }
    }

    [Fact]
    public void ReadFloatingPointNumber()
    {
        var json =
            @"[0.0,0.0,0.1,1.0,1.000001,1E-06,4.94065645841247E-324,Infinity,-Infinity,NaN,1.7976931348623157E+308,-1.7976931348623157E+308,Infinity,-Infinity,NaN,0e-10,0.25e-5,0.3e10]";

        using (JsonReader jsonReader = new JsonTextReader(new StringReader(json)))
        {
            jsonReader.Read();
            Assert.Equal(JsonToken.StartArray, jsonReader.TokenType);

            jsonReader.Read();
            Assert.Equal(JsonToken.Float, jsonReader.TokenType);
            Assert.Equal(0.0, jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.Float, jsonReader.TokenType);
            Assert.Equal(0.0, jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.Float, jsonReader.TokenType);
            Assert.Equal(0.1, jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.Float, jsonReader.TokenType);
            Assert.Equal(1.0, jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.Float, jsonReader.TokenType);
            Assert.Equal(1.000001, jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.Float, jsonReader.TokenType);
            Assert.Equal(1E-06, jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.Float, jsonReader.TokenType);
            Assert.Equal(4.94065645841247E-324, jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.Float, jsonReader.TokenType);
            Assert.Equal(double.PositiveInfinity, jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.Float, jsonReader.TokenType);
            Assert.Equal(double.NegativeInfinity, jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.Float, jsonReader.TokenType);
            Assert.Equal(double.NaN, jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.Float, jsonReader.TokenType);
            Assert.Equal(double.MaxValue, jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.Float, jsonReader.TokenType);
            Assert.Equal(double.MinValue, jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.Float, jsonReader.TokenType);
            Assert.Equal(double.PositiveInfinity, jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.Float, jsonReader.TokenType);
            Assert.Equal(double.NegativeInfinity, jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.Float, jsonReader.TokenType);
            Assert.Equal(double.NaN, jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.Float, jsonReader.TokenType);
            Assert.Equal(0d, jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.Float, jsonReader.TokenType);
            Assert.Equal(0.0000025d, jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.Float, jsonReader.TokenType);
            Assert.Equal(3000000000d, jsonReader.Value);

            jsonReader.Read();
            Assert.Equal(JsonToken.EndArray, jsonReader.TokenType);
        }
    }
}