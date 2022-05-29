// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

[DataContract]
public class DataContractPrivateMembers
{
    public DataContractPrivateMembers()
    {
    }

    public DataContractPrivateMembers(string name, int age, int rank, string title)
    {
        _name = name;
        Age = age;
        Rank = rank;
        Title = title;
    }

    [DataMember]
    string _name;

    [DataMember(Name = "_age")]
    int Age { get; set; }

    [JsonProperty]
    int Rank { get; set; }

    [JsonProperty(PropertyName = "JsonTitle")]
    [DataMember(Name = "DataTitle")]
    string Title { get; set; }

    public string NotIncluded { get; set; }

    public override string ToString() =>
        $"_name: {_name}, _age: {Age}, Rank: {Rank}, JsonTitle: {Title}";
}