// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace TestObjects;

public partial class BusRun
{
    public IEnumerable<DateTime?> Departures { get; set; }
    public Boolean WheelchairAccessible { get; set; }
}