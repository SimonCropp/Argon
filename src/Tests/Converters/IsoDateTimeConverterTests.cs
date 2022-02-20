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

using Argon.Tests.TestObjects;
using Argon.Tests.TestObjects.Organization;
using Xunit;

namespace Argon.Tests.Converters;

public class IsoDateTimeConverterTests : TestFixtureBase
{
    [Fact]
    public void PropertiesShouldBeSet()
    {
        var converter = new IsoDateTimeConverter();
        Assert.Equal(CultureInfo.CurrentCulture, converter.Culture);
        Assert.Equal(string.Empty, converter.DateTimeFormat);
        Assert.Equal(DateTimeStyles.RoundtripKind, converter.DateTimeStyles);

        converter = new IsoDateTimeConverter
        {
            DateTimeFormat = "F",
            Culture = CultureInfo.InvariantCulture,
            DateTimeStyles = DateTimeStyles.None
        };

        Assert.Equal(CultureInfo.InvariantCulture, converter.Culture);
        Assert.Equal("F", converter.DateTimeFormat);
        Assert.Equal(DateTimeStyles.None, converter.DateTimeStyles);
    }

    public static string GetUtcOffsetText(DateTime d)
    {
        var utcOffset = d.GetUtcOffset();

        return $"{utcOffset.Hours.ToString("+00;-00", CultureInfo.InvariantCulture)}:{utcOffset.Minutes.ToString("00;00", CultureInfo.InvariantCulture)}";
    }

    [Fact]
    public void SerializeDateTime()
    {
        var converter = new IsoDateTimeConverter();

        var d = new DateTime(2000, 12, 15, 22, 11, 3, 55, DateTimeKind.Utc);

        var result = JsonConvert.SerializeObject(d, converter);
        Assert.Equal(@"""2000-12-15T22:11:03.055Z""", result);

        Assert.Equal(d, JsonConvert.DeserializeObject<DateTime>(result, converter));

        d = new DateTime(2000, 12, 15, 22, 11, 3, 55, DateTimeKind.Local);
        result = JsonConvert.SerializeObject(d, converter);
        Assert.Equal($@"""2000-12-15T22:11:03.055{GetUtcOffsetText(d)}""", result);
    }

    [Fact]
    public void SerializeFormattedDateTimeInvariantCulture()
    {
        var converter = new IsoDateTimeConverter { DateTimeFormat = "F", Culture = CultureInfo.InvariantCulture };

        var d = new DateTime(2000, 12, 15, 22, 11, 3, 0, DateTimeKind.Utc);

        var result = JsonConvert.SerializeObject(d, converter);
        Assert.Equal(@"""Friday, 15 December 2000 22:11:03""", result);

        Assert.Equal(d, JsonConvert.DeserializeObject<DateTime>(result, converter));

        d = new DateTime(2000, 12, 15, 22, 11, 3, 0, DateTimeKind.Local);
        result = JsonConvert.SerializeObject(d, converter);
        Assert.Equal(@"""Friday, 15 December 2000 22:11:03""", result);
    }

    [Fact]
    public void SerializeCustomFormattedDateTime()
    {
        var converter = new IsoDateTimeConverter
        {
            DateTimeFormat = "dd/MM/yyyy",
            Culture = CultureInfo.InvariantCulture
        };

        var json = @"""09/12/2006""";

        var d = JsonConvert.DeserializeObject<DateTime>(json, converter);

        Assert.Equal(9, d.Day);
        Assert.Equal(12, d.Month);
        Assert.Equal(2006, d.Year);
    }

    [Fact]
    public void SerializeFormattedDateTimeNewZealandCulture()
    {
        var culture = new CultureInfo("en-NZ")
        {
            DateTimeFormat =
            {
                AMDesignator = "a.m.",
                PMDesignator = "p.m."
            }
        };

        var converter = new IsoDateTimeConverter { DateTimeFormat = "F", Culture = culture };

        var d = new DateTime(2000, 12, 15, 22, 11, 3, 0, DateTimeKind.Utc);

        var result = JsonConvert.SerializeObject(d, converter);
        Assert.Equal(@"""Friday, 15 December 2000 10:11:03 p.m.""", result);

        Assert.Equal(d, JsonConvert.DeserializeObject<DateTime>(result, converter));

        d = new DateTime(2000, 12, 15, 22, 11, 3, 0, DateTimeKind.Local);
        result = JsonConvert.SerializeObject(d, converter);
        Assert.Equal(@"""Friday, 15 December 2000 10:11:03 p.m.""", result);
    }

    [Fact]
    public void SerializeDateTimeCulture()
    {
        var converter = new IsoDateTimeConverter { Culture = CultureInfo.GetCultureInfo("en-NZ") };

        var json = @"""09/12/2006""";

        var d = JsonConvert.DeserializeObject<DateTime>(json, converter);

        Assert.Equal(9, d.Day);
        Assert.Equal(12, d.Month);
        Assert.Equal(2006, d.Year);
    }

    [Fact]
    public void SerializeDateTimeOffset()
    {
        var converter = new IsoDateTimeConverter();

        var d = new DateTimeOffset(2000, 12, 15, 22, 11, 3, 55, TimeSpan.Zero);

        var result = JsonConvert.SerializeObject(d, converter);
        Assert.Equal(@"""2000-12-15T22:11:03.055+00:00""", result);

        Assert.Equal(d, JsonConvert.DeserializeObject<DateTimeOffset>(result, converter));
    }

