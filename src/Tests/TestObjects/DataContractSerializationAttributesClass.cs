// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[DataContract]
public class DataContractSerializationAttributesClass
{
    public string NoAttribute { get; set; }

    [IgnoreDataMember]
    public string IgnoreDataMemberAttribute { get; set; }

    [DataMember]
    public string DataMemberAttribute { get; set; }

    [IgnoreDataMember]
    [DataMember]
    public string IgnoreDataMemberAndDataMemberAttribute { get; set; }
}