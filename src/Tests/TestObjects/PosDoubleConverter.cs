// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class PosDoubleConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var p = (PosDouble)value;

        if (p != null)
        {
            writer.WriteRawValue($"new PosD({p.X},{p.Y})");
        }
        else
        {
            writer.WriteNull();
        }
    }

    public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer) =>
        throw new NotImplementedException();

    public override bool CanConvert(Type type) =>
        type == typeof(PosDouble);
}