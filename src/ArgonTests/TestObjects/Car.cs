// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class Car
{
    // included in JSON
    public string Model { get; set; }
    public DateTime Year { get; set; }
    public List<string> Features { get; set; }

    // ignored
    [JsonIgnore]
    public DateTime LastModified { get; set; }
}