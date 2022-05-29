// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class NameContainerConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var nameContainer = value as NameContainer;

        if (nameContainer != null)
        {
            writer.WriteValue(nameContainer.Value);
        }
        else
        {
            writer.WriteNull();
        }
    }

    public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
    {
        var nameContainer = new NameContainer
        {
            Value = (string)reader.Value
        };

        return nameContainer;
    }

    public override bool CanConvert(Type type) =>
        type == typeof(NameContainer);
}