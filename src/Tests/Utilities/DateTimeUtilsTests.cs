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

    static StringReference CreateStringReference(string s) =>
        new(s.ToCharArray(), 0, s.Length);

    static void RoundtripDateIso(DateTime value)
    {
        var stringWriter = new StringWriter();
        DateTimeUtils.WriteDateTimeString(stringWriter, value, null, CultureInfo.InvariantCulture);
        var minDateText = stringWriter.ToString();

        DateTimeUtils.TryParseDateTime(minDateText, null, CultureInfo.InvariantCulture, out var parsedDt);

        Assert.Equal(value, parsedDt);
    }

    [Fact]
    public void Parse24HourDateTimeOffset()
    {
        Assert.True(DateTimeUtils.TryParseDateTimeOffsetIso(CreateStringReference("2000-12-15T24:00:00Z"), out var dt));
        Assert.Equal(new(2000, 12, 16, 0, 0, 0, TimeSpan.Zero), dt);

        Assert.False(DateTimeUtils.TryParseDateTimeOffsetIso(CreateStringReference("2000-12-15T24:01:00Z"), out dt));
        Assert.False(DateTimeUtils.TryParseDateTimeOffsetIso(CreateStringReference("2000-12-15T24:00:01Z"), out dt));
        Assert.False(DateTimeUtils.TryParseDateTimeOffsetIso(CreateStringReference("2000-12-15T24:00:00.0000001Z"), out dt));
    }
}