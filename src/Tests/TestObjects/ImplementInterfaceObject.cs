// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class ImplementInterfaceObject : IInterfaceObject
{
    public DateTime InterfaceMember { get; set; }
    public string NewMember { get; set; }

    [JsonProperty(PropertyName = "newMemberWithProperty")]
    public string NewMemberWithProperty { get; set; }
}