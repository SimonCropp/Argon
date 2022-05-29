// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class WidgetIdJsonConverter : JsonConverter
{
    public override bool CanConvert(Type type)
    {
        return type == typeof(WidgetId1) || type == typeof(WidgetId1?);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var id = (WidgetId1)value;
        writer.WriteValue(id.Value.ToString());
    }

    public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }
        return new WidgetId1 { Value = int.Parse(reader.Value.ToString()) };
    }
}