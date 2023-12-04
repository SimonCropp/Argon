// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

#if !NET6_0_OR_GREATER
using System.Drawing;

namespace TestObjects;

public class MetroColorConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var color = (Color)value;
        var fixedColor = color == Color.White || color == Color.Black ? color : Color.Gray;

        writer.WriteValue($":::{fixedColor.ToKnownColor().ToString().ToUpper()}:::");
    }

    public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer) =>
        Enum.Parse(typeof(Color), reader.Value.ToString());

    public override bool CanConvert(Type type) =>
        type == typeof(Color);
}
#endif