    [Fact]
    public void SerializeUTC()
    {
        var c = new DateTimeTestClass
        {
            DateTimeField = new DateTime(2008, 12, 12, 12, 12, 12, 0, DateTimeKind.Utc).ToLocalTime(),
            DateTimeOffsetField = new DateTime(2008, 12, 12, 12, 12, 12, 0, DateTimeKind.Utc).ToLocalTime(),
            PreField = "Pre",
            PostField = "Post"
        };
        var json = JsonConvert.SerializeObject(c, new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal });
        Assert.Equal(@"{""PreField"":""Pre"",""DateTimeField"":""2008-12-12T12:12:12Z"",""DateTimeOffsetField"":""2008-12-12T12:12:12+00:00"",""PostField"":""Post""}", json);

        //test the other edge case too
        c.DateTimeField = new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Utc).ToLocalTime();
        c.DateTimeOffsetField = new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Utc).ToLocalTime();
        c.PreField = "Pre";
        c.PostField = "Post";
        json = JsonConvert.SerializeObject(c, new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal });
        Assert.Equal(@"{""PreField"":""Pre"",""DateTimeField"":""2008-01-01T01:01:01Z"",""DateTimeOffsetField"":""2008-01-01T01:01:01+00:00"",""PostField"":""Post""}", json);
    }

    [Fact]
    public void NullableSerializeUTC()
    {
        var c = new NullableDateTimeTestClass
        {
            DateTimeField = new DateTime(2008, 12, 12, 12, 12, 12, 0, DateTimeKind.Utc).ToLocalTime(),
            DateTimeOffsetField = new DateTime(2008, 12, 12, 12, 12, 12, 0, DateTimeKind.Utc).ToLocalTime(),
            PreField = "Pre",
            PostField = "Post"
        };
        var json = JsonConvert.SerializeObject(c, new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal });
        Assert.Equal(@"{""PreField"":""Pre"",""DateTimeField"":""2008-12-12T12:12:12Z"",""DateTimeOffsetField"":""2008-12-12T12:12:12+00:00"",""PostField"":""Post""}", json);

        //test the other edge case too
        c.DateTimeField = null;
        c.DateTimeOffsetField = null;
        c.PreField = "Pre";
        c.PostField = "Post";
        json = JsonConvert.SerializeObject(c, new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal });
        Assert.Equal(@"{""PreField"":""Pre"",""DateTimeField"":null,""DateTimeOffsetField"":null,""PostField"":""Post""}", json);
    }

    [Fact]
    public void NullableDeserializeEmptyString()
    {
        var json = @"{""DateTimeField"":""""}";

        var c = JsonConvert.DeserializeObject<NullableDateTimeTestClass>(json,
            new JsonSerializerSettings { Converters = new[] { new IsoDateTimeConverter() } });
        Assert.Equal(null, c.DateTimeField);
    }

    [Fact]
    public void DeserializeNullToNonNullable()
    {
        XUnitAssert.Throws<JsonSerializationException>(
            () =>
            {
                var c2 = JsonConvert.DeserializeObject<DateTimeTestClass>(@"{""PreField"":""Pre"",""DateTimeField"":null,""DateTimeOffsetField"":null,""PostField"":""Post""}", new IsoDateTimeConverter {DateTimeStyles = DateTimeStyles.AssumeUniversal});
            },
            "Cannot convert null value to System.DateTime. Path 'DateTimeField', line 1, position 38.");
    }

    [Fact]
    public void SerializeShouldChangeNonUTCDates()
    {
        var localDateTime = new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Local);

        var c = new DateTimeTestClass
        {
            DateTimeField = localDateTime,
            PreField = "Pre",
            PostField = "Post"
        };
        var json = JsonConvert.SerializeObject(c, new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }); //note that this fails without the Utc converter...
        c.DateTimeField = new DateTime(2008, 1, 1, 1, 1, 1, 0, DateTimeKind.Utc);
        var json2 = JsonConvert.SerializeObject(c, new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal });

        var offset = localDateTime.GetUtcOffset();

        // if the current timezone is utc then local already equals utc
        if (offset == TimeSpan.Zero)
        {
            Assert.Equal(json, json2);
        }
        else
        {
            Assert.NotEqual(json, json2);
        }
    }

    [Fact]
    public void BlogCodeSample()
    {
        var p = new Person
        {
            Name = "Keith",
            BirthDate = new DateTime(1980, 3, 8),
            LastModified = new DateTime(2009, 4, 12, 20, 44, 55),
        };

        var jsonText = JsonConvert.SerializeObject(p, new IsoDateTimeConverter());
        // {
        //   "Name": "Keith",
        //   "BirthDate": "1980-03-08T00:00:00",
        //   "LastModified": "2009-04-12T20:44:55"
        // }

        Assert.Equal(@"{""Name"":""Keith"",""BirthDate"":""1980-03-08T00:00:00"",""LastModified"":""2009-04-12T20:44:55""}", jsonText);
    }

    [Fact]
    public void DeserializeDateTimeOffset()
    {
        var settings = new JsonSerializerSettings
        {
            DateParseHandling = DateParseHandling.DateTimeOffset
        };
        settings.Converters.Add(new IsoDateTimeConverter());

        // Intentionally use an offset that is unlikely in the real world,
        // so the test will be accurate regardless of the local time zone setting.
        var offset = new TimeSpan(2, 15, 0);
        var dto = new DateTimeOffset(2014, 1, 1, 0, 0, 0, 0, offset);

        var test = JsonConvert.DeserializeObject<DateTimeOffset>("\"2014-01-01T00:00:00+02:15\"", settings);

        Assert.Equal(dto, test);
        Assert.Equal(dto.ToString("o"), test.ToString("o"));
    }
}