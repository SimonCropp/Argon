// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class AttachmentReadConverter :
    JsonConverter
{
    public override bool CanConvert(Type type) =>
        type == typeof(System.Net.Mail.Attachment);

    public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
    {
        var info = serializer.Deserialize<AttachmentInfo>(reader);

        var attachment = info != null
            ? new System.Net.Mail.Attachment(new MemoryStream(Convert.FromBase64String(info.ContentBase64)), "application/octet-stream")
            {
                ContentDisposition = { FileName = info.FileName }
            }
            : null;
        return attachment;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
        throw new NotImplementedException();

    class AttachmentInfo
    {
        [JsonProperty(Required = Required.Always)]
        public string FileName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string ContentBase64 { get; set; }
    }
}