// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class MailAddressReadConverter : JsonConverter
{
    public override bool CanConvert(Type type) =>
        type == typeof(System.Net.Mail.MailAddress);

    public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
    {
        var messageJObject = serializer.Deserialize<JObject>(reader);
        if (messageJObject == null)
        {
            return null;
        }

        var address = messageJObject.GetValue("Address", StringComparison.OrdinalIgnoreCase).ToObject<string>();

        string displayName;
        if (messageJObject.TryGetValue("DisplayName", StringComparison.OrdinalIgnoreCase, out var displayNameToken)
            && !string.IsNullOrEmpty(displayName = displayNameToken.ToObject<string>()))
        {
            return new System.Net.Mail.MailAddress(address, displayName);
        }

        return new System.Net.Mail.MailAddress(address);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
        throw new NotImplementedException();
}