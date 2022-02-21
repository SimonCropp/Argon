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

public class ConvertUtilsTests : TestFixtureBase
{
    static void AssertDecimalTryParse(string s, ParseResult expectedResult, decimal? expectedValue)
    {
        var c = s.ToCharArray();
        var result = ConvertUtils.DecimalTryParse(c, 0, c.Length, out var d);

        var result2 = decimal.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d2)
                      && !s.StartsWith(".")
                      && !s.EndsWith(".")
                      && !(s.StartsWith("0") && s.Length > 1 && !s.StartsWith("0.") && !s.StartsWith("0e", StringComparison.OrdinalIgnoreCase))
                      && !(s.StartsWith("-0") && s.Length > 2 && !s.StartsWith("-0.") && !s.StartsWith("-0e", StringComparison.OrdinalIgnoreCase))
                      && s.IndexOf(".e", StringComparison.OrdinalIgnoreCase) == -1;

        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedResult == ParseResult.Success, result2);

        if (result2)
        {
            Assert.True(expectedValue.HasValue);

            Assert.Equal(expectedValue.Value, d);

            Assert.Equal(expectedValue.Value, d2);

            Assert.Equal(expectedValue.Value.ToString(), d.ToString());
        }
    }

    [Fact]
    public void DecimalTryParse()
    {
        AssertDecimalTryParse("0", ParseResult.Success, 0M);
        AssertDecimalTryParse("-0", ParseResult.Success, 0M);
        AssertDecimalTryParse("1", ParseResult.Success, 1M);
        AssertDecimalTryParse("-1", ParseResult.Success, -1M);
        AssertDecimalTryParse("1E1", ParseResult.Success, 10M);
        AssertDecimalTryParse("1E28", ParseResult.Success, 10000000000000000000000000000M);
        AssertDecimalTryParse("1.0", ParseResult.Success, 1.0M);
        AssertDecimalTryParse("1.10000", ParseResult.Success, 1.10000M);
        AssertDecimalTryParse("1000.000000000000", ParseResult.Success, 1000.000000000000M);
        AssertDecimalTryParse("87.50", ParseResult.Success, 87.50M);

        AssertDecimalTryParse("1.2345678901234567890123456789", ParseResult.Success, 1.2345678901234567890123456789M);
        AssertDecimalTryParse("1.0000000000000000000000000001", ParseResult.Success, 1.0000000000000000000000000001M);
        AssertDecimalTryParse("-1.0000000000000000000000000001", ParseResult.Success, -1.0000000000000000000000000001M);

        AssertDecimalTryParse(decimal.MaxValue.ToString(CultureInfo.InvariantCulture), ParseResult.Success, decimal.MaxValue);
        AssertDecimalTryParse(decimal.MinValue.ToString(CultureInfo.InvariantCulture), ParseResult.Success, decimal.MinValue);

        AssertDecimalTryParse("12345678901234567890123456789", ParseResult.Success, 12345678901234567890123456789M);
        AssertDecimalTryParse("12345678901234567890123456789.4", ParseResult.Success, 12345678901234567890123456789M);
        AssertDecimalTryParse("12345678901234567890123456789.5", ParseResult.Success, 12345678901234567890123456790M);
        AssertDecimalTryParse("-12345678901234567890123456789", ParseResult.Success, -12345678901234567890123456789M);
        AssertDecimalTryParse("-12345678901234567890123456789.4", ParseResult.Success, -12345678901234567890123456789M);
        AssertDecimalTryParse("-12345678901234567890123456789.5", ParseResult.Success, -12345678901234567890123456790M);

        AssertDecimalTryParse("1.2345678901234567890123456789e-25", ParseResult.Success, 0.0000000000000000000000001235M);
        AssertDecimalTryParse("1.2345678901234567890123456789e-26", ParseResult.Success, 0.0000000000000000000000000123M);
        AssertDecimalTryParse("1.2345678901234567890123456789e-28", ParseResult.Success, 0.0000000000000000000000000001M);
        AssertDecimalTryParse("1.2345678901234567890123456789e-29", ParseResult.Success, 0M);

        AssertDecimalTryParse("1E-999", ParseResult.Success, 0M);

        for (decimal i = -100; i < 100; i += 0.1m)
        {
            AssertDecimalTryParse(i.ToString(CultureInfo.InvariantCulture), ParseResult.Success, i);
        }

        AssertDecimalTryParse("1E+29", ParseResult.Overflow, null);
        AssertDecimalTryParse("-1E+29", ParseResult.Overflow, null);
        AssertDecimalTryParse("79228162514264337593543950336", ParseResult.Overflow, null); // decimal.MaxValue + 1
        AssertDecimalTryParse("-79228162514264337593543950336", ParseResult.Overflow, null); // decimal.MinValue - 1

        AssertDecimalTryParse("1-1", ParseResult.Invalid, null);
        AssertDecimalTryParse("1-", ParseResult.Invalid, null);
        AssertDecimalTryParse("--1", ParseResult.Invalid, null);
        AssertDecimalTryParse("-", ParseResult.Invalid, null);
        AssertDecimalTryParse(".", ParseResult.Invalid, null);
        AssertDecimalTryParse("01E28", ParseResult.Invalid, null);
        AssertDecimalTryParse("-01E28", ParseResult.Invalid, null);
        AssertDecimalTryParse("1.", ParseResult.Invalid, null);
        AssertDecimalTryParse("0.", ParseResult.Invalid, null);
        AssertDecimalTryParse(".1E23", ParseResult.Invalid, null);
        AssertDecimalTryParse("1..1E23", ParseResult.Invalid, null);
        AssertDecimalTryParse("1.E23", ParseResult.Invalid, null);
        AssertDecimalTryParse("1E2.3", ParseResult.Invalid, null);
        AssertDecimalTryParse("1EE-10", ParseResult.Invalid, null);
        AssertDecimalTryParse("1E-1-0", ParseResult.Invalid, null);
        AssertDecimalTryParse("1-E10", ParseResult.Invalid, null);
        AssertDecimalTryParse("", ParseResult.Invalid, null);
        AssertDecimalTryParse("5.1231231E", ParseResult.Invalid, null);
        AssertDecimalTryParse("1E+23i", ParseResult.Invalid, null);
        AssertDecimalTryParse("1EE+23", ParseResult.Invalid, null);
        AssertDecimalTryParse("1E++23", ParseResult.Invalid, null);
        AssertDecimalTryParse("1E--23", ParseResult.Invalid, null);
        AssertDecimalTryParse("E23", ParseResult.Invalid, null);
        AssertDecimalTryParse("00", ParseResult.Invalid, null);
    }

    [Fact]
    public void Int64TryParse()
    {
        var c = "43443333222211111117".ToCharArray();
        var result = ConvertUtils.Int64TryParse(c, 0, c.Length, out var l);
        Assert.Equal(ParseResult.Overflow, result);

        c = "9223372036854775807".ToCharArray();
        result = ConvertUtils.Int64TryParse(c, 0, c.Length, out l);
        Assert.Equal(ParseResult.Success, result);
        Assert.Equal(9223372036854775807L, l);

        c = "9223372036854775808".ToCharArray();
        result = ConvertUtils.Int64TryParse(c, 0, c.Length, out l);
        Assert.Equal(ParseResult.Overflow, result);

        for (var i = 3; i < 10; i++)
        {
            c = ("9" + i + "23372036854775807").ToCharArray();
            result = ConvertUtils.Int64TryParse(c, 0, c.Length, out l);
            Assert.Equal(ParseResult.Overflow, result);
        }

        c = "-9223372036854775808".ToCharArray();
        result = ConvertUtils.Int64TryParse(c, 0, c.Length, out l);
        Assert.Equal(ParseResult.Success, result);
        Assert.Equal(-9223372036854775808L, l);

        c = "-9223372036854775809".ToCharArray();
        result = ConvertUtils.Int64TryParse(c, 0, c.Length, out l);
        Assert.Equal(ParseResult.Overflow, result);

        for (var i = 3; i < 10; i++)
        {
            c = ("-9" + i + "23372036854775808").ToCharArray();
            result = ConvertUtils.Int64TryParse(c, 0, c.Length, out l);
            Assert.Equal(ParseResult.Overflow, result);
        }
    }

    [Fact]
    public void Int32TryParse()
    {
        var c = "43443333227".ToCharArray();
        var result = ConvertUtils.Int32TryParse(c, 0, c.Length, out var i);
        Assert.Equal(ParseResult.Overflow, result);

        c = "2147483647".ToCharArray();
        result = ConvertUtils.Int32TryParse(c, 0, c.Length, out i);
        Assert.Equal(ParseResult.Success, result);
        Assert.Equal(2147483647, i);

        c = "2147483648".ToCharArray();
        result = ConvertUtils.Int32TryParse(c, 0, c.Length, out i);
        Assert.Equal(ParseResult.Overflow, result);

        c = "-2147483648".ToCharArray();
        result = ConvertUtils.Int32TryParse(c, 0, c.Length, out i);
        Assert.Equal(ParseResult.Success, result);
        Assert.Equal(-2147483648, i);

        c = "-2147483649".ToCharArray();
        result = ConvertUtils.Int32TryParse(c, 0, c.Length, out i);
        Assert.Equal(ParseResult.Overflow, result);

        for (var j = 2; j < 10; j++)
        {
            for (var k = 2; k < 10; k++)
            {
                var t = $"{j.ToString(CultureInfo.InvariantCulture)}{k.ToString(CultureInfo.InvariantCulture)}47483647";

                c = t.ToCharArray();
                result = ConvertUtils.Int32TryParse(c, 0, c.Length, out i);

                Assert.Equal(ParseResult.Overflow, result);
            }
        }

        for (var j = 2; j < 10; j++)
        {
            for (var k = 2; k < 10; k++)
            {
                var t = $"-{j.ToString(CultureInfo.InvariantCulture)}{k.ToString(CultureInfo.InvariantCulture)}47483648";

                c = t.ToCharArray();
                result = ConvertUtils.Int32TryParse(c, 0, c.Length, out i);

                Assert.Equal(ParseResult.Overflow, result);
            }
        }
    }

    [Fact]
    public void HexParse()
    {
        HexParseSame("0000");
        HexParseSame("1234");
        HexParseSame("4321");
        HexParseSame("abcd");
        HexParseSame("dcba");
        HexParseSame("ffff");
        HexParseSame("ABCD");
        HexParseSame("DCBA");
        HexParseSame("FFFF");
    }

    [Fact]
    public void HexParseOffset()
    {
        Assert.True(ConvertUtils.TryHexTextToInt("!0000".ToCharArray(), 1, 5, out var value));
        Assert.Equal(0, value);
    }

    [Fact]
    public void HexParseError()
    {
        Assert.False(ConvertUtils.TryHexTextToInt("-100".ToCharArray(), 0, 4, out var value));
        Assert.False(ConvertUtils.TryHexTextToInt("000g".ToCharArray(), 0, 4, out value));
        Assert.False(ConvertUtils.TryHexTextToInt(" ssd".ToCharArray(), 0, 4, out value));
        Assert.False(ConvertUtils.TryHexTextToInt("000:".ToCharArray(), 0, 4, out value));
        Assert.False(ConvertUtils.TryHexTextToInt("000G".ToCharArray(), 0, 4, out value));
    }

    static void HexParseSame(string text)
    {
        var v1 = int.Parse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

        Assert.True(ConvertUtils.TryHexTextToInt(text.ToCharArray(), 0, 4, out var v2));

        Assert.Equal(v1, v2);
    }
}