// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class Issue2768 : TestFixtureBase
{
    [Fact]
    public void Test_Serialize()
    {
        var d = 0.0m;
        var json = JsonConvert.SerializeObject(d);

        Assert.Equal("0.0", json);
    }

    [Fact]
    public void Test_Serialize_NoTrailingZero()
    {
        var d = 0m;
        var json = JsonConvert.SerializeObject(d);

        Assert.Equal("0.0", json);
    }

    [Fact]
    public void Test_Deserialize()
    {
        var d = JsonConvert.DeserializeObject<decimal>("0.0");

        Assert.Equal("0.0", d.ToString());
    }

    [Fact]
    public void Test_Deserialize_Negative()
    {
        decimal d = JsonConvert.DeserializeObject<decimal>("-0.0");

        Assert.Equal("0.0", d.ToString());
    }

    [Fact]
    public void Test_Deserialize_NegativeNoTrailingZero()
    {
        var d = JsonConvert.DeserializeObject<decimal>("-0");

        Assert.Equal("0", d.ToString());
    }

    [Fact]
    public void Test_Deserialize_MultipleTrailingZeroes()
    {
        var d = JsonConvert.DeserializeObject<decimal>("0.00");

        Assert.Equal("0.00", d.ToString());
    }

    [Fact]
    public void Test_Deserialize_NoTrailingZero()
    {
        var d = JsonConvert.DeserializeObject<decimal>("0");

        Assert.Equal("0", d.ToString());
    }

    [Fact]
    public void ParseJsonDecimal()
    {
        var json = @"{ ""property"": 0.0 }";

        var reader = new JsonTextReader(new StringReader(json))
        {
            FloatParseHandling = FloatParseHandling.Decimal
        };

        decimal? parsedValue = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.Float)
            {
                parsedValue = (decimal) reader.Value;
                break;
            }
        }

        Assert.Equal("0.0", parsedValue.ToString());
    }

    [Fact]
    public void ParseJsonDecimal_IsBoxedInstanceSame()
    {
        var json = @"[ 0.0, 0.0 ]";

        var reader = new JsonTextReader(new StringReader(json))
        {
            FloatParseHandling = FloatParseHandling.Decimal
        };

        var boxedDecimals = new List<object>();

        // Start array
        Assert.True(reader.Read());

        Assert.True(reader.Read());
        boxedDecimals.Add(reader.Value);

        Assert.True(reader.Read());
        boxedDecimals.Add(reader.Value);

        Assert.True(reader.Read());
        Assert.False(reader.Read());

        // Boxed values will match or not depending on whether framework supports.
#if NET6_0_OR_GREATER
        Assert.True(ReferenceEquals(boxedDecimals[0], boxedDecimals[1]));
#else
        Assert.False(ReferenceEquals(boxedDecimals[0], boxedDecimals[1]));
#endif
    }

    [Fact]
    public void Test_Deserialize_Double_Negative()
    {
        var d = JsonConvert.DeserializeObject<double>("-0.0");

#if NETCOREAPP3_1_OR_GREATER
        Assert.Equal("-0", d.ToString());
#else
        Assert.Equal("0", d.ToString());
#endif
    }

    [Fact]
    public void Test_Deserialize_Double_NegativeNoTrailingZero()
    {
        var d = JsonConvert.DeserializeObject<double>("-0");

#if NETCOREAPP3_1_OR_GREATER
        Assert.Equal("-0", d.ToString());
#else
        Assert.Equal("0", d.ToString());
#endif
    }

    [Fact]
    public void JValueDouble_ToString()
    {
        var d = new JValue(-0.0d);

#if NETCOREAPP3_1_OR_GREATER
        Assert.Equal("-0", d.ToString());
#else
        Assert.Equal("0", d.ToString());
#endif
    }
}