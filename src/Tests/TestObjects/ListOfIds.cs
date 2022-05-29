// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class ListOfIds<T> : JsonConverter where T : Bar, new()
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var list = (IList<T>)value;

        writer.WriteStartArray();
        foreach (var item in list)
        {
            writer.WriteValue(item.Id);
        }
        writer.WriteEndArray();
    }

    public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
    {
        var list = new List<T>();

        reader.Read();
        while (reader.TokenType != JsonToken.EndArray)
        {
            var id = (long)reader.Value;

            list.Add(new()
            {
                Id = Convert.ToInt32(id)
            });

            reader.Read();
        }

        return list;
    }

    public override bool CanConvert(Type type) =>
        typeof(IList<T>).IsAssignableFrom(type);
}