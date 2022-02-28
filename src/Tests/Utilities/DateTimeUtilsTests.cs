// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

public class DateTimeUtilsTests : TestFixtureBase
{
    [Fact]
    public void RoundTripDateTimeMinAndMax()
    {
        RoundtripDateIso(DateTime.MinValue);
        RoundtripDateIso(DateTime.MaxValue);
    }

    static StringReference CreateStringReference(string s)
    {
        return new StringReference(s.ToCharArray(), 0, s.Length);
    }

    static void RoundtripDateIso(DateTime value)
    {
        var stringWriter = new StringWriter();
        DateTimeUtils.WriteDateTimeString(stringWriter, value, null, CultureInfo.InvariantCulture);
        var minDateText = stringWriter.ToString();

        DateTimeUtils.TryParseDateTimeIso(CreateStringReference(minDateText), DateTimeZoneHandling.RoundtripKind, out var parsedDt);

        Assert.Equal(value, parsedDt);
    }

    [Fact]
    public void Parse24HourDateTime()
    {
        Assert.True(DateTimeUtils.TryParseDateTimeIso(CreateStringReference("2000-12-15T24:00:00Z"), DateTimeZoneHandling.RoundtripKind, out var dt));
        Assert.Equal(new DateTime(2000, 12, 16, 0, 0, 0, DateTimeKind.Utc), dt);

        Assert.False(DateTimeUtils.TryParseDateTimeIso(CreateStringReference("2000-12-15T24:01:00Z"), DateTimeZoneHandling.RoundtripKind, out dt));
        Assert.False(DateTimeUtils.TryParseDateTimeIso(CreateStringReference("2000-12-15T24:00:01Z"), DateTimeZoneHandling.RoundtripKind, out dt));
        Assert.False(DateTimeUtils.TryParseDateTimeIso(CreateStringReference("2000-12-15T24:00:00.0000001Z"), DateTimeZoneHandling.RoundtripKind, out dt));
    }

    [Fact]
    public void Parse24HourDateTimeOffset()
    {
        Assert.True(DateTimeUtils.TryParseDateTimeOffsetIso(CreateStringReference("2000-12-15T24:00:00Z"), out var dt));
        Assert.Equal(new DateTimeOffset(2000, 12, 16, 0, 0, 0, TimeSpan.Zero), dt);

        Assert.False(DateTimeUtils.TryParseDateTimeOffsetIso(CreateStringReference("2000-12-15T24:01:00Z"), out dt));
        Assert.False(DateTimeUtils.TryParseDateTimeOffsetIso(CreateStringReference("2000-12-15T24:00:01Z"), out dt));
        Assert.False(DateTimeUtils.TryParseDateTimeOffsetIso(CreateStringReference("2000-12-15T24:00:00.0000001Z"), out dt));
    }

    [Fact]
    public void NewDateTimeParse()
    {
        AssertNewDateTimeParseEqual("999x-12-31T23:59:59");
        AssertNewDateTimeParseEqual("9999x12-31T23:59:59");
        AssertNewDateTimeParseEqual("9999-1x-31T23:59:59");
        AssertNewDateTimeParseEqual("9999-12x31T23:59:59");
        AssertNewDateTimeParseEqual("9999-12-3xT23:59:59");
        AssertNewDateTimeParseEqual("9999-12-31x23:59:59");
        AssertNewDateTimeParseEqual("9999-12-31T2x:59:59");
        AssertNewDateTimeParseEqual("9999-12-31T23x59:59");
        AssertNewDateTimeParseEqual("9999-12-31T23:5x:59");
        AssertNewDateTimeParseEqual("9999-12-31T23:59x59");
        AssertNewDateTimeParseEqual("9999-12-31T23:59:5x");
        AssertNewDateTimeParseEqual("9999-12-31T23:59:5");
        AssertNewDateTimeParseEqual("9999-12-31T23:59:59.x");
        AssertNewDateTimeParseEqual("9999-12-31T23:59:59.99999999");
        //AssertNewDateTimeParseEqual("9999-12-31T23:59:59.", null); // DateTime.TryParse is bugged and should return null

        AssertNewDateTimeParseEqual("2000-12-15T22:11:03.055Z");
        AssertNewDateTimeParseEqual("2000-12-15T22:11:03.055");
        AssertNewDateTimeParseEqual("2000-12-15T22:11:03.055+00:00");
        AssertNewDateTimeParseEqual("2000-12-15T22:11:03.055+11:30");
        AssertNewDateTimeParseEqual("2000-12-15T22:11:03.055-11:30");

        AssertNewDateTimeParseEqual("2000-12-15T22:11:03Z");
        AssertNewDateTimeParseEqual("2000-12-15T22:11:03");
        AssertNewDateTimeParseEqual("2000-12-15T22:11:03+00:00");
        AssertNewDateTimeParseEqual("2000-12-15T22:11:03+11:30");
        AssertNewDateTimeParseEqual("2000-12-15T22:11:03-11:30");

        AssertNewDateTimeParseEqual("0001-01-01T00:00:00Z");
        AssertNewDateTimeParseEqual("0001-01-01T00:00:00"); // this is DateTime.MinDate
        //AssertNewDateTimeParseEqual("0001-01-01T00:00:00+00:00"); // when the timezone is negative then this breaks
        //AssertNewDateTimeParseEqual("0001-01-01T00:00:00+11:30"); // when the timezone is negative then this breaks
        AssertNewDateTimeParseEqual("0001-01-01T00:00:00-12:00");

        AssertNewDateTimeParseEqual("9999-12-31T23:59:59.9999999Z");
        AssertNewDateTimeParseEqual("9999-12-31T23:59:59.9999999"); // this is DateTime.MaxDate
        AssertNewDateTimeParseEqual("9999-12-31T23:59:59.9999999+00:00", DateTime.MaxValue); // DateTime.TryParse fails instead of returning MaxDate in some timezones
        AssertNewDateTimeParseEqual("9999-12-31T23:59:59.9999999+11:30", DateTime.MaxValue); // DateTime.TryParse fails instead of returning MaxDate in some timezones
        AssertNewDateTimeParseEqual("9999-12-31T23:59:59.9999999-11:30", DateTime.MaxValue); // DateTime.TryParse fails instead of returning MaxDate in some timezones
    }

