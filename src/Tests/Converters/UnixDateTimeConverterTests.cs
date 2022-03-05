// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using TestObjects;

public class UnixDateTimeConverterTests : TestFixtureBase
{
    [Fact]
    public void SerializeDateTime()
    {
        var unixEpoch = UnixDateTimeConverter.UnixEpoch;

        var result = JsonConvert.SerializeObject(unixEpoch, new UnixDateTimeConverter());

        Assert.Equal("0", result);
    }

    [Fact]
    public void SerializeDateTimeNow()
    {
        var now = DateTime.Now;
        var nowSeconds = (long) (now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;

        var result = JsonConvert.SerializeObject(now, new UnixDateTimeConverter());

        Assert.Equal($"{nowSeconds}", result);
    }

    [Fact]
    public void SerializeInvalidDate()
    {
        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.SerializeObject(new DateTime(1964, 2, 7), new UnixDateTimeConverter()),
            "Cannot convert date value that is before Unix epoch of 00:00:00 UTC on 1 January 1970."
        );
    }

    [Fact]
    public void WriteJsonInvalidType()
    {
        var converter = new UnixDateTimeConverter();

        XUnitAssert.Throws<JsonSerializationException>(
            () => converter.WriteJson(new JTokenWriter(), new(), new()),
            "Expected date object value."
        );
    }

    [Fact]
    public void SerializeDateTimeOffset()
    {
        var now = new DateTimeOffset(2018, 1, 1, 16, 1, 16, TimeSpan.FromHours(-5));

        var result = JsonConvert.SerializeObject(now, new UnixDateTimeConverter());

        Assert.Equal("1514840476", result);
    }

    [Fact]
    public void SerializeNullableDateTimeClass()
    {
        var t = new NullableDateTimeTestClass
        {
            DateTimeField = null,
            DateTimeOffsetField = null
        };

        var converter = new UnixDateTimeConverter();

        var result = JsonConvert.SerializeObject(t, converter);

        Assert.Equal(@"{""PreField"":null,""DateTimeField"":null,""DateTimeOffsetField"":null,""PostField"":null}", result);

        t = new()
        {
            DateTimeField = new DateTime(2018, 1, 1, 21, 1, 16, DateTimeKind.Utc),
            DateTimeOffsetField = new DateTimeOffset(1970, 2, 1, 20, 6, 18, TimeSpan.Zero)
        };

        result = JsonConvert.SerializeObject(t, converter);
        Assert.Equal(@"{""PreField"":null,""DateTimeField"":1514840476,""DateTimeOffsetField"":2750778,""PostField"":null}", result);
    }

    [Fact]
    public void DeserializeNullToNonNullable()
    {
        XUnitAssert.Throws<Exception>(
            () => JsonConvert.DeserializeObject<DateTimeTestClass>(
                @"{""PreField"":""Pre"",""DateTimeField"":null,""DateTimeOffsetField"":null,""PostField"":""Post""}",
                new UnixDateTimeConverter()
            ),
            "Cannot convert null value to System.DateTime. Path 'DateTimeField', line 1, position 38."
        );
    }

    [Fact]
    public void DeserializeDateTimeOffset()
    {
        var converter = new UnixDateTimeConverter();
        var d = new DateTimeOffset(1970, 2, 1, 20, 6, 18, TimeSpan.Zero);

        var json = JsonConvert.SerializeObject(d, converter);

        var result = JsonConvert.DeserializeObject<DateTimeOffset>(json, converter);

        Assert.Equal(new(1970, 2, 1, 20, 6, 18, TimeSpan.Zero), result);
    }

    [Fact]
    public void DeserializeStringToDateTimeOffset()
    {
        var result = JsonConvert.DeserializeObject<DateTimeOffset>(@"""1514840476""", new UnixDateTimeConverter());

        Assert.Equal(new(2018, 1, 1, 21, 1, 16, TimeSpan.Zero), result);
    }

