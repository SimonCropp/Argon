// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class PersonRaw
{
    [JsonIgnore]
    public Guid InternalId { get; set; }

    [JsonProperty("first_name")]
    public string FirstName { get; set; }

    public JRaw RawContent { get; set; }

    [JsonProperty("last_name")]
    public string LastName { get; set; }
}