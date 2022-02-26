// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[DataContract]
public class BaseWithContract
{
    [DataMember(Name = "VirtualWithDataMemberBase")]
    public virtual string VirtualWithDataMember { get; set; }

    [DataMember]
    public virtual string Virtual { get; set; }

    [DataMember(Name = "WithDataMemberBase")]
    public string WithDataMember { get; set; }

    [DataMember]
    public string JustAProperty { get; set; }
}