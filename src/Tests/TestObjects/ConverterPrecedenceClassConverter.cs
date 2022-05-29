// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public abstract class ConverterPrecedenceClassConverter : JsonConverter
{
    public abstract string ConverterType { get; }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var c = (ConverterPrecedenceClass)value;

        JToken j = new JArray(ConverterType, c.TestValue);

        j.WriteTo(writer);
    }

    public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
    {
        JToken j = JArray.Load(reader);

        var converter = (string)j[0];
        if (converter != ConverterType)
        {
            throw new($"Serialize converter {converter} and deserialize converter {ConverterType} do not match.");
        }

        var testValue = (string)j[1];
        return new ConverterPrecedenceClass(testValue);
    }

    public override bool CanConvert(Type type) =>
        type == typeof(ConverterPrecedenceClass);
}