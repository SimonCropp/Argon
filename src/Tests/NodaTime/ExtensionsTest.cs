// Copyright 2014 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Argon.NodaTime;
using NodaTime;

public class ExtensionsTest
{
    [Fact]
    public void Serializer_ConfigureForNodaTime_DefaultInterval()
    {
        var configuredSerializer = new JsonSerializer().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        var explicitSerializer = new JsonSerializer {
            Converters =
            {
                NodaConverters.IntervalConverter, NodaConverters.InstantConverter
            }
        };
        var interval = new Interval(Instant.FromUnixTimeTicks(1000L), Instant.FromUnixTimeTicks(20000L));
        Assert.Equal(Serialize(interval, explicitSerializer),
            Serialize(interval, configuredSerializer));
    }

    [Fact]
    public void Serializer_ConfigureForNodaTime_WithIsoIntervalConverter()
    {
        var configuredSerializer = new JsonSerializer().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb).WithIsoIntervalConverter();
        var explicitSerializer = new JsonSerializer
        {
            Converters =
            {
                NodaConverters.IsoIntervalConverter
            }
        };
        var interval = new Interval(Instant.FromUnixTimeTicks(1000L), Instant.FromUnixTimeTicks(20000L));
        Assert.Equal(Serialize(interval, explicitSerializer),
            Serialize(interval, configuredSerializer));
    }

    [Fact]
    public void Settings_ConfigureForNodaTime_DefaultInterval()
    {
        var configuredSettings = new JsonSerializerSettings().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        var explicitSettings = new JsonSerializerSettings
        {
            Converters =
            {
                NodaConverters.IntervalConverter,
                NodaConverters.InstantConverter
            }
        };
        var interval = new Interval(Instant.FromUnixTimeTicks(1000L), Instant.FromUnixTimeTicks(20000L));
        Assert.Equal(JsonConvert.SerializeObject(interval, explicitSettings),
            JsonConvert.SerializeObject(interval, configuredSettings));
    }

    [Fact]
    public void Settings_ConfigureForNodaTime_WithIsoIntervalConverter()
    {
        var configuredSettings = new JsonSerializerSettings().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb).WithIsoIntervalConverter();
        var explicitSettings = new JsonSerializerSettings
        {
            Converters =
            {
                NodaConverters.IsoIntervalConverter
            }
        };
        var interval = new Interval(Instant.FromUnixTimeTicks(1000L), Instant.FromUnixTimeTicks(20000L));
        Assert.Equal(JsonConvert.SerializeObject(interval, explicitSettings),
            JsonConvert.SerializeObject(interval, configuredSettings));
    }

    [Fact]
    public void Serializer_ConfigureForNodaTime_DefaultDateInterval()
    {
        var configuredSerializer = new JsonSerializer().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        var explicitSerializer = new JsonSerializer
        {
            Converters =
            {
                NodaConverters.DateIntervalConverter,
                NodaConverters.LocalDateConverter
            }
        };
        var interval = new DateInterval(new(2001, 2, 3), new(2004, 5, 6));
        Assert.Equal(Serialize(interval, explicitSerializer),
            Serialize(interval, configuredSerializer));
    }

    [Fact]
    public void Serializer_ConfigureForNodaTime_WithIsoDateIntervalConverter()
    {
        var configuredSerializer = new JsonSerializer().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb).WithIsoDateIntervalConverter();
        var explicitSerializer = new JsonSerializer
        {
            Converters =
            {
                NodaConverters.IsoDateIntervalConverter
            }
        };
        var interval = new DateInterval(new(2001, 2, 3), new(2004, 5, 6));
        Assert.Equal(Serialize(interval, explicitSerializer),
            Serialize(interval, configuredSerializer));
    }

    [Fact]
    public void Settings_ConfigureForNodaTime_DefaultDateInterval()
    {
        var configuredSettings = new JsonSerializerSettings().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        var explicitSettings = new JsonSerializerSettings
        {
            Converters =
            {
                NodaConverters.DateIntervalConverter,
                NodaConverters.LocalDateConverter
            }
        };
        var interval = new DateInterval(new(2001, 2, 3), new(2004, 5, 6));
        Assert.Equal(JsonConvert.SerializeObject(interval, explicitSettings),
            JsonConvert.SerializeObject(interval, configuredSettings));
    }

    [Fact]
    public void Settings_ConfigureForNodaTime_WithIsoDateIntervalConverter()
    {
        var configuredSettings = new JsonSerializerSettings().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb).WithIsoDateIntervalConverter();
        var explicitSettings = new JsonSerializerSettings
        {
            Converters =
            {
                NodaConverters.IsoDateIntervalConverter
            }
        };
        var interval = new DateInterval(new(2001, 2, 3), new(2004, 5, 6));
        Assert.Equal(JsonConvert.SerializeObject(interval, explicitSettings),
            JsonConvert.SerializeObject(interval, configuredSettings));
    }

    static string Serialize<T>(T value, JsonSerializer serializer)
    {
        var writer = new StringWriter();
        serializer.Serialize(writer, value);
        return writer.ToString();
    }
}