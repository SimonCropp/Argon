// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class MetroStringConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
#if !NET5_0_OR_GREATER
        writer.WriteValue($":::{value.ToString().ToUpper(CultureInfo.InvariantCulture)}:::");
#else
        writer.WriteValue($":::{value.ToString().ToUpper()}:::");
#endif
    }

    public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
    {
        var s = (string)reader.Value;
        if (s == null)
        {
            return null;
        }

#if !NET5_0_OR_GREATER
        return s.ToLower(CultureInfo.InvariantCulture).Trim(':');
#else
        return s.ToLower().Trim(':');
#endif
    }

    public override bool CanConvert(Type type) =>
        type == typeof(string);
}