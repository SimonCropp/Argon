// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using TestObjects;

public class JavaScriptDateTimeConverterTests : TestFixtureBase
{
    [Fact]
    public void SerializeDateTime()
    {
        var converter = new JavaScriptDateTimeConverter();

        var d = new DateTime(2000, 12, 15, 22, 11, 3, 55, DateTimeKind.Utc);

        var result = JsonConvert.SerializeObject(d, converter);
        Assert.Equal("new Date(976918263055)", result);
    }

    [Fact]
    public void SerializeDateTimeOffset()
    {
        var converter = new JavaScriptDateTimeConverter();

        var now = new DateTimeOffset(2000, 12, 15, 22, 11, 3, 55, TimeSpan.Zero);

        var result = JsonConvert.SerializeObject(now, converter);
        Assert.Equal("new Date(976918263055)", result);
    }

    [Fact]
    public void SerializeNullableDateTimeClass()
    {
        var t = new NullableDateTimeTestClass
        {
            DateTimeField = null,
            DateTimeOffsetField = null
        };

        var converter = new JavaScriptDateTimeConverter();

        var result = JsonConvert.SerializeObject(t, converter);
        Assert.Equal(@"{""PreField"":null,""DateTimeField"":null,""DateTimeOffsetField"":null,""PostField"":null}", result);

        t = new NullableDateTimeTestClass
        {
            DateTimeField = new DateTime(2000, 12, 15, 22, 11, 3, 55, DateTimeKind.Utc),
            DateTimeOffsetField = new DateTimeOffset(2000, 12, 15, 22, 11, 3, 55, TimeSpan.Zero)
        };

        result = JsonConvert.SerializeObject(t, converter);
        Assert.Equal(@"{""PreField"":null,""DateTimeField"":new Date(976918263055),""DateTimeOffsetField"":new Date(976918263055),""PostField"":null}", result);
    }

    [Fact]
    public void DeserializeNullToNonNullable()
    {
        XUnitAssert.Throws<Exception>(
            () =>
            {
                var c2 = JsonConvert.DeserializeObject<DateTimeTestClass>(@"{""PreField"":""Pre"",""DateTimeField"":null,""DateTimeOffsetField"":null,""PostField"":""Post""}", new JavaScriptDateTimeConverter());
            },
            "Cannot convert null value to System.DateTime. Path 'DateTimeField', line 1, position 38.");
    }

    [Fact]
    public void DeserializeDateTimeOffset()
    {
        var converter = new JavaScriptDateTimeConverter();
        var start = new DateTimeOffset(2000, 12, 15, 22, 11, 3, 55, TimeSpan.Zero);

        var json = JsonConvert.SerializeObject(start, converter);

        var result = JsonConvert.DeserializeObject<DateTimeOffset>(json, converter);
        Assert.Equal(new DateTimeOffset(2000, 12, 15, 22, 11, 3, 55, TimeSpan.Zero), result);
    }

    [Fact]
    public void DeserializeDateTime()
    {
        var converter = new JavaScriptDateTimeConverter();

        var result = JsonConvert.DeserializeObject<DateTime>("new Date(976918263055)", converter);
        Assert.Equal(new DateTime(2000, 12, 15, 22, 11, 3, 55, DateTimeKind.Utc), result);
    }

    [Fact]
    public void DeserializeDateTime_MultipleArguments()
    {
        var converter = new JavaScriptDateTimeConverter();

        var result = JsonConvert.DeserializeObject<DateTime>("new Date(2000, 11)", converter);
        Assert.Equal(new DateTime(2000, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), result);

        result = JsonConvert.DeserializeObject<DateTime>("new Date(2000, 11, 12)", converter);
        Assert.Equal(new DateTime(2000, 12, 12, 0, 0, 0, 0, DateTimeKind.Utc), result);

        result = JsonConvert.DeserializeObject<DateTime>("new Date(2000, 11, 12, 20)", converter);
        Assert.Equal(new DateTime(2000, 12, 12, 20, 0, 0, 0, DateTimeKind.Utc), result);

        result = JsonConvert.DeserializeObject<DateTime>("new Date(2000, 11, 12, 20, 1)", converter);
        Assert.Equal(new DateTime(2000, 12, 12, 20, 1, 0, 0, DateTimeKind.Utc), result);

        result = JsonConvert.DeserializeObject<DateTime>("new Date(2000, 11, 12, 20, 1, 2)", converter);
        Assert.Equal(new DateTime(2000, 12, 12, 20, 1, 2, 0, DateTimeKind.Utc), result);

        result = JsonConvert.DeserializeObject<DateTime>("new Date(2000, 11, 12, 20, 1, 2, 3)", converter);
        Assert.Equal(new DateTime(2000, 12, 12, 20, 1, 2, 3, DateTimeKind.Utc), result);

        result = JsonConvert.DeserializeObject<DateTime>("new Date(2000, 11, 1, 0, 0, 0, 0)", converter);
        Assert.Equal(new DateTime(2000, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), result);
    }

