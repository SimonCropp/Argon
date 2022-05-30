// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

#if NET6_0_OR_GREATER

public class DateOnlyTests : TestFixtureBase
{
    [Fact]
    public void Serialize()
    {
        var d = new DateOnly(2000, 12, 29);
        var json = JsonConvert.SerializeObject(d, Formatting.Indented);

        Assert.Equal(@"""2000-12-29""", json);
    }

    [Fact]
    public void SerializeDefault()
    {
        DateOnly d = default;
        var json = JsonConvert.SerializeObject(d, Formatting.Indented);

        Assert.Equal(@"""0001-01-01""", json);
    }

    [Fact]
    public void SerializeMaxValue()
    {
        var d = DateOnly.MaxValue;
        var json = JsonConvert.SerializeObject(d, Formatting.Indented);

        Assert.Equal(@"""9999-12-31""", json);
    }

    [Fact]
    public void SerializeMinValue()
    {
        var d = DateOnly.MinValue;
        var json = JsonConvert.SerializeObject(d, Formatting.Indented);

        Assert.Equal(@"""0001-01-01""", json);
    }

    [Fact]
    public void SerializeNullable_Null()
    {
        DateOnly? d = default;
        var json = JsonConvert.SerializeObject(d, Formatting.Indented);

        Assert.Equal("null", json);
    }

    [Fact]
    public void SerializeNullable_Value()
    {
        DateOnly? d = new DateOnly(2000, 12, 29);
        var json = JsonConvert.SerializeObject(d, Formatting.Indented);

        Assert.Equal(@"""2000-12-29""", json);
    }

    [Fact]
    public Task SerializeList()
    {
        var d = new List<DateOnly>
        {
            new(2000, 12, 29)
        };
        var json = JsonConvert.SerializeObject(d, Formatting.Indented);

        return VerifyJson(json);
    }

    [Fact]
    public Task SerializeList_Nullable()
    {
        var d = new List<DateOnly?>
        {
            new DateOnly(2000, 12, 29),
            null
        };
        var json = JsonConvert.SerializeObject(d, Formatting.Indented);

        return VerifyJson(json);
    }

    [Fact]
    public void Deserialize()
    {
        var d = JsonConvert.DeserializeObject<DateOnly>(@"""2000-12-29""");

        Assert.Equal(new(2000, 12, 29), d);
    }

    [Fact]
    public void DeserializeDefault()
    {
        var d = JsonConvert.DeserializeObject<DateOnly>(@"""0001-01-01""");

        Assert.Equal(default(DateOnly), d);
    }

    [Fact]
    public void DeserializeMaxValue()
    {
        var d = JsonConvert.DeserializeObject<DateOnly>(@"""9999-12-31""");

        Assert.Equal(DateOnly.MaxValue, d);
    }

    [Fact]
    public void DeserializeMinValue()
    {
        var d = JsonConvert.DeserializeObject<DateOnly>(@"""0001-01-01""");

        Assert.Equal(DateOnly.MinValue, d);
    }

    [Fact]
    public void DeserializeNullable_Null()
    {
        var d = JsonConvert.DeserializeObject<DateOnly?>(@"null");

        Assert.Equal(null, d);
    }

    [Fact]
    public void DeserializeNullable_Value()
    {
        var d = JsonConvert.DeserializeObject<DateOnly?>(@"""2000-12-29""");

        Assert.Equal(new DateOnly(2000, 12, 29), d);
    }

    [Fact]
    public void DeserializeList()
    {
        var l = JsonConvert.DeserializeObject<IList<DateOnly>>(@"[
""2000-12-29""
]");

        Assert.Equal(1, l.Count);
        Assert.Equal(new(2000, 12, 29), l[0]);
    }

    [Fact]
    public void DeserializeList_Nullable()
    {
        var l = JsonConvert.DeserializeObject<IList<DateOnly?>>(@"[
""2000-12-29"",
null
]");

        Assert.Equal(2, l.Count);
        Assert.Equal(new DateOnly(2000, 12, 29), l[0]);
        Assert.Equal(null, l[1]);
    }
}
#endif
