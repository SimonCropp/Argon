// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class ChildDataContractWithHidden :
    BaseDataContractWithHidden
{
    [DataMember(Name = "NewMember")]
    public new virtual string NewMember { get; set; }

    public override string VirtualMember { get; set; }
    public string AddedMember { get; set; }
}