    [Fact]
    public void DeserializeDateTime_TooManyArguments()
    {
        var converter = new JavaScriptDateTimeConverter();

        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<DateTime>("new Date(1, 2, 3, 4, 5, 6, 7, 8)", converter),
            "Unexpected number of arguments when reading date constructor. Path '', line 1, position 32.");
    }

    [Fact]
    public void DeserializeDateTime_NoArguments()
    {
        var converter = new JavaScriptDateTimeConverter();

        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<DateTime>("new Date()", converter),
            "Date constructor has no arguments. Path '', line 1, position 10.");
    }

    [Fact]
    public void DeserializeDateTime_NotArgumentsNotClosed()
    {
        var converter = new JavaScriptDateTimeConverter();

        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<DateTime>("new Date(", converter),
            "Unexpected end when reading date constructor. Path '', line 1, position 9.");
    }

    [Fact]
    public void DeserializeDateTime_NotClosed()
    {
        var converter = new JavaScriptDateTimeConverter();

        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<DateTime>("new Date(2, 3", converter),
            "Unexpected end when reading date constructor. Path '[1]', line 1, position 13.");
    }

    [Fact]
    public void ConverterList()
    {
        var l1 = new ConverterList<object>
        {
            new DateTime(2000, 12, 12, 20, 10, 0, DateTimeKind.Utc),
            new DateTime(1983, 10, 9, 23, 10, 0, DateTimeKind.Utc)
        };

        var json = JsonConvert.SerializeObject(l1, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"[
  new Date(
    976651800000
  ),
  new Date(
    434589000000
  )
]", json);

        var l2 = JsonConvert.DeserializeObject<ConverterList<object>>(json);
        Assert.NotNull(l2);

        Assert.Equal(new DateTime(2000, 12, 12, 20, 10, 0, DateTimeKind.Utc), l2[0]);
        Assert.Equal(new DateTime(1983, 10, 9, 23, 10, 0, DateTimeKind.Utc), l2[1]);
    }

    [Fact]
    public void ConverterDictionary()
    {
        // ReSharper disable once UseObjectOrCollectionInitializer
        var l1 = new ConverterDictionary<object>();
        l1.Add("First", new DateTime(2000, 12, 12, 20, 10, 0, DateTimeKind.Utc));
        l1.Add("Second", new DateTime(1983, 10, 9, 23, 10, 0, DateTimeKind.Utc));

        var json = JsonConvert.SerializeObject(l1, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"{
  ""First"": new Date(
    976651800000
  ),
  ""Second"": new Date(
    434589000000
  )
}", json);

        var l2 = JsonConvert.DeserializeObject<ConverterDictionary<object>>(json);
        Assert.NotNull(l2);

        Assert.Equal(new DateTime(2000, 12, 12, 20, 10, 0, DateTimeKind.Utc), l2["First"]);
        Assert.Equal(new DateTime(1983, 10, 9, 23, 10, 0, DateTimeKind.Utc), l2["Second"]);
    }

    [Fact]
    public void ConverterObject()
    {
        var l1 = new ConverterObject
        {
            Object1 = new DateTime(2000, 12, 12, 20, 10, 0, DateTimeKind.Utc),
            Object2 = null,
            ObjectNotHandled = new DateTime(2000, 12, 12, 20, 10, 0, DateTimeKind.Utc)
        };

        var json = JsonConvert.SerializeObject(l1, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"{
  ""Object1"": new Date(
    976651800000
  ),
  ""Object2"": null,
  ""ObjectNotHandled"": 631122486000000000
}", json);

        var l2 = JsonConvert.DeserializeObject<ConverterObject>(json);
        Assert.NotNull(l2);

        //Assert.AreEqual(new DateTime(2000, 12, 12, 20, 10, 0, DateTimeKind.Utc), l2["First"]);
        //Assert.AreEqual(new DateTime(1983, 10, 9, 23, 10, 0, DateTimeKind.Utc), l2["Second"]);
    }
}

[JsonArray(ItemConverterType = typeof(JavaScriptDateTimeConverter))]
public class ConverterList<T> : List<T>
{
}

[JsonDictionary(ItemConverterType = typeof(JavaScriptDateTimeConverter))]
public class ConverterDictionary<T> : Dictionary<string, T>
{
}

[JsonObject(ItemConverterType = typeof(JavaScriptDateTimeConverter))]
public class ConverterObject
{
    public object Object1 { get; set; }
    public object Object2 { get; set; }

    [JsonConverter(typeof(DateIntConverter))]
    public object ObjectNotHandled { get; set; }
}

public class DateIntConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var d = (DateTime?)value;
        if (d == null)
        {
            writer.WriteNull();
        }
        else
        {
            writer.WriteValue(d.Value.Ticks);
        }
    }

    public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
    {
        return new DateTime(Convert.ToInt64(reader.Value), DateTimeKind.Utc);
    }

    public override bool CanConvert(Type type)
    {
        return type == typeof(DateTime) || type == typeof(DateTime?);
    }
}