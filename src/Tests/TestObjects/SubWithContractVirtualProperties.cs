// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[DataContract]
public class SubWithContractVirtualProperties : BaseWithContract
{
#pragma warning disable 108, 114
    [DataMember(Name = "VirtualWithDataMemberSub")]
    public virtual string VirtualWithDataMember { get; set; }
#pragma warning restore 108, 114
}