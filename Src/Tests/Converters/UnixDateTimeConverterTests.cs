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
using Argon.Tests.TestObjects;

namespace Argon.Tests.Converters;

public class UnixDateTimeConverterTests : TestFixtureBase
{
    [Fact]
    public void SerializeDateTime()
    {
        var unixEpoch = UnixDateTimeConverter.UnixEpoch;

        var result = JsonConvert.SerializeObject(unixEpoch, new UnixDateTimeConverter());

        Assert.AreEqual("0", result);
    }

    [Fact]
    public void SerializeDateTimeNow()
    {
        var now = DateTime.Now;
        var nowSeconds = (long)(now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;

        var result = JsonConvert.SerializeObject(now, new UnixDateTimeConverter());

        Assert.AreEqual(nowSeconds + "", result);
    }

    [Fact]
    public void SerializeInvalidDate()
    {
        ExceptionAssert.Throws<JsonSerializationException>(
            () => JsonConvert.SerializeObject(new DateTime(1964, 2, 7), new UnixDateTimeConverter()),
            "Cannot convert date value that is before Unix epoch of 00:00:00 UTC on 1 January 1970."
        );
    }

    [Fact]
    public void WriteJsonInvalidType()
    {
        var converter = new UnixDateTimeConverter();

        ExceptionAssert.Throws<JsonSerializationException>(
            () => converter.WriteJson(new JTokenWriter(), new object(), new JsonSerializer()),
            "Expected date object value."
        );
    }

    [Fact]
    public void SerializeDateTimeOffset()
    {
        var now = new DateTimeOffset(2018, 1, 1, 16, 1, 16, TimeSpan.FromHours(-5));

        var result = JsonConvert.SerializeObject(now, new UnixDateTimeConverter());

        Assert.AreEqual("1514840476", result);
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

        Assert.AreEqual(@"{""PreField"":null,""DateTimeField"":null,""DateTimeOffsetField"":null,""PostField"":null}", result);

        t = new NullableDateTimeTestClass
        {
            DateTimeField = new DateTime(2018, 1, 1, 21, 1, 16, DateTimeKind.Utc),
            DateTimeOffsetField = new DateTimeOffset(1970, 2, 1, 20, 6, 18, TimeSpan.Zero)
        };

        result = JsonConvert.SerializeObject(t, converter);
        Assert.AreEqual(@"{""PreField"":null,""DateTimeField"":1514840476,""DateTimeOffsetField"":2750778,""PostField"":null}", result);
    }

    [Fact]
    public void DeserializeNullToNonNullable()
    {
        ExceptionAssert.Throws<Exception>(
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

        Assert.AreEqual(new DateTimeOffset(1970, 2, 1, 20, 6, 18, TimeSpan.Zero), result);
    }

    [Fact]
    public void DeserializeStringToDateTimeOffset()
    {
        var result = JsonConvert.DeserializeObject<DateTimeOffset>(@"""1514840476""", new UnixDateTimeConverter());

        Assert.AreEqual(new DateTimeOffset(2018, 1, 1, 21, 1, 16, TimeSpan.Zero), result);
    }

    [Fact]
    public void DeserializeInvalidStringToDateTimeOffset()
    {
        ExceptionAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<DateTimeOffset>(@"""PIE""", new UnixDateTimeConverter()),
            "Cannot convert invalid value to System.DateTimeOffset. Path '', line 1, position 5."
        );
    }

    [Fact]
    public void DeserializeIntegerToDateTime()
    {
        var result = JsonConvert.DeserializeObject<DateTime>("1514840476", new UnixDateTimeConverter());

        Assert.AreEqual(new DateTime(2018, 1, 1, 21, 1, 16, DateTimeKind.Utc), result);
    }

    [Fact]
    public void DeserializeNullToNullable()
    {
        var result = JsonConvert.DeserializeObject<DateTime?>("null", new UnixDateTimeConverter());

        Xunit.Assert.Null(result);
    }

    [Fact]
    public void DeserializeInvalidValue()
    {
        ExceptionAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<DateTime>("-1", new UnixDateTimeConverter()),
            "Cannot convert value that is before Unix epoch of 00:00:00 UTC on 1 January 1970 to System.DateTime. Path '', line 1, position 2."
        );
    }

    [Fact]
    public void DeserializeInvalidValueType()
    {
        ExceptionAssert.Throws<JsonSerializationException>(
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
            new DateTime(1970, 1, 1, 0, 0, 3, DateTimeKind.Utc),
        };

        var json = JsonConvert.SerializeObject(l1, Formatting.Indented);
        StringAssert.AreEqual(@"[
  1514840476,
  3
]", json);

        var l2 = JsonConvert.DeserializeObject<UnixConverterList<object>>(json);
        Xunit.Assert.NotNull(l2);

        Assert.AreEqual(new DateTime(2018, 1, 1, 21, 1, 16, DateTimeKind.Utc), l2[0]);
        Assert.AreEqual(new DateTime(1970, 1, 1, 0, 0, 3, DateTimeKind.Utc), l2[1]);
    }

    [Fact]
    public void ConverterDictionary()
    {
        var l1 = new UnixConverterDictionary<object>
        {
            {"First", new DateTime(1970, 1, 1, 0, 0, 3, DateTimeKind.Utc)},
            {"Second", new DateTime(2018, 1, 1, 21, 1, 16, DateTimeKind.Utc)},
        };

        var json = JsonConvert.SerializeObject(l1, Formatting.Indented);
        StringAssert.AreEqual(@"{
  ""First"": 3,
  ""Second"": 1514840476
}", json);

        var l2 = JsonConvert.DeserializeObject<UnixConverterDictionary<object>>(json);
        Xunit.Assert.NotNull(l2);

        Assert.AreEqual(new DateTime(1970, 1, 1, 0, 0, 3, DateTimeKind.Utc), l2["First"]);
        Assert.AreEqual(new DateTime(2018, 1, 1, 21, 1, 16, DateTimeKind.Utc), l2["Second"]);
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
        StringAssert.AreEqual(@"{
  ""Object1"": 3,
  ""Object2"": null,
  ""ObjectNotHandled"": 1514840476
}", json);

        var obj2 = JsonConvert.DeserializeObject<UnixConverterObject>(json);
        Xunit.Assert.NotNull(obj2);

        Assert.AreEqual(new DateTime(1970, 1, 1, 0, 0, 3, DateTimeKind.Utc), obj2.Object1);
        Xunit.Assert.Null(obj2.Object2);
        Assert.AreEqual(new DateTime(2018, 1, 1, 21, 1, 16, DateTimeKind.Utc), obj2.ObjectNotHandled);
    }
}

[JsonArray(ItemConverterType = typeof(UnixDateTimeConverter))]
public class UnixConverterList<T> : List<T> { }

[JsonDictionary(ItemConverterType = typeof(UnixDateTimeConverter))]
public class UnixConverterDictionary<T> : Dictionary<string, T> { }

[JsonObject(ItemConverterType = typeof(UnixDateTimeConverter))]
public class UnixConverterObject
{
    public object Object1 { get; set; }

    public object Object2 { get; set; }

    [JsonConverter(typeof(UnixDateTimeConverter))]
    public object ObjectNotHandled { get; set; }
}