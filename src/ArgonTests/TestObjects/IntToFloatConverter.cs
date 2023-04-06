// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class IntToFloatConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
        writer.WriteValue(Convert.ToDouble(value));

    public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer) =>
        Convert.ToInt32(reader.Value);

    public override bool CanConvert(Type type) =>
        type == typeof(int);
}