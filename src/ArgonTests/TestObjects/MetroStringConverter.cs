// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class MetroStringConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
#if NET6_0_OR_GREATER
        writer.WriteValue($":::{value.ToString().ToUpper()}:::");
#else
        writer.WriteValue($":::{value.ToString().ToUpper(InvariantCulture)}:::");
#endif
    }

    public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
    {
        var s = (string)reader.Value;

#if NET6_0_OR_GREATER
        return s?.ToLower().Trim(':');
#else
        return s.ToLower(InvariantCulture).Trim(':');
#endif
    }

    public override bool CanConvert(Type type) =>
        type == typeof(string);
}