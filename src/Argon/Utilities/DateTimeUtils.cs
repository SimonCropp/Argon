// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

static class DateTimeUtils
{
    const string IsoDateFormat = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK";

    const int DaysPer100Years = 36524;
    const int DaysPer400Years = 146097;
    const int DaysPer4Years = 1461;
    const int DaysPerYear = 365;
    const long TicksPerDay = 864000000000L;
    static readonly int[] DaysToMonth365;
    static readonly int[] DaysToMonth366;

    static DateTimeUtils()
    {
        DaysToMonth365 = new[] { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365 };
        DaysToMonth366 = new[] { 0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366 };
    }

    public static TimeSpan GetUtcOffset(this DateTime d)
    {
        return TimeZoneInfo.Local.GetUtcOffset(d);
    }

    internal static DateTime EnsureDateTime(DateTime value, DateTimeZoneHandling timeZone)
    {
        switch (timeZone)
        {
            case DateTimeZoneHandling.Local:
                value = SwitchToLocalTime(value);
                break;
            case DateTimeZoneHandling.Utc:
                value = SwitchToUtcTime(value);
                break;
            case DateTimeZoneHandling.Unspecified:
                value = new DateTime(value.Ticks, DateTimeKind.Unspecified);
                break;
            case DateTimeZoneHandling.RoundtripKind:
                break;
            default:
                throw new ArgumentException("Invalid date time handling value.");
        }

        return value;
    }

    static DateTime SwitchToLocalTime(DateTime value)
    {
        switch (value.Kind)
        {
            case DateTimeKind.Unspecified:
                return new DateTime(value.Ticks, DateTimeKind.Local);

            case DateTimeKind.Utc:
                return value.ToLocalTime();

            case DateTimeKind.Local:
                return value;
        }
        return value;
    }

    static DateTime SwitchToUtcTime(DateTime value)
    {
        switch (value.Kind)
        {
            case DateTimeKind.Unspecified:
                return new DateTime(value.Ticks, DateTimeKind.Utc);

            case DateTimeKind.Utc:
                return value;

            case DateTimeKind.Local:
                return value.ToUniversalTime();
        }
        return value;
    }

    static long ToUniversalTicks(DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Utc)
        {
            return dateTime.Ticks;
        }

