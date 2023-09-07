// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

// ReSharper disable UnusedParameter.Local
namespace TestObjects;

public class ErroringJsonConverter : JsonConverter
{
    public ErroringJsonConverter(string s)
    {
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
        throw new NotImplementedException();

    public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer) =>
        throw new NotImplementedException();

    public override bool CanConvert(Type type) =>
        throw new NotImplementedException();
}