    [Fact]
    public void DeserializeInvalidStringToDateTimeOffset()
    {
        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<DateTimeOffset>(@"""PIE""", new UnixDateTimeConverter()),
            "Cannot convert invalid value to System.DateTimeOffset. Path '', line 1, position 5."
        );
    }

    [Fact]
    public void DeserializeIntegerToDateTime()
    {
        var result = JsonConvert.DeserializeObject<DateTime>("1514840476", new UnixDateTimeConverter());

        Assert.Equal(new(2018, 1, 1, 21, 1, 16, DateTimeKind.Utc), result);
    }

    [Fact]
    public void DeserializeNullToNullable()
    {
        var result = JsonConvert.DeserializeObject<DateTime?>("null", new UnixDateTimeConverter());

        Assert.Null(result);
    }

    [Fact]
    public void DeserializeInvalidValue()
    {
        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<DateTime>("-1", new UnixDateTimeConverter()),
            "Cannot convert value that is before Unix epoch of 00:00:00 UTC on 1 January 1970 to System.DateTime. Path '', line 1, position 2."
        );
    }

    [Fact]
    public void DeserializeInvalidValueType()
    {
        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<DateTime>("false", new UnixDateTimeConverter()),
            "Unexpected token parsing date. Expected Integer or String, got Boolean. Path '', line 1, position 5."
        );
    }

    [Fact]
    public void ConverterList()
    {
        var l1 = new UnixConverterList<object>
        {
            new DateTime(2018, 1, 1, 21, 1, 16, DateTimeKind.Utc),
            new DateTime(1970, 1, 1, 0, 0, 3, DateTimeKind.Utc)
        };

        var json = JsonConvert.SerializeObject(l1, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"[
  1514840476,
  3
]", json);

        var l2 = JsonConvert.DeserializeObject<UnixConverterList<object>>(json);
        Assert.NotNull(l2);

        Assert.Equal(new DateTime(2018, 1, 1, 21, 1, 16, DateTimeKind.Utc), l2[0]);
        Assert.Equal(new DateTime(1970, 1, 1, 0, 0, 3, DateTimeKind.Utc), l2[1]);
    }

    [Fact]
    public void ConverterDictionary()
    {
        var l1 = new UnixConverterDictionary<object>
        {
            {"First", new DateTime(1970, 1, 1, 0, 0, 3, DateTimeKind.Utc)},
            {"Second", new DateTime(2018, 1, 1, 21, 1, 16, DateTimeKind.Utc)}
        };

        var json = JsonConvert.SerializeObject(l1, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"{
  ""First"": 3,
  ""Second"": 1514840476
}", json);

        var l2 = JsonConvert.DeserializeObject<UnixConverterDictionary<object>>(json);
        Assert.NotNull(l2);

        Assert.Equal(new DateTime(1970, 1, 1, 0, 0, 3, DateTimeKind.Utc), l2["First"]);
        Assert.Equal(new DateTime(2018, 1, 1, 21, 1, 16, DateTimeKind.Utc), l2["Second"]);
    }

    [Fact]
    public void ConverterObject()
    {
        var obj1 = new UnixConverterObject
        {
            Object1 = new DateTime(1970, 1, 1, 0, 0, 3, DateTimeKind.Utc),
            Object2 = null,
            ObjectNotHandled = new DateTime(2018, 1, 1, 21, 1, 16, DateTimeKind.Utc)
        };

        var json = JsonConvert.SerializeObject(obj1, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"{
  ""Object1"": 3,
  ""Object2"": null,
  ""ObjectNotHandled"": 1514840476
}", json);

        var obj2 = JsonConvert.DeserializeObject<UnixConverterObject>(json);
        Assert.NotNull(obj2);

        Assert.Equal(new DateTime(1970, 1, 1, 0, 0, 3, DateTimeKind.Utc), obj2.Object1);
        Assert.Null(obj2.Object2);
        Assert.Equal(new DateTime(2018, 1, 1, 21, 1, 16, DateTimeKind.Utc), obj2.ObjectNotHandled);
    }
}

[JsonArray(ItemConverterType = typeof(UnixDateTimeConverter))]
public class UnixConverterList<T> : List<T>
{
}

[JsonDictionary(ItemConverterType = typeof(UnixDateTimeConverter))]
public class UnixConverterDictionary<T> : Dictionary<string, T>
{
}

[JsonObject(ItemConverterType = typeof(UnixDateTimeConverter))]
public class UnixConverterObject
{
    public object Object1 { get; set; }

    public object Object2 { get; set; }

    [JsonConverter(typeof(UnixDateTimeConverter))]
    public object ObjectNotHandled { get; set; }
}