    static void AssertNewDateTimeParseEqual(string text, object oldDate)
    {
        if (TryParseDateIso(text, DateParseHandling.DateTime, DateTimeZoneHandling.RoundtripKind, out var oldDt))
        {
            oldDate = oldDt;
        }

        object newDt = null;
        if (DateTimeUtils.TryParseDateTimeIso(CreateStringReference(text), DateTimeZoneHandling.RoundtripKind, out var temp))
        {
            newDt = temp;
        }

        if (!Equals(oldDate, newDt))
        {
            Assert.Equal(oldDate, newDt);
        }
    }

    static void AssertNewDateTimeParseEqual(string text)
    {
        //Console.WriteLine("Parsing date text: " + text);

        TryParseDateIso(text, DateParseHandling.DateTime, DateTimeZoneHandling.RoundtripKind, out var oldDt);

        AssertNewDateTimeParseEqual(text, oldDt);
    }

    [Fact]
    public void NewDateTimeOffsetParse()
    {
        AssertNewDateTimeOffsetParseEqual("0001-01-01T00:00:00");

        AssertNewDateTimeOffsetParseEqual("2000-12-15T22:11:03.055Z");
        AssertNewDateTimeOffsetParseEqual("2000-12-15T22:11:03.055");
        AssertNewDateTimeOffsetParseEqual("2000-12-15T22:11:03.055+00:00");
        AssertNewDateTimeOffsetParseEqual("2000-12-15T22:11:03.055+13:30");
        AssertNewDateTimeOffsetParseEqual("2000-12-15T22:11:03.055-13:30");

        AssertNewDateTimeOffsetParseEqual("2000-12-15T22:11:03Z");
        AssertNewDateTimeOffsetParseEqual("2000-12-15T22:11:03");
        AssertNewDateTimeOffsetParseEqual("2000-12-15T22:11:03+00:00");
        AssertNewDateTimeOffsetParseEqual("2000-12-15T22:11:03+13:30");
        AssertNewDateTimeOffsetParseEqual("2000-12-15T22:11:03-13:30");

        AssertNewDateTimeOffsetParseEqual("0001-01-01T00:00:00Z");
        AssertNewDateTimeOffsetParseEqual("0001-01-01T00:00:00+00:00");
        AssertNewDateTimeOffsetParseEqual("0001-01-01T00:00:00+13:30");
        AssertNewDateTimeOffsetParseEqual("0001-01-01T00:00:00-13:30");

        AssertNewDateTimeOffsetParseEqual("9999-12-31T23:59:59.9999999Z");
        AssertNewDateTimeOffsetParseEqual("9999-12-31T23:59:59.9999999");
        AssertNewDateTimeOffsetParseEqual("9999-12-31T23:59:59.9999999+00:00");
        AssertNewDateTimeOffsetParseEqual("9999-12-31T23:59:59.9999999+13:30");
        AssertNewDateTimeOffsetParseEqual("9999-12-31T23:59:59.9999999-13:30");
    }

    static void AssertNewDateTimeOffsetParseEqual(string text)
    {
        object newDt = null;

        TryParseDateIso(text, DateParseHandling.DateTimeOffset, DateTimeZoneHandling.Unspecified, out var oldDt);

        if (DateTimeUtils.TryParseDateTimeOffsetIso(CreateStringReference(text), out var temp))
        {
            newDt = temp;
        }

        if (!Equals(oldDt, newDt))
        {
            var oldTicks = oldDt != null ? (long?)((DateTime)oldDt).Ticks : null;
            var newTicks = newDt != null ? (long?)((DateTime)newDt).Ticks : null;

            Assert.Equal(oldDt, newDt);
        }
    }

    internal static bool TryParseDateIso(string text, DateParseHandling dateParseHandling, DateTimeZoneHandling dateTimeZoneHandling, out object dt)
    {
        const string isoDateFormat = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK";

        if (dateParseHandling == DateParseHandling.DateTimeOffset)
        {
            if (DateTimeOffset.TryParseExact(text, isoDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTimeOffset))
            {
                dt = dateTimeOffset;
                return true;
            }
        }
        else
        {
            if (DateTime.TryParseExact(text, isoDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTime))
            {
                dateTime = DateTimeUtils.EnsureDateTime(dateTime, dateTimeZoneHandling);

                dt = dateTime;
                return true;
            }
        }

        dt = null;
        return false;
    }
}