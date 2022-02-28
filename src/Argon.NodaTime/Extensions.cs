// Copyright 2012 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace Argon.NodaTime;

/// <summary>
/// Static class containing extension methods to configure Json.NET for Noda Time types.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Configures Json.NET with everything required to properly serialize and deserialize NodaTime data types.
    /// </summary>
    /// <param name="settings">The existing settings to add Noda Time converters to.</param>
    /// <param name="provider">The time zone provider to use when parsing time zones and zoned date/times.</param>
    /// <returns>The original <paramref name="settings"/> value, for further chaining.</returns>
    public static JsonSerializerSettings ConfigureForNodaTime(this JsonSerializerSettings settings, IDateTimeZoneProvider provider)
    {
        // Add our converters
        AddDefaultConverters(settings.Converters, provider);

        // Disable automatic conversion of anything that looks like a date and time to BCL types.
        settings.DateParseHandling = DateParseHandling.None;

        // return to allow fluent chaining if desired
        return settings;
    }

    /// <summary>
    /// Configures Json.NET with everything required to properly serialize and deserialize NodaTime data types.
    /// </summary>
    /// <param name="serializer">The existing serializer to add Noda Time converters to.</param>
    /// <param name="provider">The time zone provider to use when parsing time zones and zoned date/times.</param>
    /// <returns>The original <paramref name="serializer"/> value, for further chaining.</returns>
    public static JsonSerializer ConfigureForNodaTime(this JsonSerializer serializer, IDateTimeZoneProvider provider)
    {
        // Add our converters
        AddDefaultConverters(serializer.Converters, provider);

        // Disable automatic conversion of anything that looks like a date and time to BCL types.
        serializer.DateParseHandling = DateParseHandling.None;

        // return to allow fluent chaining if desired
        return serializer;
    }

    /// <summary>
    /// Configures the given serializer settings to use <see cref="NodaConverters.IsoIntervalConverter"/>.
    /// Any other converters which can convert <see cref="Interval"/> are removed from the serializer.
    /// </summary>
    /// <param name="settings">The existing serializer settings to add Noda Time converters to.</param>
    /// <returns>The original <paramref name="settings"/> value, for further chaining.</returns>
    public static JsonSerializerSettings WithIsoIntervalConverter(this JsonSerializerSettings settings)
    {
        ReplaceExistingConverters<Interval>(settings.Converters, NodaConverters.IsoIntervalConverter);
        return settings;
    }

    /// <summary>
    /// Configures the given serializer to use <see cref="NodaConverters.IsoIntervalConverter"/>.
    /// Any other converters which can convert <see cref="Interval"/> are removed from the serializer.
    /// </summary>
    /// <param name="serializer">The existing serializer to add Noda Time converters to.</param>
    /// <returns>The original <paramref name="serializer"/> value, for further chaining.</returns>
    public static JsonSerializer WithIsoIntervalConverter(this JsonSerializer serializer)
    {
        ReplaceExistingConverters<Interval>(serializer.Converters, NodaConverters.IsoIntervalConverter);
        return serializer;
    }

    /// <summary>
    /// Configures the given serializer settings to use <see cref="NodaConverters.IsoDateIntervalConverter"/>.
    /// Any other converters which can convert <see cref="DateInterval"/> are removed from the serializer.
    /// </summary>
    /// <param name="settings">The existing serializer settings to add Noda Time converters to.</param>
    /// <returns>The original <paramref name="settings"/> value, for further chaining.</returns>
    public static JsonSerializerSettings WithIsoDateIntervalConverter(this JsonSerializerSettings settings)
    {
        ReplaceExistingConverters<DateInterval>(settings.Converters, NodaConverters.IsoDateIntervalConverter);
        return settings;
    }

    /// <summary>
    /// Configures the given serializer to use <see cref="NodaConverters.IsoDateIntervalConverter"/>.
    /// Any other converters which can convert <see cref="DateInterval"/> are removed from the serializer.
    /// </summary>
    /// <param name="serializer">The existing serializer to add Noda Time converters to.</param>
    /// <returns>The original <paramref name="serializer"/> value, for further chaining.</returns>
    public static JsonSerializer WithIsoDateIntervalConverter(this JsonSerializer serializer)
    {
        ReplaceExistingConverters<DateInterval>(serializer.Converters, NodaConverters.IsoDateIntervalConverter);
        return serializer;
    }

    static void AddDefaultConverters(IList<JsonConverter> converters, IDateTimeZoneProvider provider)
    {
        converters.Add(NodaConverters.InstantConverter);
        converters.Add(NodaConverters.IntervalConverter);
        converters.Add(NodaConverters.LocalDateConverter);
        converters.Add(NodaConverters.LocalDateTimeConverter);
        converters.Add(NodaConverters.LocalTimeConverter);
        converters.Add(NodaConverters.AnnualDateConverter);
        converters.Add(NodaConverters.DateIntervalConverter);
        converters.Add(NodaConverters.OffsetConverter);
        converters.Add(NodaConverters.CreateDateTimeZoneConverter(provider));
        converters.Add(NodaConverters.DurationConverter);
        converters.Add(NodaConverters.RoundtripPeriodConverter);
        converters.Add(NodaConverters.OffsetDateTimeConverter);
        converters.Add(NodaConverters.OffsetDateConverter);
        converters.Add(NodaConverters.OffsetTimeConverter);
        converters.Add(NodaConverters.CreateZonedDateTimeConverter(provider));
    }

    static void ReplaceExistingConverters<T>(IList<JsonConverter> converters, JsonConverter newConverter)
    {
        for (var i = converters.Count - 1; i >= 0; i--)
        {
            if (converters[i].CanConvert(typeof(T)))
            {
                converters.RemoveAt(i);
            }
        }
        converters.Add(newConverter);
    }

    /// <summary>
    /// Resolves property name according <see cref="DefaultContractResolver.NamingStrategy"/>.
    /// <para>If serializer is not <see cref="DefaultContractResolver"/> then original <paramref name="propertyName"/> returns.</para>
    /// </summary>
    /// <param name="serializer">The serializer to use name resolve.</param>
    /// <param name="propertyName">Property name.</param>
    /// <returns>Resolved or original property name.</returns>
    internal static string ResolvePropertyName(this JsonSerializer serializer, string propertyName) =>
        (serializer.ContractResolver as DefaultContractResolver)?.GetResolvedPropertyName(propertyName) ?? propertyName;
}