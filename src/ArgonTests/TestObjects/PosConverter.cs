// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class PosConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var p = (Pos) value;
        writer.WriteRawValue($"new Pos({p.X},{p.Y})");
    }

    public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer) =>
        throw new NotImplementedException();

    public override bool CanConvert(Type type) =>
        type == typeof(Pos);
}