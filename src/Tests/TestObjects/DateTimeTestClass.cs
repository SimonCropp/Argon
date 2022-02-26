// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.ComponentModel;

namespace TestObjects;

public class DateTimeTestClass
{
    public string PreField { get; set; }

    [DefaultValue("")]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public DateTime DateTimeField { get; set; }

    public DateTimeOffset DateTimeOffsetField { get; set; }
    public string PostField { get; set; }
}