// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class RequiredMembersClass
{
    [JsonProperty(Required = Required.Always)]
    public string FirstName { get; set; }

    [JsonProperty]
    public string MiddleName { get; set; }

    [JsonProperty(Required = Required.AllowNull)]
    public string LastName { get; set; }

    [JsonProperty(Required = Required.Default)]
    public DateTime BirthDate { get; set; }
}