// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
#if !NET5_0_OR_GREATER
[System.ComponentModel.Description("DescriptionAttribute description!")]
#endif
public class Person
{
    // "John Smith"
    [JsonProperty]
    public string Name { get; set; }

    // "2000-12-15T22:11:03"
    [JsonProperty]
    //[JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime BirthDate { get; set; }

    // new Date(976918263055)
    [JsonProperty]
    //[JsonConverter(typeof(JavaScriptDateTimeConverter))]
    public DateTime LastModified { get; set; }

    // not serialized
    public string Department { get; set; }
}