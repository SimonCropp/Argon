// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public class Event1
{
    public string EventName { get; set; }
    public string Venue { get; set; }

    public IList<DateTime> Performances { get; set; }
}