// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.ComponentModel;

namespace TestObjects;

class TypeConverterJsonConverter : JsonConverter
{
    static TypeConverter GetConverter(Type type)
    {
        var converters = ReflectionUtils.GetAttributes(type, typeof(TypeConverterAttribute), true).Union(
            from t in type.GetInterfaces()
            from c in ReflectionUtils.GetAttributes(t, typeof(TypeConverterAttribute), true)
            select c).Distinct();

        return
            (from c in converters
                let converter =
                    (TypeConverter)Activator.CreateInstance(Type.GetType(((TypeConverterAttribute)c).ConverterTypeName))
                where converter.CanConvertFrom(typeof(string))
                      && converter.CanConvertTo(typeof(string))
                select converter)
            .FirstOrDefault();
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var converter = GetConverter(value.GetType());
        var text = converter.ConvertToInvariantString(value);

        writer.WriteValue(text);
    }

    public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
    {
        var converter = GetConverter(type);
        return converter.ConvertFromInvariantString(reader.Value.ToString());
    }

    public override bool CanConvert(Type type) =>
        GetConverter(type) != null;
}