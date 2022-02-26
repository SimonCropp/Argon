// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[DataContract]
public class SubWithoutContractVirtualProperties : BaseWithContract
{
    public override string VirtualWithDataMember { get; set; }

    [DataMember(Name = "VirtualSub")]
    public override string Virtual { get; set; }
}