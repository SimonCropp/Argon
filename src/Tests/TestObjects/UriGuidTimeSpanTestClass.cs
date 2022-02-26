// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class UriGuidTimeSpanTestClass
{
    public Guid Guid { get; set; }
    public Guid? NullableGuid { get; set; }
    public TimeSpan TimeSpan { get; set; }
    public TimeSpan? NullableTimeSpan { get; set; }
    public Uri Uri { get; set; }
}