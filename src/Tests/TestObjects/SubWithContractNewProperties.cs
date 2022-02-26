// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[DataContract]
public class SubWithContractNewProperties : BaseWithContract
{
#pragma warning disable 108, 114
    [DataMember(Name = "VirtualWithDataMemberSub")]
    public string VirtualWithDataMember { get; set; }

    [DataMember(Name = "Virtual2")]
    public string Virtual { get; set; }

    [DataMember(Name = "WithDataMemberSub")]
    public string WithDataMember { get; set; }

    [DataMember(Name = "JustAProperty2")]
    public string JustAProperty { get; set; }
#pragma warning restore 108, 114
}