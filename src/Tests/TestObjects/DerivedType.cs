// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[DataContract]
public class DerivedType : BaseType
{
    [DataMember(Order = 0)]
    public string bird;

    [DataMember(Order = 1)]
    public string parrot;

    [DataMember]
    public string dog;

    [DataMember(Order = 3)]
    public string antelope;

    [DataMember]
    public string cat;

    [JsonProperty(Order = 1)]
    public string albatross;

    [JsonProperty(Order = -2)]
    public string dinosaur;
}