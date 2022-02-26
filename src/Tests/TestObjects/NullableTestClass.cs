// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class NullableTestClass
{
    public bool? MyNullableBool { get; set; }
    public int? MyNullableInteger { get; set; }
    public DateTime? MyNullableDateTime { get; set; }
    public DateTimeOffset? MyNullableDateTimeOffset { get; set; }
    public Decimal? MyNullableDecimal { get; set; }
}