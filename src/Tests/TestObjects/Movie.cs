// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class Movie
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Classification { get; set; }
    public string Studio { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public List<string> ReleaseCountries { get; set; }
}