        return ToUniversalTicks(dateTime, dateTime.GetUtcOffset());
    }

    static long ToUniversalTicks(DateTime dateTime, TimeSpan offset)
    {
        // special case min and max value
        // they never have a timezone appended to avoid issues
        if (dateTime.Kind == DateTimeKind.Utc || dateTime == DateTime.MaxValue || dateTime == DateTime.MinValue)
        {
            return dateTime.Ticks;
        }

        var ticks = dateTime.Ticks - offset.Ticks;
        if (ticks > 3155378975999999999L)
        {
            return 3155378975999999999L;
        }

        if (ticks < 0L)
        {
            return 0L;
        }

        return ticks;
    }

    internal static long ConvertDateTimeToJavaScriptTicks(DateTime dateTime)
    {
        return ConvertDateTimeToJavaScriptTicks(dateTime, true);
    }

    static long ConvertDateTimeToJavaScriptTicks(DateTime dateTime, bool convertToUtc)
    {
        var ticks = convertToUtc ? ToUniversalTicks(dateTime) : dateTime.Ticks;

        return UniversalTicksToJavaScriptTicks(ticks);
    }

    static long UniversalTicksToJavaScriptTicks(long universalTicks)
    {
        var javaScriptTicks = (universalTicks - InitialJavaScriptDateTicks) / 10000;

        return javaScriptTicks;
    }

    internal static DateTime ConvertJavaScriptTicksToDateTime(long javaScriptTicks)
    {
        var dateTime = new DateTime(javaScriptTicks * 10000 + InitialJavaScriptDateTicks, DateTimeKind.Utc);

        return dateTime;
    }

    #region Parse
    internal static bool TryParseDateTimeIso(StringReference text, DateTimeZoneHandling dateTimeZoneHandling, out DateTime dt)
    {
        var dateTimeParser = new DateTimeParser();
        if (!dateTimeParser.Parse(text.Chars, text.StartIndex, text.Length))
        {
            dt = default;
            return false;
        }

        var d = CreateDateTime(dateTimeParser);

        long ticks;

        switch (dateTimeParser.Zone)
        {
            case ParserTimeZone.Utc:
                d = new DateTime(d.Ticks, DateTimeKind.Utc);
                break;

            case ParserTimeZone.LocalWestOfUtc:
            {
                var offset = new TimeSpan(dateTimeParser.ZoneHour, dateTimeParser.ZoneMinute, 0);
                ticks = d.Ticks + offset.Ticks;
                if (ticks <= DateTime.MaxValue.Ticks)
                {
                    d = new DateTime(ticks, DateTimeKind.Utc).ToLocalTime();
                }
                else
                {
                    ticks += d.GetUtcOffset().Ticks;
                    if (ticks > DateTime.MaxValue.Ticks)
                    {
                        ticks = DateTime.MaxValue.Ticks;
                    }

                    d = new DateTime(ticks, DateTimeKind.Local);
                }
                break;
            }
            case ParserTimeZone.LocalEastOfUtc:
            {
                var offset = new TimeSpan(dateTimeParser.ZoneHour, dateTimeParser.ZoneMinute, 0);
                ticks = d.Ticks - offset.Ticks;
                if (ticks >= DateTime.MinValue.Ticks)
                {
                    d = new DateTime(ticks, DateTimeKind.Utc).ToLocalTime();
                }
                else
                {
                    ticks += d.GetUtcOffset().Ticks;
                    if (ticks < DateTime.MinValue.Ticks)
                    {
                        ticks = DateTime.MinValue.Ticks;
                    }

                    d = new DateTime(ticks, DateTimeKind.Local);
                }
                break;
            }
        }

        dt = EnsureDateTime(d, dateTimeZoneHandling);
        return true;
    }

    internal static bool TryParseDateTimeOffsetIso(StringReference text, out DateTimeOffset dt)
    {
        var dateTimeParser = new DateTimeParser();
        if (!dateTimeParser.Parse(text.Chars, text.StartIndex, text.Length))
        {
            dt = default;
            return false;
        }

        var d = CreateDateTime(dateTimeParser);

        TimeSpan offset;

        switch (dateTimeParser.Zone)
        {
            case ParserTimeZone.Utc:
                offset = new TimeSpan(0L);
                break;
            case ParserTimeZone.LocalWestOfUtc:
                offset = new TimeSpan(-dateTimeParser.ZoneHour, -dateTimeParser.ZoneMinute, 0);
                break;
            case ParserTimeZone.LocalEastOfUtc:
                offset = new TimeSpan(dateTimeParser.ZoneHour, dateTimeParser.ZoneMinute, 0);
                break;
            default:
                offset = TimeZoneInfo.Local.GetUtcOffset(d);
                break;
        }

        var ticks = d.Ticks - offset.Ticks;
        if (ticks is < 0 or > 3155378975999999999)
        {
            dt = default;
            return false;
        }

        dt = new DateTimeOffset(d, offset);
        return true;
    }

    static DateTime CreateDateTime(DateTimeParser dateTimeParser)
    {
        bool is24Hour;
        if (dateTimeParser.Hour == 24)
        {
            is24Hour = true;
            dateTimeParser.Hour = 0;
        }
        else
        {
            is24Hour = false;
        }

        var d = new DateTime(dateTimeParser.Year, dateTimeParser.Month, dateTimeParser.Day, dateTimeParser.Hour, dateTimeParser.Minute, dateTimeParser.Second);
        d = d.AddTicks(dateTimeParser.Fraction);

        if (is24Hour)
        {
            d = d.AddDays(1);
        }
        return d;
    }

    internal static bool TryParseDateTime(StringReference s, DateTimeZoneHandling dateTimeZoneHandling, string? dateFormatString, CultureInfo culture, out DateTime dt)
    {
        if (s.Length > 0)
        {
            var i = s.StartIndex;
            if (s.Length is >= 19 and <= 40 && char.IsDigit(s[i]) && s[i + 10] == 'T')
            {
                if (TryParseDateTimeIso(s, dateTimeZoneHandling, out dt))
                {
                    return true;
                }
            }

            if (!StringUtils.IsNullOrEmpty(dateFormatString))
            {
                if (TryParseDateTimeExact(s.ToString(), dateTimeZoneHandling, dateFormatString, culture, out dt))
                {
                    return true;
                }
            }
        }

        dt = default;
        return false;
    }

    internal static bool TryParseDateTime(string s, DateTimeZoneHandling dateTimeZoneHandling, string? dateFormatString, CultureInfo culture, out DateTime dt)
    {
        if (s.Length > 0)
        {
            if (s.Length is >= 19 and <= 40 && char.IsDigit(s[0]) && s[10] == 'T')
            {
                if (DateTime.TryParseExact(s, IsoDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dt))
                {
                    dt = EnsureDateTime(dt, dateTimeZoneHandling);
                    return true;
                }
            }

            if (!StringUtils.IsNullOrEmpty(dateFormatString))
            {
                if (TryParseDateTimeExact(s, dateTimeZoneHandling, dateFormatString, culture, out dt))
                {
                    return true;
                }
            }
        }

        dt = default;
        return false;
    }

    internal static bool TryParseDateTimeOffset(StringReference s, string? dateFormatString, CultureInfo culture, out DateTimeOffset dt)
    {
        if (s.Length > 0)
        {
            var i = s.StartIndex;
            if (s.Length is >= 19 and <= 40 && char.IsDigit(s[i]) && s[i + 10] == 'T')
            {
                if (TryParseDateTimeOffsetIso(s, out dt))
                {
                    return true;
                }
            }

            if (!StringUtils.IsNullOrEmpty(dateFormatString))
            {
                if (TryParseDateTimeOffsetExact(s.ToString(), dateFormatString, culture, out dt))
                {
                    return true;
                }
            }
        }

        dt = default;
        return false;
    }

    internal static bool TryParseDateTimeOffset(string s, string? dateFormatString, CultureInfo culture, out DateTimeOffset dt)
    {
        if (s.Length > 0)
        {
            if (s.Length is >= 19 and <= 40 && char.IsDigit(s[0]) && s[10] == 'T')
            {
                if (DateTimeOffset.TryParseExact(s, IsoDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dt))
                {
                    if (TryParseDateTimeOffsetIso(new StringReference(s.ToCharArray(), 0, s.Length), out dt))
                    {
                        return true;
                    }
                }
            }

            if (!StringUtils.IsNullOrEmpty(dateFormatString))
            {
                if (TryParseDateTimeOffsetExact(s, dateFormatString, culture, out dt))
                {
                    return true;
                }
            }
        }

        dt = default;
        return false;
    }

    static bool TryParseDateTimeExact(string text, DateTimeZoneHandling dateTimeZoneHandling, string dateFormatString, CultureInfo culture, out DateTime dt)
    {
        if (DateTime.TryParseExact(text, dateFormatString, culture, DateTimeStyles.RoundtripKind, out var temp))
        {
            temp = EnsureDateTime(temp, dateTimeZoneHandling);
            dt = temp;
            return true;
        }

        dt = default;
        return false;
    }

    static bool TryParseDateTimeOffsetExact(string text, string dateFormatString, CultureInfo culture, out DateTimeOffset dt)
    {
        if (DateTimeOffset.TryParseExact(text, dateFormatString, culture, DateTimeStyles.RoundtripKind, out var temp))
        {
            dt = temp;
            return true;
        }

        dt = default;
        return false;
    }
    #endregion

    #region Write
    internal static void WriteDateTimeString(TextWriter writer, DateTime value, string? formatString, CultureInfo culture)
    {
        if (StringUtils.IsNullOrEmpty(formatString))
        {
            var chars = new char[64];
            var pos = WriteDateTimeString(chars, 0, value, null, value.Kind);
            writer.Write(chars, 0, pos);
        }
        else
        {
            writer.Write(value.ToString(formatString, culture));
        }
    }

    internal static int WriteDateTimeString(char[] chars, int start, DateTime value, TimeSpan? offset, DateTimeKind kind)
    {
          var  pos = WriteDefaultIsoDate(chars, start, value);

          if (kind == DateTimeKind.Local)
          {
              return WriteDateTimeOffset(chars, pos, offset ?? value.GetUtcOffset());
          }

          if (kind == DateTimeKind.Utc)
          {
              chars[pos++] = 'Z';
          }

          return pos;
    }

    static int WriteDefaultIsoDate(char[] chars, int start, DateTime dt)
    {
        var length = 19;

        GetDateValues(dt, out var year, out var month, out var day);

        CopyIntToCharArray(chars, start, year, 4);
        chars[start + 4] = '-';
        CopyIntToCharArray(chars, start + 5, month, 2);
        chars[start + 7] = '-';
        CopyIntToCharArray(chars, start + 8, day, 2);
        chars[start + 10] = 'T';
        CopyIntToCharArray(chars, start + 11, dt.Hour, 2);
        chars[start + 13] = ':';
        CopyIntToCharArray(chars, start + 14, dt.Minute, 2);
        chars[start + 16] = ':';
        CopyIntToCharArray(chars, start + 17, dt.Second, 2);

        var fraction = (int)(dt.Ticks % 10000000L);

        if (fraction != 0)
        {
            var digits = 7;
            while (fraction % 10 == 0)
            {
                digits--;
                fraction /= 10;
            }

            chars[start + 19] = '.';
            CopyIntToCharArray(chars, start + 20, fraction, digits);

            length += digits + 1;
        }

        return start + length;
    }

    static void CopyIntToCharArray(char[] chars, int start, int value, int digits)
    {
        while (digits-- != 0)
        {
            chars[start + digits] = (char)(value % 10 + 48);
            value /= 10;
        }
    }

    internal static int WriteDateTimeOffset(char[] chars, int start, TimeSpan offset)
    {
        chars[start++] = offset.Ticks >= 0L ? '+' : '-';

        var absHours = Math.Abs(offset.Hours);
        CopyIntToCharArray(chars, start, absHours, 2);
        start += 2;

        chars[start++] = ':';

        var absMinutes = Math.Abs(offset.Minutes);
        CopyIntToCharArray(chars, start, absMinutes, 2);
        start += 2;

        return start;
    }

    internal static void WriteDateTimeOffsetString(TextWriter writer, DateTimeOffset value, string? formatString, CultureInfo culture)
    {
        if (StringUtils.IsNullOrEmpty(formatString))
        {
            var chars = new char[64];
            var pos = WriteDateTimeString(chars, 0, value.DateTime, value.Offset, DateTimeKind.Local);

            writer.Write(chars, 0, pos);
        }
        else
        {
            writer.Write(value.ToString(formatString, culture));
        }
    }
    #endregion

    static void GetDateValues(DateTime td, out int year, out int month, out int day)
    {
        var ticks = td.Ticks;
        // n = number of days since 1/1/0001
        var n = (int)(ticks / TicksPerDay);
        // y400 = number of whole 400-year periods since 1/1/0001
        var y400 = n / DaysPer400Years;
        // n = day number within 400-year period
        n -= y400 * DaysPer400Years;
        // y100 = number of whole 100-year periods within 400-year period
        var y100 = n / DaysPer100Years;
        // Last 100-year period has an extra day, so decrement result if 4
        if (y100 == 4)
        {
            y100 = 3;
        }
        // n = day number within 100-year period
        n -= y100 * DaysPer100Years;
        // y4 = number of whole 4-year periods within 100-year period
        var y4 = n / DaysPer4Years;
        // n = day number within 4-year period
        n -= y4 * DaysPer4Years;
        // y1 = number of whole years within 4-year period
        var y1 = n / DaysPerYear;
        // Last year has an extra day, so decrement result if 4
        if (y1 == 4)
        {
            y1 = 3;
        }

        year = y400 * 400 + y100 * 100 + y4 * 4 + y1 + 1;

        // n = day number within year
        n -= y1 * DaysPerYear;

        // Leap year calculation looks different from IsLeapYear since y1, y4,
        // and y100 are relative to year 1, not year 0
        var leapYear = y1 == 3 && (y4 != 24 || y100 == 3);
        var days = leapYear ? DaysToMonth366 : DaysToMonth365;
        // All months have less than 32 days, so n >> 5 is a good conservative
        // estimate for the month
        var m = n >> 5 + 1;
        // m = 1-based month number
        while (n >= days[m])
        {
            m++;
        }

        month = m;

        // Return 1-based day-of-month
        day = n - days[m - 1] + 1;
    }
}