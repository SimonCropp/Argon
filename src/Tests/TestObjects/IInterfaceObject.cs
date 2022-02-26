// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public interface IInterfaceObject
{
    [JsonProperty(PropertyName = "virtualMember")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    DateTime InterfaceMember { get; set; }
}