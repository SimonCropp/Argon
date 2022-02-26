// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[DataContract]
public class BaseDataContractWithHidden
{
    [DataMember(Name = "virtualMember")]
    public virtual string VirtualMember { get; set; }

    [DataMember(Name = "nonVirtualMember")]
    public string NonVirtualMember { get; set; }

    public virtual object NewMember { get; set; }
}