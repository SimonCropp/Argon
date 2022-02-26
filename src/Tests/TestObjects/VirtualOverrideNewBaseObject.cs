// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class VirtualOverrideNewBaseObject
{
    [JsonProperty(PropertyName = "virtualMember")]
    public virtual string VirtualMember { get; set; }

    [JsonProperty(PropertyName = "nonVirtualMember")]
    public string NonVirtualMember { get; set; }
}