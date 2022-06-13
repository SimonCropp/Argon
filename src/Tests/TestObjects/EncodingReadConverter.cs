// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class EncodingReadConverter : JsonConverter
{
    public override bool CanConvert(Type type) =>
        typeof(Encoding).IsAssignableFrom(type);

    public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
    {
        var encodingName = serializer.TryDeserialize<string>(reader);
        if (encodingName == null)
        {
            return null;
        }

        return Encoding.GetEncoding(encodingName);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
        throw new NotImplementedException();
}